using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Truesight.Parser.Api;
using Truesight.Parser.Impl;
using Truesight.Parser.Impl.Ops;
using Truesight.Parser.Impl.Reader;
using Truesight.TextGenerators.Core;
using XenoGears.Functional;
using Truesight.TextGenerators.Parser.KnowledgeBase;
using NUnit.Framework;
using XenoGears.Assertions;
using XenoGears.Strings;
using AssertionHelper=XenoGears.Assertions.AssertionHelper;
using MethodBody=Truesight.Parser.Impl.MethodBody;

namespace Truesight.TextGenerators.Parser
{
    [TestFixture]
    public partial class Generator
    {
        [Test]
        public void GenerateOpCodeClasses()
        {
            var globalKb = ILOpsKb.Content;

            // todo list for the generator
            // 1) introduce caching for property getters: 
            //    * NB! GENERATE IT rather than wrap around in e.g. ILOpBase
            // 2) same for prefix getters
            // 3) implement the "no." prefix 0xfe19 (CIL spec mentions it, but OpCodes do not)
            // 4) support "readonly." prefix for opcodes it's applicable to

            // first generate enumerations
            var enumTypes = new[] {typeof(OperatorType), typeof(PredicateType)};
            foreach (var t_enum in enumTypes)
            {
                // so far we need only enums
                t_enum.IsEnum.AssertTrue();

                Helpers.GenerateIntoClass(
                    @"..\..\..\Truesight\Parser\Api\Ops\" + t_enum.Name + ".cs",
                    "Truesight.Parser.Api.Ops",
                    "public enum " + t_enum.Name,
                    buffer => 
                    {
                        var values = Enum.GetValues(t_enum).Cast<Object>();
                        var s_values = values.Select(v => v.ToInvariantString().Indent().Indent());
                        buffer.Append(s_values.StringJoin("," + Environment.NewLine));
                    });
            }

            // second, set up redirections from enums to generated enums
            var typeRedirections = enumTypes.ToDictionary(
                t => t.GetCSharpRef(ToCSharpOptions.ForCodegen),
                t => "Truesight.Parser.Api.Ops." + t.Name);

            // third, generate the IILOpType enumeration
            Helpers.GenerateIntoClass(
                @"..\..\..\Truesight\Parser\Api\IILOpType.cs",
                "Truesight.Parser.Api",
                "public enum IILOpType",
                buffer => buffer.Append(globalKb.Select(fkb => "        " + fkb.Name.Capitalize()).StringJoin("," + Environment.NewLine)));

            // now generate opcode classes
            foreach (var fkb in globalKb)
            {
                var isIgnored = fkb.SubSpecs.Values.Any(kb => kb.Tags.Contains("Ignore"));
                (isIgnored && fkb.SubSpecs.Count > 1).AssertFalse();

                var isPrefix = fkb.SubSpecs.Values.Any(kb => kb.Tags.Contains("Prefix"));
                (isPrefix && fkb.SubSpecs.Count > 1).AssertFalse();

                var className = fkb.Name.Capitalize();
                fkb.OpCodes.AssertNotEmpty();
                var opCodesComment = "// " + 
                    fkb.OpCodes.Select(opcode => opcode.Name).StringJoin();
                var opcodesAttribute = String.Format("[{0}({1})]",
                    typeof(OpCodesAttribute).GetCSharpRef(ToCSharpOptions.ForCodegen),
                    fkb.OpCodes.Select(opcode => opcode.GetCSharpByteSequence()).StringJoin());
                var debuggerNonUserCodeAttribute = String.Format("[{0}]",
                    typeof(DebuggerNonUserCodeAttribute).GetCSharpRef(ToCSharpOptions.ForCodegen));
                var classDeclaration =
                    opCodesComment + Environment.NewLine + "    " +
                    opcodesAttribute + Environment.NewLine + "    " +
                    debuggerNonUserCodeAttribute + Environment.NewLine + "    " +
                    (isIgnored ? "internal" : "public") + " sealed class " + className + 
                    " : " + typeof(ILOp).GetCSharpRef(ToCSharpOptions.ForCodegen);

                Helpers.GenerateIntoClass(
                    @"..\..\..\Truesight\Parser\Api\Ops\" + className + ".cs",
                    "Truesight.Parser.Api.Ops",
                    classDeclaration,
                    buffer =>
                    {
                        var fields = fkb.SubSpecs.Values
                            .SelectMany(spec => spec.Fields.Values.Select(f => f.Name)).Distinct().Order()
                            .Select(fname => UniteSubSpecsForField(fkb, fname)).ToArray();
                        if (fkb.Name == "branch") fields = fields.Reverse().ToArray();
                        var props = fkb.SubSpecs.Values
                            .SelectMany(spec => spec.Props.Values.Select(p => p.Name)).Distinct().Order()
                            .Select(pname => UniteSubSpecsForProp(fkb, pname)).ToArray();
                        var prefixes = fkb.SubSpecs.Values
                            .SelectMany(spec => spec.Prefixes.Values.Select(p => p.Name)).Distinct().Order()
                            .Select(pname =>
                            {
                                var sample = fkb.SubSpecs.Values.First().Prefixes[pname];
                                fkb.SubSpecs.Values.AssertAll(kb =>
                                {
                                    var prefix = kb.Prefixes[pname];
                                    (prefix.Name == sample.Name).AssertTrue();
                                    (prefix.Type == sample.Type).AssertTrue();
                                    (prefix.PrefixName == sample.PrefixName).AssertTrue();
                                    (prefix.Getter == sample.Getter).AssertTrue();
                                    (prefix.Setter == sample.Setter).AssertTrue();
                                    // setters ain't supported since they weren't necessary
                                    prefix.Setter.AssertNullOrEmpty();
                                    return true;
                                });
                                return sample;
                            }).ToArray();

                        // 0. Generate OpCodeType
                        buffer.AppendFormat("public override {0} OpType {{ get {{ return {0}.{1}; }} }}".Indent().Indent(),
                            typeof(IILOpType).GetCSharpRef(ToCSharpOptions.ForCodegen), className).AppendLine().AppendLine();

                        // 1. Field declarations
                        foreach (var field in fields)
                        {
                            field.IsUnsafe.AssertFalse();
                            buffer.AppendLine(String.Format("private readonly {0} {1};",
                                field.Type.GetCSharpRef(ToCSharpOptions.ForCodegen), field.Name).Indent().Indent());
                        }
                        if (fields.Any()) buffer.AppendLine();

                        // 2. Constructor (parsing and field init)
                        var auxCtorHeadline = String.Format("internal {0}({1} source, {2} reader)",
                            className,
                            typeof(MethodBody).GetCSharpRef(ToCSharpOptions.ForCodegen),
                            typeof(BinaryReader).GetCSharpRef(ToCSharpOptions.ForCodegen));
                        buffer.AppendLine(auxCtorHeadline.Indent().Indent());
                        var emptyPrefixes = String.Format("{0}.ToReadOnly({1}.Empty<{2}>())",
                            typeof(EnumerableExtensions).GetCSharpRef(ToCSharpOptions.ForCodegen),
                            typeof(Enumerable).GetCSharpRef(ToCSharpOptions.ForCodegen),
                            typeof(ILOp).GetCSharpRef(ToCSharpOptions.ForCodegen));
                        buffer.AppendLine((": this(source, reader, " + emptyPrefixes + ")").Indent().Indent().Indent());
                        buffer.AppendLine("{".Indent().Indent());
                        buffer.AppendLine("}".Indent().Indent());
                        buffer.AppendLine();

                        var mainCtorHeadline = String.Format("{0}, {1} prefixes)",
                            auxCtorHeadline.Slice(0, -1),
                            typeof(ReadOnlyCollection<ILOp>).GetCSharpRef(ToCSharpOptions.ForCodegen));
                        buffer.AppendLine(mainCtorHeadline.Indent().Indent());
                        buffer.Append(": base(source, AssertSupportedOpCode(reader), ".Indent().Indent().Indent());
                        // note. be wary of offset magic here!
                        buffer.AppendFormat(
                            "({0})reader.BaseStream.Position - " +
                            "{1}.Sum({1}.Select(prefixes ?? {2}, prefix => prefix.Size))",
                            typeof(Int32).GetCSharpRef(ToCSharpOptions.ForCodegen),
                            typeof(Enumerable).GetCSharpRef(ToCSharpOptions.ForCodegen),
                            emptyPrefixes);
                        buffer.AppendLine(", prefixes ?? " + emptyPrefixes + ")");
                        buffer.AppendLine("{".Indent().Indent());
                        buffer.AppendLine("// this is necessary for further verification".Indent().Indent().Indent());
                        buffer.AppendLine("var origPos = reader.BaseStream.Position;".Indent().Indent().Indent());
                        buffer.AppendLine();
                        fields.ForEach(field =>
                        {
                            buffer.AppendLine(("// initializing " + field.Name).Indent().Indent().Indent());
                            buffer.AppendLine(field.Initializer.Indent().Indent().Indent());
                        });
                        buffer.AppendLine("// verify that we've read exactly the amount of bytes we should".Indent().Indent().Indent());
                        buffer.AppendLine("var bytesRead = reader.BaseStream.Position - origPos;".Indent().Indent().Indent());
                        // this validation is partially redundant for switch, tho I'm cba to invent something better now
                        buffer.AppendLine(String.Format("{0}.AssertTrue(bytesRead == SizeOfOperand);",
                            typeof(AssertionHelper).GetCSharpRef(ToCSharpOptions.ForCodegen)).Indent().Indent().Indent());
                        buffer.AppendLine();
                        buffer.AppendLine("// now when the initialization is completed verify that we've got only prefixes we support".Indent().Indent().Indent());
                        buffer.AppendLine(String.Format("{0}.AssertAll(Prefixes, prefix => ".Indent().Indent().Indent(),
                            typeof(AssertionHelper).GetCSharpRef(ToCSharpOptions.ForCodegen)));
                        buffer.AppendLine("{".Indent().Indent().Indent());
                        var cond_vars = new List<String>();
                        foreach (var prefix in prefixes)
                        {
                            var var_name = prefix.PrefixName + "_ok";
                            cond_vars.Add(var_name);
                            buffer.AppendLine(String.Format("var {0} = prefix is {1}{2};".Indent().Indent().Indent().Indent(),
                                var_name, prefix.PrefixName.Capitalize(),
                                prefix.Filter.IsNullOrEmpty() ? "" : " && " + prefix.Filter));
                        }
                        buffer.AppendLine(String.Format("return {0};".Indent().Indent().Indent().Indent(),
                            cond_vars.Concat("false").StringJoin(" || ")));
                        buffer.AppendLine("});".Indent().Indent().Indent());
                        buffer.AppendLine("}".Indent().Indent());
                        buffer.AppendLine();

                        // 3. OpCode inference
                        buffer.AppendLine(String.Format("private static {0} AssertSupportedOpCode({1} reader)",
                            typeof(OpCode).GetCSharpRef(ToCSharpOptions.ForCodegen),
                            typeof(BinaryReader).GetCSharpRef(ToCSharpOptions.ForCodegen)).Indent().Indent());
                        buffer.AppendLine("{".Indent().Indent());

                        buffer.AppendLine(String.Format(
                            "var opcode = {0}.ReadOpCode(reader);",
                            typeof(OpCodeReader).GetCSharpRef(ToCSharpOptions.ForCodegen)).Indent().Indent().Indent());
                        buffer.AppendLine(String.Format(
                            "{0}.AssertNotNull(opcode);",
                            typeof(AssertionHelper).GetCSharpRef(ToCSharpOptions.ForCodegen)).Indent().Indent().Indent());
                        buffer.AppendLine(opCodesComment.Indent().Indent().Indent());
                        buffer.AppendLine(String.Format(
                            "{0}.AssertTrue({1}.Contains(new {2}[]{{{3}}}, ({4})opcode.Value.Value));",
                            typeof(AssertionHelper).GetCSharpRef(ToCSharpOptions.ForCodegen),
                            typeof(Enumerable).GetCSharpRef(ToCSharpOptions.ForCodegen),
                            typeof(UInt16).GetCSharpRef(ToCSharpOptions.ForCodegen),
                            fkb.OpCodes.Select(opcode => opcode.GetCSharpByteSequence()).StringJoin(),
                            typeof(UInt16).GetCSharpRef(ToCSharpOptions.ForCodegen))
                            .Indent().Indent().Indent());

                        buffer.AppendLine();
                        buffer.AppendLine("return opcode.Value;".Indent().Indent().Indent());
                        buffer.AppendLine("}".Indent().Indent());
                        buffer.AppendLine();

                        // 4. SizeOfOperands override (special case for Switch)
                        if (fkb.OpCodes.SingleOrDefault2() == OpCodes.Switch)
                        {
                            buffer.AppendLine();
                            buffer.AppendLine(String.Format("public override {0} SizeOfOperand",
                                typeof(Int32).GetCSharpRef(ToCSharpOptions.ForCodegen)).Indent().Indent());
                            buffer.AppendLine("{".Indent().Indent());
                            buffer.AppendLine("get".Indent().Indent().Indent());
                            buffer.AppendLine("{".Indent().Indent().Indent());
                            buffer.AppendLine(String.Format("return sizeof({0}) + _targetOffsets.Count * sizeof({0});",
                                 typeof(Int32).GetCSharpRef(ToCSharpOptions.ForCodegen)).Indent().Indent().Indent().Indent());
                            buffer.AppendLine("}".Indent().Indent().Indent());
                            buffer.AppendLine("}".Indent().Indent());
                        }

                        // 5. Property declarations
                        if (props.IsNotEmpty()) buffer.AppendLine();
                        foreach (var prop in props)
                        {
                            buffer.AppendLine(String.Format("{0}public {1} {2}",
                                prop.IsUnsafe ? "unsafe " : "", 
                                prop.Type.GetCSharpRef(ToCSharpOptions.ForCodegen), 
                                prop.Name).Indent().Indent());

                            buffer.AppendLine("{".Indent().Indent());
                            buffer.AppendLine("get".Indent().Indent().Indent());
                            buffer.AppendLine("{".Indent().Indent().Indent());

                            var getter = prop.Getter.TrimEnd();
                            buffer.AppendLine(getter.Indent().Indent().Indent().Indent());
                            prop.Setter.AssertNullOrEmpty();

                            buffer.AppendLine("}".Indent().Indent().Indent());
                            buffer.AppendLine("}".Indent().Indent());
                            buffer.AppendLine();
                        }

                        // 6. Prefix declarations
                        if (props.IsEmpty() && prefixes.IsNotEmpty()) buffer.AppendLine();
                        foreach (var prefix in prefixes)
                        {
                            buffer.AppendLine(String.Format("{0}public {1} {2}",
                                prefix.IsUnsafe ? "unsafe " : "",
                                prefix.Type.GetCSharpRef(ToCSharpOptions.ForCodegen),
                                prefix.Name).Indent().Indent());

                            buffer.AppendLine("{".Indent().Indent());
                            buffer.AppendLine("get".Indent().Indent().Indent());
                            buffer.AppendLine("{".Indent().Indent().Indent());

                            prefix.Setter.AssertNullOrEmpty();
                            if (prefix.Getter == null)
                            {
                                var getter = String.Format(
                                    "return {0}.Any({0}.OfType<{1}>(Prefixes));",
                                    typeof(Enumerable).GetCSharpRef(ToCSharpOptions.ForCodegen),
                                    prefix.PrefixName.Capitalize());
                                buffer.AppendLine(getter.Indent().Indent().Indent().Indent());
                            }
                            else
                            {
                                var getter = String.Format(
                                    "return {0}.Single({0}.OfType<{1}>(Prefixes)).{2};",
                                    typeof(Enumerable).GetCSharpRef(ToCSharpOptions.ForCodegen),
                                    prefix.PrefixName.Capitalize(),
                                    prefix.Getter);
                                buffer.AppendLine(getter.Indent().Indent().Indent().Indent());
                            }

                            buffer.AppendLine("}".Indent().Indent().Indent());
                            buffer.AppendLine("}".Indent().Indent());
                            buffer.AppendLine();
                        }

                        // 7. Generate the stringify routine
                        if (props.IsEmpty()) buffer.AppendLine();
                        buffer.AppendLine(String.Format("public override {0} ToString()",
                            typeof(String).GetCSharpRef(ToCSharpOptions.ForCodegen)).Indent().Indent());
                        buffer.AppendLine("{".Indent().Indent());

                        // Part 1. Offset (for non-prefix only)
                        buffer.Append("var offset = ".Indent().Indent().Indent());
                        if (isPrefix) buffer.AppendLine("\"\"; // prefixes never get printed in standalone mode so nothing here");
                        if (!isPrefix) buffer.AppendLine("OffsetToString(Offset) + \":\";");

                        // Part 2. Prefixes (wrapped in brackets if any)
                        buffer.Append("var prefixSpec = ".Indent().Indent().Indent());
                        buffer.Append("Prefixes.Count == 0 ? \"\" : (\"[\" + ");
                        buffer.Append(String.Format("{0}.StringJoin(Prefixes)",
                            typeof(EnumerableExtensions).GetCSharpRef(ToCSharpOptions.ForCodegen)));
                        buffer.AppendLine(" + \"]\");");

                        // Part 3. Name (as simple as that lol)
                        var tos_name = fkb.Name == "operator" ? "OperatorTypeToString(OperatorType)" : ("\"" + fkb.Name + "\"");
                        buffer.AppendLine(("var name =  " + tos_name + ";").Indent().Indent().Indent());

                        var mods = new List<String>();
                        String operand = null;

                        // Time for some inference before we continue
                        // note. this stuff is hardcoded (see Visualizer.PrintOutILOpsWithProps for more info)
                        if (fkb.Name == "ldc")
                        {
                            mods.Add("Type == null || OpSpec.OpCode.Value == 0xd0 /*ldtoken*/ ? null : TypeToString(Type)");
                            operand = "ObjectToCSharpLiteral(Value)";
                        }
                        else if (fkb.Name == "cast")
                        {
                            mods.Add("ExpectsUn ? \"un\" : \"\"");
                            mods.Add("FailsOnOverflow ? \"ovf\" : \"\"");
                            mods.Add(String.Format("{0}.Format(\"{{0}}->{{1}}\", " + 
                                "ExpectsRefOrVal ? \"refval\" : (ExpectsRef ? \"ref\" : (ExpectsVal ? \"val\" : \"???\")), " +
                                "YieldsRefOrVal ? \"refval\" : (YieldsRef ? \"ref\" : (YieldsVal ? \"val\" : \"???\"))" + ")",
                                typeof(String).GetCSharpRef(ToCSharpOptions.ForCodegen)));
                            operand = "(Type != null ? TypeToString(Type) : null)";
                        }
                        else if (fkb.Name == "call")
                        {
                            mods.Add("IsVirtual ? \"virt\" : \"\"");
                            operand = "(Method != null ? MethodBaseToString(Method) : null)";
                        }
                        else if (fkb.Name == "operator")
                        {
                            mods.Add("ExpectsUn ? \"un\" : \"\"");
                            mods.Add("FailsOnOverflow ? \"ovf\" : \"\"");
                        }
                        else if (fkb.Name == "branch")
                        {
                            mods.Add("ExpectsUn ? \"un\" : \"\"");
                            mods.Add("PredicateTypeToString(PredicateType)");
                            operand = "OffsetToString(_absoluteTargetOffset)";
                        }
                        else if (fkb.Name == "new")
                        {
                            operand = "(Ctor != null ? ConstructorInfoToString(Ctor) : (Type != null ? TypeToString(Type) : null))";
                        }
                        else if (fkb.Name == "ldftn")
                        {
                            mods.Add("IsVirtual ? \"virt\" : \"\"");
                            operand = "(Method != null ? MethodBaseToString(Method) : null)";
                        }
                        else if (fkb.Name == "compare")
                        {
                            mods.Add("ExpectsUn ? \"un\" : \"\"");
                            mods.Add("PredicateTypeToString(PredicateType)");
                        }
                        else if (fkb.Name == "switch")
                        {
                            operand = "OffsetsToString(AbsoluteTargetOffsets)";
                        }
                        else if (fkb.Name == "calli")
                        {
                            operand = "Sig != null ? ByteArrayToString(Sig) : null";
                        }
                        else if (fkb.Name == "ldarg" || fkb.Name == "ldarga" || fkb.Name == "starg" ||
                            fkb.Name == "ldloc" || fkb.Name == "ldloca" || fkb.Name == "stloc")
                        {
                            props.AssertCount(2);
                            var index = props.Single(p => p.Name == "Index");
                            var other = props.Except(index).Single();

                            operand = String.Format("{2} != null ? "+
                                "{1}ToString({2}) : " + 
                                "{0}.ToString()",
                                index.Name,
                                other.Type.Name,
                                other.Name);
                        }
                        else if (props.Any(p => p.Name.Contains("Token")))
                        {
                            PropertySpec token, resolved;
                            if (props.Count() == 2)
                            {
                                token = props.Single(p => p.Name.Contains("Token"));
                                resolved = props.Except(token).SingleOrDefault2();
                            }
                            else
                            {
                                if (fkb.Name == "ldind" || fkb.Name == "stind")
                                {
                                    token = props.Single(p => p.Name == "TypeToken");
                                    resolved = props.Single(p => p.Name == "Type");
                                }
                                else if (fkb.Name == "ldfld" || fkb.Name == "stfld" || fkb.Name == "ldflda")
                                {
                                    token = props.Single(p => p.Name == "FieldToken");
                                    resolved = props.Single(p => p.Name == "Field");
                                }
                                else
                                {
                                    throw AssertionHelper.Fail();
                                }
                            }

                            operand = String.Format("{0}ToString({1})",
                                resolved.Type.Name,
                                resolved.Name);
                            if (!resolved.Type.IsValueType)
                            {
                                operand = String.Format("({1} != null ? {0} : null)",
                                    operand, resolved.Name);
                            }
                        }
                        else if (fkb.Name == "initblk" || fkb.Name == "cpblk")
                        {
                            // do nothing
                        }
                        else if (props.IsNotEmpty())
                        {
                            props.AssertCount(1);

                            operand = String.Format("{0}ToString({1})",
                                props.Single().Type.Name,
                                props.Single().Name);
                            if (!props.Single().Type.IsValueType)
                            {
                                operand = String.Format("({1} != null ? {0} : null)",
                                    operand, props.Single().Name);
                            }
                        }

                        // this hack is necessary for token-related ops
                        // not to crash when the module is left unspecified
                        if (props.Any(p => p.Name.Contains("Token")))
                        {
                            String tokenExpr; 
                            if (fkb.Name == "new")
                            {
                                var rawTokenExpr = "(_ctorToken ?? _typeToken).Value";
                                rawTokenExpr = String.Format("(\"0x\" + {0}.ToString(\"x8\"))", rawTokenExpr);

                                tokenExpr = "(OpSpec.OpCode.Value == 0x8d /*newarr*/ ? \"arr of \" : \"\") + ";
                                tokenExpr = "(" + tokenExpr + rawTokenExpr + ")";
                            }
                            else if (fkb.Name == "cast")
                            {
                                var token = props.Single(p => p.Name.Contains("Token"));
                                var rawTokenExpr = String.Format("(\"0x\" + {0}.ToString(\"x8\"))", token.Name);
                                tokenExpr = String.Format("(_typeToken == 0 ? {0} : {1})", operand, rawTokenExpr);
                            }
                            else if (fkb.Name == "call")
                            {
                                var calliExpr = "(Signature != null ? ByteArrayToString(Signature) : (\"0x\" + _signatureToken.ToString(\"x8\")))";
                                var callExpr = "(\"0x\" + _methodToken.ToString(\"x8\"))";
                                tokenExpr = String.Format("(OpSpec.OpCode.Value == 0x29 /*calli*/ ? {0} : {1})", calliExpr, callExpr);
                            }
                            else
                            {
                                var token = props.Single(p => p.Name.Contains("Token"));
                                tokenExpr = String.Format("(\"0x\" + {0}.ToString(\"x8\"))", token.Name);
                            }

                            operand = String.Format("({0} ?? ({1}))", operand, tokenExpr);
                        }

                        // Part 4. Mods (e.g. Un, Ovf and likes)
                        buffer.AppendLine(String.Format("var mods = new {0}();",
                            typeof(List<String>).GetCSharpRef(ToCSharpOptions.ForCodegen)).Indent().Indent().Indent());
                        mods.ForEach(mod => buffer.AppendLine(String.Format(
                            "mods.Add({0});", mod).Indent().Indent().Indent()));
                        buffer.AppendLine(String.Format("var modSpec = {0}.StringJoin({1}.Where(mods, mod => {2}.IsNeitherNullNorEmpty(mod)), \", \");",
                            typeof(EnumerableExtensions).GetCSharpRef(ToCSharpOptions.ForCodegen),
                            typeof(Enumerable).GetCSharpRef(ToCSharpOptions.ForCodegen),
                            typeof(EnumerableExtensions).GetCSharpRef(ToCSharpOptions.ForCodegen)).Indent().Indent().Indent());

                        // Part 5. Operand (something to be displayed near the opcode)
                        buffer.AppendLine(String.Format("var operand = {0};",
                            operand.IsNullOrEmpty() ? "\"\"" : operand).Indent().Indent().Indent());

                        // Now assemble the stringified view
                        buffer.AppendLine();
                        buffer.AppendLine("var parts = new []{offset, prefixSpec, name, modSpec, operand};".Indent().Indent().Indent());
                        buffer.AppendLine(String.Format(
                            "var result = {0}.StringJoin({1}.Where(parts, p => {2}.IsNeitherNullNorEmpty(p)), \" \");",
                            typeof(EnumerableExtensions).GetCSharpRef(ToCSharpOptions.ForCodegen),
                            typeof(Enumerable).GetCSharpRef(ToCSharpOptions.ForCodegen),
                            typeof(EnumerableExtensions).GetCSharpRef(ToCSharpOptions.ForCodegen)).Indent().Indent().Indent());
                        buffer.AppendLine();
                        buffer.AppendLine("return result;".Indent().Indent().Indent());

                        buffer.AppendLine("}".Indent().Indent());

                        // Fixup the last eoln in the class (added by the TextGeneratedIntoClass.template)
                        var eolnLen = Environment.NewLine.Length;
                        if (buffer.ToString(buffer.Length - eolnLen, eolnLen) == Environment.NewLine)
                            buffer.Remove(buffer.Length - eolnLen, eolnLen);

                        // Finally, don't forget to redirect from local enums to generated classes
                        typeRedirections.ForEach(redir => buffer.Replace(redir.Key, redir.Value));
                    });
            }
        }

        private FieldSpec UniteSubSpecsForField(OpCodeFamilyKb fkb, String fname)
        {
            // 1. Create a field spec
            var fspecs = fkb.SubSpecs.ToDictionary(kvp => kvp.Key, kvp => 
                kvp.Value.Fields.Values.SingleOrDefault(f => f.Name == fname) ?? new FieldSpec{Name = fname});
            var united = new FieldSpec();
            united.Name = fname;

            // 2. Find out an unified type (it should be specified at least once and every time the same)
            var ftypes = fspecs.Values.Select(v => v.Type);
            var ftype = ftypes.Where(v => v != null).Distinct().Single();
            united.Type = ftype;

            // 3. Normalize initializers for every opcode (replace null with default(T))
            foreach (var spec in fspecs.Values)
            {
                var init = spec.Initializer;
                if (init.IsNullOrEmpty())
                {
                    spec.Initializer = "default(" + ftype.GetCSharpRef(ToCSharpOptions.ForCodegen) + ")";
                }
            }

            // 4. Now assemble all initializers into a single language construct
            var initGroups = fspecs.Where(kvp => kvp.Value.Initializer.IsNeitherNullNorEmpty())
                .GroupBy(kvp => kvp.Value.Initializer, kvp => kvp.Key);
            (initGroups.Count() >= 1).AssertTrue();

            // 4.1. There's only 1 possible opcode => 1 possible initializer
            // or maybe all initializers for different opcodes are the same.
            // This means that we can reuse its expression and assign the value to the field. EZ!
            if (initGroups.Count() == 1)
            {
                var initCode = initGroups.Single().Key;
                if (!initCode.EndsWith(";")) initCode = initCode + ";";
                initCode = fname + " = " + initCode;
                united.Initializer = (initCode + Environment.NewLine);
            }
            // 4.2. There are multiple distinct initializers
            // Thus, we're forced to implement a switch depending on the opcode.
            // Default will crash the switch since we're unprepared to it.
            else
            {
                var init = new StringBuilder();
                init.AppendLine(String.Format("switch(({0})OpSpec.OpCode.Value)",
                    typeof(UInt16).GetCSharpRef(ToCSharpOptions.ForCodegen)));
                init.AppendLine("{");

                foreach (var initGroup in initGroups)
                {
                    foreach (var opcode in initGroup)
                    {
                        var @case = "case " + opcode.GetCSharpByteSequence() + ":";
                        @case = @case + " //" + opcode.Name;
                        init.AppendLine(@case.Indent());
                    }

                    var initCode = initGroup.Key;
                    if (!initCode.EndsWith(";")) initCode = initCode + ";";
                    initCode = fname + " = " + initCode;
                    init.AppendLine(initCode.Indent().Indent());
                    init.AppendLine("break;".Indent().Indent());
                }

                init.AppendLine("default:".Indent());
                var throwOnUnknown = String.Format("throw {0}.Fail();", 
                    typeof(AssertionHelper).GetCSharpRef(ToCSharpOptions.ForCodegen));
                init.AppendLine(throwOnUnknown.Indent().Indent());

                init.AppendLine("}");
                united.Initializer = init.ToString();
            }

            return united;
        }

        private PropertySpec UniteSubSpecsForProp(OpCodeFamilyKb fkb, String pname)
        {
            // 1. Create a property spec
            var pspecs = fkb.SubSpecs.ToDictionary(kvp => kvp.Key, kvp => 
                kvp.Value.Props.Values.SingleOrDefault(f => f.Name == pname) ?? new PropertySpec{Name = pname});
            var united = new PropertySpec();
            united.Name = pname;

            // 2. Find out an unified type (it should be specified at least once and every time the same)
            var ptypes = pspecs.Values.Select(v => v.Type);
            var ptype = ptypes.Where(v => v != null).Distinct().Single();
            united.Type = ptype;

            // 3. Normalize getters (setters ain't supported) for every opcode (replace null with default(T))
            foreach (var spec in pspecs.Values)
            {
                var init = spec.Getter;
                if (init.IsNullOrEmpty())
                {
                    spec.Getter = "default(" + ptype.GetCSharpRef(ToCSharpOptions.ForCodegen) + ")";
                }
            }

            // 4. Now assemble all initializers into a single language construct (setters ain't supported)!
            var getters = pspecs.Where(kvp => kvp.Value.Getter.IsNeitherNullNorEmpty())
                .GroupBy(kvp => kvp.Value.Getter, kvp => kvp.Key);
            (getters.Count() >= 1).AssertTrue();

            // 4.1. There's only 1 possible opcode => 1 possible getter
            // or maybe all getters for different opcodes are the same.
            // This means that we can reuse its expression and just return it. EZ!
            if (getters.Count() == 1)
            {
                var getterCode = getters.Single().Key;
                if (!getterCode.EndsWith(";")) getterCode = getterCode + ";";
                if (!getterCode.StartsWith("return") && !getterCode.Contains(Environment.NewLine)) 
                    getterCode = "return " + getterCode;
                united.Getter = getterCode;
            }
            // 4.2. There are multiple distinct getters
            // Thus, we're forced to implement a switch depending on the opcode.
            // Default will crash the switch since we're unprepared to it.
            else
            {
                var getter = new StringBuilder();
                getter.AppendLine(String.Format("switch(({0})OpSpec.OpCode.Value)",
                    typeof(UInt16).GetCSharpRef(ToCSharpOptions.ForCodegen)));
                getter.AppendLine("{");

                foreach (var getterClause in getters)
                {
                    foreach (var opcode in getterClause)
                    {
                        var @case = "case " + opcode.GetCSharpByteSequence() + ":";
                        @case = @case + " //" + opcode.Name;
                        getter.AppendLine(@case.Indent());
                    }

                    var getterCode = getterClause.Key;
                    if (!getterCode.EndsWith(";")) getterCode = getterCode + ";";
                    if (!getterCode.StartsWith("return") && !getterCode.Contains(Environment.NewLine)) 
                        getterCode = "return " + getterCode;
                    getter.AppendLine(getterCode.Indent().Indent());
                }

                getter.AppendLine("default:".Indent());
                var throwOnUnknown = String.Format("throw {0}.Fail();",
                    typeof(AssertionHelper).GetCSharpRef(ToCSharpOptions.ForCodegen));
                getter.AppendLine(throwOnUnknown.Indent().Indent());

                getter.AppendLine("}");
                united.Getter = getter.ToString();
            }

            // 5. Setters ain't supported since they weren't necessary
            var setters = pspecs.Where(kvp => kvp.Value.Setter.IsNeitherNullNorEmpty())
                .GroupBy(kvp => kvp.Value.Setter, kvp => kvp.Key);
            setters.AssertCount(0);

            return united;
        }
    }
}
