using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using Truesight.Parser.Api;
using Truesight.Parser.Api.Ops;
using Truesight.Parser.Impl;
using Truesight.Parser.Impl.Ops;
using XenoGears.Assertions;
using XenoGears;
using XenoGears.Strings;

namespace Truesight.TextGenerators.Parser.KnowledgeBase
{
    public static class ILOpsKb
    {
        private static Dictionary<String, OpCodeFamilyKb> _kb = new Dictionary<String, OpCodeFamilyKb>();
        public static ReadOnlyCollection<OpCodeFamilyKb> Content { get { return _kb.Values.ToReadOnly(); } }

        static ILOpsKb()
        {
            OpCodeReference.AllOpCodes.ForEach(PopulateKb);
        }

        private static void PopulateKb(OpCode opcode)
        {
            // n0te. names of OpCode instances have parts delimited by periods
            // however OpCodes static fields have name parts delimited by underscores

            (opcode.OpCodeType == OpCodeType.Nternal).AssertFalse();
            var parts = opcode.Name.Split(".".MkArray(), StringSplitOptions.None);
            if (opcode.Name == "unbox.any") parts = opcode.Name.MkArray();

            var pp_parts = InferFamily(parts);
            var family = pp_parts.First();
            var tags = pp_parts.Skip(1);

            var spec = _kb.GetOrCreate(family, k => new OpCodeFamilyKb(k));
            spec.SubSpecs.Add(opcode, new OpCodeKb(spec, opcode));
            var subspec = spec.SubSpecs[opcode];

            InferFromOpcode(subspec);
            tags.ForEach(tag => subspec.Tags.Add(tag));
            tags.ForEach(tag => InferFromTag(subspec, tag));
        }

        private static IEnumerable<String> InferFamily(IEnumerable<String> parts)
        {
            var family = parts.First();
            if (family == "ldnull")
            {
                yield return "ldc";
                yield return "null";
            }
            else if (family == "ldstr")
            {
                yield return "ldc";
                yield return "str";
            }
            else if (family == "ldtoken")
            {
                yield return "ldc";
                yield return "token";
            }
            else if (family == "callvirt" || family == "ldvirtftn")
            {
                yield return family.Replace("virt", "");
                yield return "virt";
            }
            else if (family == "ldsfld" || family == "stsfld" || family == "ldsflda")
            {
                yield return family.Replace("sfld", "fld");
            }
            else if (family == "conv" || family == "castclass" || 
                family == "box" || family == "unbox" || family == "unbox.any")
            {
                yield return "cast";

                if (family == "conv")
                {
                    yield return "expects.val";
                    yield return "yields.val";
                }
                else if (family == "castclass")
                {
                    yield return "expects.ref";
                    yield return "yields.ref";
                }
                else if (family == "box")
                {
                    yield return "expects.val";
                    yield return "yields.ref";
                }
                else if (family == "unbox")
                {
                    yield return "expects.ref";
                    yield return "yields.val";
                }
                else if (family == "unbox.any")
                {
                    yield return "expects.ref.or.val";
                    yield return "yields.ref.or.val";
                }
            }
            else if (family == "newobj" || family == "initobj" || family == "newarr")
            {
                yield return "new";

                var tag = "instantiates.";
                if (family == "newobj") tag += "ref";
                if (family == "initobj") tag += "val";
                if (family == "newarr") tag += "arr";
                yield return tag;
            }
            else if (family == "br" || family == "leave")
            {
                yield return "branch";
                yield return family;
            }
            else if (family == "brfalse" || family == "brtrue")
            {
                yield return "branch";
                yield return PredicateSpecFromFamily(family.Substring(2));
            }
            else if (family == "beq" || family == "blt" || family == "ble" ||
                family == "bne" || family == "bgt" || family == "bge")
            {
                yield return "branch";
                yield return PredicateSpecFromFamily(family.Substring(1));
            }
            else if (family == "add" || family == "and" || family == "div" ||
                family == "xor" || family == "shl" || family == "rem" ||
                family == "mul" || family == "neg" || family == "not" ||
                family == "or" || family == "shr" || family == "sub" ||
                family == "ceq" || family == "clt" || family == "cgt")
            {
                yield return "operator";
                yield return OperatorSpecFromFamily(family);
            }
            else if (family == "ldobj" || family == "stobj")
            {
                yield return family.Replace("obj", "ind");
            }
            else if (family == "calli")
            {
                yield return "call";
            }
            else
            {
                yield return family;
            }

            var tags = parts.Skip(1);
            foreach (var tag in tags)
            {
                yield return tag;
            }
        }

        private static void InferFromOpcode(OpCodeKb kb)
        {
            if (kb.OpCode.FlowControl == FlowControl.Meta)
            {
                kb.Tags.Add("Prefix");

                if (kb.Family == "unaligned")
                {
                    (kb.OpCode.OperandType == OperandType.ShortInlineI).AssertTrue();

                    var p = kb.EnsureProperty("Alignment", typeof(Byte));
                    kb.Meta["ValueProp"] = p.Name;
                    p.Getter = "_alignment";

                    var f = kb.EnsureField("_alignment", typeof(Byte));
                    f.Initializer = "ReadU1(reader)";
                }
                else if (kb.Family == "constrained")
                {
                    (kb.OpCode.OperandType == OperandType.InlineType).AssertTrue();

                    var p_Token = kb.EnsureProperty("TypeToken", typeof(Int32));
                    p_Token.Getter = "_typeToken;";

                    var p_Type = kb.EnsureProperty("Type", typeof(Type));
                    p_Type.Getter = "TypeFromToken(_typeToken);";

                    var f = kb.EnsureField("_typeToken", typeof(Int32));
                    f.Initializer = "ReadMetadataToken(reader)";
                }
                else
                {
                    (kb.OpCode.OperandType == OperandType.InlineNone).AssertTrue();
                }
            }
            else if (kb.Family == "endfinally" || kb.Family == "endfilter")
            {
                (kb.OpCode.OperandType == OperandType.InlineNone).AssertTrue();

//                kb.Tags.Add("Ignore");
                // todo. uncomment the line above and perform all necessary fixups
            }
            else if (kb.Family == "switch")
            {
                var f = kb.EnsureField("_targetOffsets", typeof(ReadOnlyCollection<Tuple<Int32, Int32>>));
                var buffer1 = new StringBuilder();
                buffer1.AppendLine(String.Format("(({0})(() => ",
                    typeof(Func<ReadOnlyCollection<Tuple<Int32, Int32>>>).GetCSharpRef(ToCSharpOptions.ForCodegen)));
                buffer1.AppendLine("{");
                buffer1.AppendLine("var n = ReadI4(reader);".Indent());
                buffer1.AppendLine(String.Format("var pivot = ({0})reader.BaseStream.Position + sizeof({0}) * n;",
                    typeof(Int32).GetCSharpRef(ToCSharpOptions.ForCodegen)).Indent());
                buffer1.AppendLine();
                buffer1.Append(String.Format("return {0}.ToReadOnly(",
                    typeof(EnumerableExtensions).GetCSharpRef(ToCSharpOptions.ForCodegen)).Indent());
                buffer1.AppendLine(String.Format("{0}.Select(",
                    typeof(Enumerable).GetCSharpRef(ToCSharpOptions.ForCodegen)));
                buffer1.AppendLine(String.Format("{0}.Range(1, n), _ => ",
                    typeof(Enumerable).GetCSharpRef(ToCSharpOptions.ForCodegen)).Indent().Indent());
                buffer1.AppendLine("{".Indent().Indent());
                buffer1.AppendLine("var relative = ReadI4(reader);".Indent().Indent().Indent());
                buffer1.AppendLine("var absolute = pivot + relative;".Indent().Indent().Indent());
                buffer1.AppendLine(String.Format("return {0}.New(relative, absolute);",
                    typeof(Tuple).GetCSharpRef(ToCSharpOptions.ForCodegen)).Indent().Indent().Indent());
                buffer1.AppendLine("}));".Indent().Indent());
                buffer1.Append("}))()");
                f.Initializer = buffer1.ToString();

                var p_RelativeTargetOffsets = kb.EnsureProperty("RelativeTargetOffsets", typeof(ReadOnlyCollection<Int32>));
                p_RelativeTargetOffsets.Getter = String.Format(
                    "{0}.ToReadOnly({1}.Select(_targetOffsets, t => t.Item2))",
                    typeof(EnumerableExtensions).GetCSharpRef(ToCSharpOptions.ForCodegen),
                    typeof(Enumerable).GetCSharpRef(ToCSharpOptions.ForCodegen));

                var p_AbsoluteTargetOffsets = kb.EnsureProperty("AbsoluteTargetOffsets", typeof(ReadOnlyCollection<Int32>));
                p_AbsoluteTargetOffsets.Getter = String.Format(
                    "{0}.ToReadOnly({1}.Select(_targetOffsets, t => t.Item2))",
                    typeof(EnumerableExtensions).GetCSharpRef(ToCSharpOptions.ForCodegen),
                    typeof(Enumerable).GetCSharpRef(ToCSharpOptions.ForCodegen));

                var p_Targets = kb.EnsureProperty("Targets", typeof(ReadOnlyCollection<ILOp>));
                var buffer2 = new StringBuilder();
                buffer2.Append(String.Format("var resolved = {0}.Select(",
                    typeof(Enumerable).GetCSharpRef(ToCSharpOptions.ForCodegen)));
                buffer2.AppendLine("AbsoluteTargetOffsets, offset => ResolveReference(offset));");
                buffer2.Append(String.Format("return {0}.ToReadOnly(resolved);",
                    typeof(EnumerableExtensions).GetCSharpRef(ToCSharpOptions.ForCodegen)));
                p_Targets.Getter = buffer2.ToString();
            }
            else if (kb.OpCode.FlowControl == FlowControl.Branch ||
                kb.OpCode.FlowControl == FlowControl.Cond_Branch)
            {
                (kb.OpCode.OperandType == OperandType.InlineBrTarget ||
                kb.OpCode.OperandType == OperandType.ShortInlineBrTarget).AssertTrue();

                var p_target = kb.EnsureProperty("Target", typeof(IILOp));
                p_target.Getter = "ResolveReference(_absoluteTargetOffset)";

                var p_rto = kb.EnsureProperty("RelativeTargetOffset", typeof(Int32));
                p_rto.Getter = "_relativeTargetOffset";

                var p_ato = kb.EnsureProperty("AbsoluteTargetOffset", typeof(Int32));
                p_ato.Getter = "_absoluteTargetOffset";

                // relative offset === operand of the opcode
                var f_rto = kb.EnsureField("_relativeTargetOffset", typeof(Int32));
                f_rto.SetLazyInitializer(_ => String.Format(
                    "Read" + TypeSpecFromSpec(kb).Capitalize() + "(reader)",
                    typeof(Int32).GetCSharpRef(ToCSharpOptions.ForCodegen)));

                // absolute offset === exact offset that can be resolved into target
                var f_ato = kb.EnsureField("_absoluteTargetOffset", typeof(Int32));
                f_ato.SetLazyInitializer(_ => String.Format(
                    "({0})origPos + sizeof({1}) + _relativeTargetOffset",
                    typeof(Int32).GetCSharpRef(ToCSharpOptions.ForCodegen),
                    TypeFromSpec(kb).AssertNotNull().GetCSharpRef(ToCSharpOptions.ForCodegen)));
            }
            else if (kb.Family == "ldarg" || kb.Family == "ldarga" || kb.Family == "starg")
            {
                (kb.OpCode.OperandType == OperandType.InlineVar ||
                kb.OpCode.OperandType == OperandType.ShortInlineVar ||
                kb.OpCode.OperandType == OperandType.InlineNone).AssertTrue();

                var p_Index = kb.EnsureProperty("Index", typeof(int));
                p_Index.Getter = "_value";

                var p_Arg = kb.EnsureProperty("Arg", typeof(ParameterInfo));
                p_Arg.Getter = 
                    "if (Source.Method == null || Source.Args == null)" + Environment.NewLine +
                    "{" + Environment.NewLine +
                    "    return null;" + Environment.NewLine +
                    "}" + Environment.NewLine +
                    "else" + Environment.NewLine +
                    "{" + Environment.NewLine +
                    "    if (Source.Method.IsStatic)" + Environment.NewLine +
                    "    {" + Environment.NewLine +
                    "        return Source.Args[_value];" + Environment.NewLine +
                    "    }" + Environment.NewLine +
                    "    else" + Environment.NewLine +
                    "    {" + Environment.NewLine +
                    "        return _value == 0 ? null : Source.Args[_value - 1];" + Environment.NewLine +
                    "    }" + Environment.NewLine +
                    "}";

                var f_value = kb.EnsureField("_value", typeof(Int32));
                var f_useConstValue = kb.EnsureField("_useConstValue", typeof(bool));
                var f_constValue = kb.EnsureField("_constValue", typeof(Int32?));
                f_value.SetLazyInitializer(_ => f_useConstValue.Name + " ? " +
                    f_constValue.Name + ".Value : Read" + TypeSpecFromSpec(kb).Capitalize() + "(reader)");
            }
            else if (kb.Family == "ldloc" || kb.Family == "ldloca" || kb.Family == "stloc")
            {
                (kb.OpCode.OperandType == OperandType.InlineVar ||
                kb.OpCode.OperandType == OperandType.ShortInlineVar ||
                kb.OpCode.OperandType == OperandType.InlineNone).AssertTrue();

                var p_Index = kb.EnsureProperty("Index", typeof(int));
                p_Index.Getter = "_value";

                var p_Loc = kb.EnsureProperty("Loc", typeof(ILocalVar));
                p_Loc.Getter =
                    "if (Source.Method == null || Source.Locals == null)" + Environment.NewLine +
                    "{" + Environment.NewLine +
                    "    return null;" + Environment.NewLine +
                    "}" + Environment.NewLine +
                    "else" + Environment.NewLine +
                    "{" + Environment.NewLine +
                    "    return Source.Locals[_value];" + Environment.NewLine +
                    "}";

                var f_value = kb.EnsureField("_value", typeof(Int32));
                var f_useConstValue = kb.EnsureField("_useConstValue", typeof(bool));
                var f_constValue = kb.EnsureField("_constValue", typeof(Int32?));
                f_value.SetLazyInitializer(_ => f_useConstValue.Name + " ? " +
                    f_constValue.Name + ".Value : Read" + TypeSpecFromSpec(kb).Capitalize() + "(reader)");
            }
            else if (kb.Family == "ldc")
            {
                (kb.OpCode.OperandType == OperandType.InlineI ||
                kb.OpCode.OperandType == OperandType.InlineI8 ||
                kb.OpCode.OperandType == OperandType.InlineR ||
                kb.OpCode.OperandType == OperandType.ShortInlineI ||
                kb.OpCode.OperandType == OperandType.ShortInlineR ||
                kb.OpCode.OperandType == OperandType.InlineString ||
                kb.OpCode.OperandType == OperandType.InlineTok ||
                kb.OpCode.OperandType == OperandType.InlineNone).AssertTrue();

                var p = kb.EnsureProperty("Value", typeof(Object));
                p.Getter = "_value";

                var f_value = kb.EnsureField("_value", typeof(Object));
                var f_useConstValue = kb.EnsureField("_useConstValue", typeof(bool));
                var f_constValue = kb.EnsureField("_constValue", typeof(Object));
                var cast = kb.OpCode.Name == "ldc.i4.s" ? String.Format("({0})",
                    typeof(int).GetCSharpRef(ToCSharpOptions.Informative)) : String.Empty;
                f_value.SetLazyInitializer(_ => f_useConstValue.Name + " ? " +
                    f_constValue.Name + " : " + cast + "Read" + TypeSpecFromSpec(kb).Capitalize() + "(reader)");
            }
            else if (kb.Family == "isinst" ||
                kb.Family == "stobj" || kb.Family == "ldobj" || kb.Family == "cpobj" || 
                kb.Family == "mkrefany" || kb.Family == "refanyval" || kb.Family == "sizeof")
            {
                (kb.OpCode.OperandType == OperandType.InlineType).AssertTrue();

                var p_Token = kb.EnsureProperty("TypeToken", typeof(Int32));
                p_Token.Getter = "_typeToken;";

                var p_Type = kb.EnsureProperty("Type", typeof(Type));
                p_Type.Getter = "TypeFromToken(_typeToken);";

                var f = kb.EnsureField("_typeToken", typeof(Int32));
                f.Initializer = "ReadMetadataToken(reader)";
            }
            else if (kb.Family == "ldelem" || kb.Family == "ldelema" || kb.Family == "stelem")
            {
                (kb.OpCode.OperandType == OperandType.InlineType ||
                kb.OpCode.OperandType == OperandType.InlineNone).AssertTrue();

                if (kb.OpCode.OperandType == OperandType.InlineType)
                {
                    var p_Token = kb.EnsureProperty("TypeToken", typeof(Int32));
                    p_Token.Getter = "_typeToken;";

                    var p_Type = kb.EnsureProperty("Type", typeof(Type));
                    p_Type.Getter = "TypeFromToken(_typeToken);";

                    var f = kb.EnsureField("_typeToken", typeof(Int32));
                    f.Initializer = "ReadMetadataToken(reader)";
                }
                else
                {
                    // InferFromTag will take care of XXXelemYYY.TYPE (see below)
                }
            }
            else if (kb.Family == "ldind" || kb.Family == "stind")
            {
                (kb.OpCode.OperandType == OperandType.InlineType ||
                kb.OpCode.OperandType == OperandType.InlineNone).AssertTrue();

                if (kb.OpCode.OperandType == OperandType.InlineType)
                {
                    var p_Token = kb.EnsureProperty("TypeToken", typeof(Int32));
                    p_Token.Getter = "_typeToken;";

                    var p_Type = kb.EnsureProperty("Type", typeof(Type));
                    p_Type.Getter = "TypeFromToken(_typeToken);";

                    var f = kb.EnsureField("_typeToken", typeof(Int32));
                    f.Initializer = "ReadMetadataToken(reader)";
                }
                else
                {
                    // InferFromTag will take care of XXXind.TYPE (see below)
                }

                kb.EnsurePrefix("IsVolatile", typeof(bool), "volatile");
                kb.EnsurePrefix("IsUnaligned", typeof(bool), "unaligned");
                var p_IsAligned = kb.EnsureProperty("IsAligned", typeof(bool));
                p_IsAligned.Getter = "!IsUnaligned";
                var p_Alignment = kb.EnsureProperty("Alignment", typeof(byte));
                var buf = new StringBuilder();
                buf.AppendLine(String.Format(
                    "var unaligned = {0}.SingleOrDefault({0}.OfType<{1}>(Prefixes));",
                    typeof(Enumerable), "Unaligned"));
                buf.AppendLine(String.Format(
                    "var defaultAlignment = (({0})(() => {{ throw new {1}(); }}))();",
                     typeof(Func<byte>).GetCSharpRef(ToCSharpOptions.ForCodegen),
                     typeof(NotImplementedException).GetCSharpRef(ToCSharpOptions.ForCodegen)));
                buf.Append("return unaligned != null ? unaligned.Alignment : defaultAlignment");
                p_Alignment.Getter = buf.ToString();
            }
            else if (kb.Family == "cast")
            {
                (kb.OpCode.OperandType == OperandType.InlineType ||
                kb.OpCode.OperandType == OperandType.InlineNone).AssertTrue();

                if (kb.OpCode.OperandType == OperandType.InlineType)
                {
                    var p_Token = kb.EnsureProperty("TypeToken", typeof(Int32));
                    p_Token.Getter = "_typeToken;";

                    var p_Type = kb.EnsureProperty("Type", typeof(Type));
                    p_Type.Getter = kb.OpCode.Name == "box" ?
                        String.Format("typeof({0})", typeof(Object).GetCSharpRef(ToCSharpOptions.ForCodegen)) :
                        "TypeFromToken(_typeToken);";

                    var f = kb.EnsureField("_typeToken", typeof(Int32));
                    f.Initializer = "ReadMetadataToken(reader)";
                }
            }
            else if (kb.Family == "new")
            {
                (kb.OpCode.OperandType == OperandType.InlineType ||
                kb.OpCode.OperandType == OperandType.InlineMethod).AssertTrue();

                var p_ctorToken = kb.EnsureProperty("CtorToken", typeof(Int32?));
                p_ctorToken.Getter = "_ctor;";

                var f_ctor = kb.EnsureField("_ctor", typeof(Int32?));
                var p_ctor = kb.EnsureProperty("Ctor", typeof(ConstructorInfo));
                p_ctor.Getter = f_ctor.Name + " == null ? null : " + 
                    "CtorFromToken(" + f_ctor.Name + ".Value)";

                var p_typeToken = kb.EnsureProperty("TypeToken", typeof(Int32?));
                p_typeToken.Getter = "_type;";

                var f_type = kb.EnsureField("_type", typeof(Int32?));
                var p_type = kb.EnsureProperty("Type", typeof(Type));
                p_type.Getter = 
                    "var type = " + f_type.Name +" == null ? "+
                    "(" + p_ctor.Name + " != null ? " + p_ctor.Name + ".DeclaringType : null) " +
                    ": TypeFromToken(" + f_type.Name + ".Value);" + Environment.NewLine +
                    "if (type == null)" + Environment.NewLine +
                    "{" + Environment.NewLine +
                    "    return null;" + Environment.NewLine +
                    "}" + Environment.NewLine +
                    "else" + Environment.NewLine +
                    "{" + Environment.NewLine +
                    "    return _isArray ? type.MakeArrayType() : type;" + Environment.NewLine +
                    "}";
            }
            else if (kb.Family == "ldftn" || kb.Family == "jmp")
            {
                (kb.OpCode.OperandType == OperandType.InlineMethod).AssertTrue();

                var p_Token = kb.EnsureProperty("MethodToken", typeof(Int32));
                p_Token.Getter = "_methodToken;";

                var p_Method = kb.EnsureProperty("Method", typeof(MethodBase));
                p_Method.Getter = "MethodBaseFromToken(_methodToken);";

                var f = kb.EnsureField("_methodToken", typeof(Int32));
                f.Initializer = "ReadMetadataToken(reader)";
            }
            else if (kb.Family == "call")
            {
                (kb.OpCode.OperandType == OperandType.InlineMethod ||
                 kb.OpCode.OperandType == OperandType.InlineSig).AssertTrue();

                var p_Method = kb.EnsureProperty("Method", typeof(MethodBase));
                if (kb.OpCode.OperandType == OperandType.InlineMethod) p_Method.Getter = "MethodBaseFromToken(_methodToken);";
                else if (kb.OpCode.OperandType == OperandType.InlineSig) p_Method.Getter = "MethodBaseFromSignature(SignatureFromToken(_signatureToken));";
                else throw AssertionHelper.Fail();

                var p_MethodToken = kb.EnsureProperty("MethodToken", typeof(Int32));
                if (kb.OpCode.OperandType == OperandType.InlineMethod) p_MethodToken.Getter = "_methodToken;";
                else if (kb.OpCode.OperandType == OperandType.InlineSig) p_MethodToken.Getter = null;
                else throw AssertionHelper.Fail();

                var f_MethodToken = kb.EnsureField("_methodToken", typeof(Int32));
                if (kb.OpCode.OperandType == OperandType.InlineMethod) f_MethodToken.Initializer = "ReadMetadataToken(reader)";
                else if (kb.OpCode.OperandType == OperandType.InlineSig) f_MethodToken.Initializer = null;
                else throw AssertionHelper.Fail();

                var p_Signature = kb.EnsureProperty("Signature", typeof(byte[]));
                if (kb.OpCode.OperandType == OperandType.InlineMethod) p_Signature.Getter = null;
                else if (kb.OpCode.OperandType == OperandType.InlineSig) p_Signature.Getter = "SignatureFromToken(_signatureToken);";
                else throw AssertionHelper.Fail();

                var p_SignatureToken = kb.EnsureProperty("SignatureToken", typeof(Int32));
                if (kb.OpCode.OperandType == OperandType.InlineMethod) p_SignatureToken.Getter = null;
                else if (kb.OpCode.OperandType == OperandType.InlineSig) p_SignatureToken.Getter = "_signatureToken;";
                else throw AssertionHelper.Fail();

                var f_SignatureToken = kb.EnsureField("_signatureToken", typeof(Int32));
                if (kb.OpCode.OperandType == OperandType.InlineMethod) f_SignatureToken.Initializer = null;
                else if (kb.OpCode.OperandType == OperandType.InlineSig) f_SignatureToken.Initializer = "ReadMetadataToken(reader)";
                else throw AssertionHelper.Fail();

                var pfx_constrained = kb.EnsurePrefix("Constraint", typeof(Type), "constrained");
                pfx_constrained.Getter = "Type";
                kb.EnsurePrefix("IsTail", typeof(bool), "tail");
            }
            else if (kb.Family == "ldfld" || kb.Family == "ldflda" || kb.Family == "stfld")
            {
                (kb.OpCode.OperandType == OperandType.InlineField).AssertTrue();

                var p_Token = kb.EnsureProperty("FieldToken", typeof(Int32));
                p_Token.Getter = "_fieldToken;";

                var p_Fld = kb.EnsureProperty("Field", typeof(FieldInfo));
                p_Fld.Getter = "FieldFromToken(_fieldToken);";

                var f = kb.EnsureField("_fieldToken", typeof(Int32));
                f.Initializer = "ReadMetadataToken(reader)";

                kb.EnsurePrefix("IsVolatile", typeof(bool), "volatile");
                var pfx_Unaligned = kb.EnsurePrefix("IsUnaligned", typeof(bool), "unaligned");
                if (kb.Family == "ldfld") pfx_Unaligned.Filter = "OpSpec.OpCode.Value != 0x7e /* ldsfld */";
                if (kb.Family == "stfld") pfx_Unaligned.Filter = "OpSpec.OpCode.Value != 0x80 /* stsfld */";

                var p_IsAligned = kb.EnsureProperty("IsAligned", typeof(bool));
                p_IsAligned.Getter = "!IsUnaligned";
                var p_Alignment = kb.EnsureProperty("Alignment", typeof(byte));
                var buf = new StringBuilder();
                buf.AppendLine(String.Format(
                    "var unaligned = {0}.SingleOrDefault({0}.OfType<{1}>(Prefixes));",
                    typeof(Enumerable), "Unaligned"));
                buf.AppendLine(String.Format(
                    "var defaultAlignment = (({0})(() => {{ throw new {1}(); }}))();",
                     typeof(Func<byte>).GetCSharpRef(ToCSharpOptions.ForCodegen),
                     typeof(NotImplementedException).GetCSharpRef(ToCSharpOptions.ForCodegen)));
                buf.Append("return unaligned != null ? unaligned.Alignment : defaultAlignment");
                p_Alignment.Getter = buf.ToString();
            }
            else if (kb.Family == "initblk" || kb.Family == "cpblk")
            {
                kb.EnsurePrefix("IsVolatile", typeof(bool), "volatile");
                kb.EnsurePrefix("IsUnaligned", typeof(bool), "unaligned");
                var p_IsAligned = kb.EnsureProperty("IsAligned", typeof(bool));
                p_IsAligned.Getter = "!IsUnaligned";
                var p_Alignment = kb.EnsureProperty("Alignment", typeof(byte));
                var buf = new StringBuilder();
                buf.AppendLine(String.Format(
                    "var unaligned = {0}.SingleOrDefault({0}.OfType<{1}>(Prefixes));",
                    typeof(Enumerable).GetCSharpRef(ToCSharpOptions.ForCodegen), 
                    "Unaligned"));
                buf.AppendLine(String.Format(
                    "var defaultAlignment = ({0}){1}.SizeOf(typeof({2}));",
                    typeof(byte).GetCSharpRef(ToCSharpOptions.ForCodegen),
                    typeof(Marshal).GetCSharpRef(ToCSharpOptions.ForCodegen),
                     typeof(IntPtr).GetCSharpRef(ToCSharpOptions.ForCodegen)));
                buf.Append("return unaligned != null ? unaligned.Alignment : defaultAlignment");
                p_Alignment.Getter = buf.ToString();
                p_Alignment.IsUnsafe = true;
            }
            else
            {
                // so that we never miss any other instructions with arguments
                (kb.OpCode.OperandType == OperandType.InlineNone).AssertTrue();
            }
        }

        private static void InferFromTag(OpCodeKb kb, String tag)
        {
            if (tag == "ovf")
            {
                var p = kb.EnsureProperty("FailsOnOverflow", typeof(bool));
                p.Getter = "true";
            }
            else if (tag == "un")
            {
                var p = kb.EnsureProperty("ExpectsUn", typeof(bool));
                p.Getter = "true";
            }
            else if (tag == "s")
            {
                // just ignore this
            }
            else if (tag == "virt")
            {
                var p = kb.EnsureProperty("IsVirtual", typeof(bool));
                p.Getter = "true";
            }
            else if (tag.StartsWith("expects.") || tag.StartsWith("yields."))
            {
                var p = kb.EnsureProperty(CapitalizeAfterPeriods(tag), typeof(bool));
                p.Getter = "true";
            }
            else if (tag.StartsWith("predicate."))
            {
                var p = kb.EnsureProperty("PredicateType", typeof(PredicateType?));
                var predicate = PredicateFromPredicateSpec(tag);
                p.Getter = typeof(PredicateType).GetCSharpRef(ToCSharpOptions.ForCodegen) + "." + predicate;
            }
            else if (tag.StartsWith("operator."))
            {
                var p = kb.EnsureProperty("OperatorType", typeof(OperatorType));
                var @operator = OperatorFromOperatorSpec(tag);
                p.Getter = typeof(OperatorType).GetCSharpRef(ToCSharpOptions.ForCodegen) + "." + @operator;
            }
            else if (tag.StartsWith("instantiates."))
            {
                var f_ctor = kb.Fields["_ctor"];
                var f_type = kb.Fields["_type"];

                var what = tag.Substring("instantiates.".Length);
                if (what == "ref")
                {
                    f_ctor.Initializer = "ReadMetadataToken(reader)";
                }
                else if (what == "val")
                {
                    f_type.Initializer = "ReadMetadataToken(reader)";
                }
                else if (what == "arr")
                {
                    f_type.Initializer = "ReadMetadataToken(reader)";

                    var f_arr = kb.EnsureField("_isArray", typeof(bool));
                    f_arr.Initializer = "true";
                }
                else
                {
                    AssertionHelper.Fail();
                }
            }
            else if (tag == "br" || tag == "leave")
            {
                // this will be processed elsewhere
            }
            else if (tag == String.Empty)
            {
                // empty string after dot in prefix ops
                kb.Tags.Remove(String.Empty);
                kb.Tags.Add(".");
            }
            else
            {
                // all this hassle is solely for Ldstr/Ldtoken instructions
                // since they need both type and const semantics

                var type = TypeFromTypeSpec(tag);
                var @const = ValueFromConstSpec(tag);

                if (type != null)
                {
                    kb.Meta["Type"] = tag;

                    kb.Props.ContainsKey("Type").AssertFalse();
                    var p = kb.EnsureProperty("Type", typeof(Type));
                    if (tag != "token") p.Getter = "typeof(" + type.GetCSharpRef(ToCSharpOptions.ForCodegen) + ")";
                    else p.Getter = "_constValue.GetType()";
                }

                if (@const != null)
                {
                    var f_constValue = kb.Fields["_constValue"];
                    var f_useConstValue = kb.Fields["_useConstValue"];

                    f_constValue.Initializer = @const.ToInvariantString();
                    f_useConstValue.Initializer = "true";
                }

                if (@type == null && @const == null)
                {
                    // so that we never miss an instruction with an unknown part
                    AssertionHelper.Fail();
                }
            }
        }

        private static Type TypeFromTypeSpec(String part)
        {
            switch (part)
            {
                case "i":
                    return typeof(IntPtr);
                case "i1":
                    return typeof(SByte);
                case "i2":
                    return typeof(Int16);
                case "i4":
                    return typeof(Int32);
                case "i8":
                    return typeof(Int64);
                case "u":
                    return typeof(UIntPtr);
                case "u1":
                    return typeof(Byte);
                case "u2":
                    return typeof(UInt16);
                case "u4":
                    return typeof(UInt32);
                case "u8":
                    return typeof(UInt64);
                case "r":
                    return typeof(Single);
                case "r4":
                    return typeof(Single);
                case "r8":
                    return typeof(Double);
                case "ref":
                    return typeof(Object);
                case "str":
                    return typeof(String);
                case "token":
                    return typeof(RuntimeHandle);
                default:
                    return null;
            }
        }

        private static String TypeSpecFromSpec(OpCodeKb kb)
        {
            var type = kb.Meta.GetOrDefault("Type", "i4");
            if (kb.Tags.Contains("s"))
            {
                var bytes = int.Parse(type.Substring(1));
                type = type.Substring(0, 1) + (bytes / 4);
            }

            return type;
        }

        private static Type TypeFromSpec(OpCodeKb kb)
        {
            return TypeFromTypeSpec(TypeSpecFromSpec(kb));
        }

        private static Object ValueFromConstSpec(String tag)
        {
            if (tag == "null")
            {
                return "null";
            }
            else if (tag == "str")
            {
                return String.Format(
                    "(({0})(() => {{ "+
                        "var token = ReadMetadataToken(reader); " +
                        "return StringFromToken(token) ?? (\"string at 0x\" + token.ToString(\"x8\")); " +
                    "}}))()",
                    typeof(Func<String>).GetCSharpRef(ToCSharpOptions.ForCodegen));
            }
            else if (tag == "token")
            {
                return String.Format(
                    "(({0})(() => {{ " +
                        "var i_token = ReadMetadataToken(reader); " +
                        "var token = new {1}(Source.Module, Source.Type, Source.Method, i_token); " +
                        "try " +
                        "{{ " +
                        "    var resolved = token.ResolveHandle(); " +
                        "    if (resolved != null) return resolved; " +
                        "    else " +
                        "    {{ " +
                        "        return token; " +
                        "    }} " +
                        "}} " +
                        "catch " +
                        "{{ " +
                        "    return token; " +
                        "}} " +
                    "}}))()",
                    typeof(Func<Object>).GetCSharpRef(ToCSharpOptions.ForCodegen),
                    typeof(RuntimeHandle).GetCSharpRef(ToCSharpOptions.ForCodegen));
            }
            else
            {
                if (tag == "m1")
                {
                    return -1;
                }
                else
                {
                    int @int;
                    if (int.TryParse(tag, out @int))
                    {
                        return @int;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        private static String PredicateSpecFromFamily(String family)
        {
            if (family == "eq") return "predicate." + PredicateType.Equal;
            if (family == "gt") return "predicate." + PredicateType.GreaterThan;
            if (family == "ge") return "predicate." + PredicateType.GreaterThanOrEqual;
            if (family == "lt") return "predicate." + PredicateType.LessThan;
            if (family == "le") return "predicate." + PredicateType.LessThanOrEqual;
            if (family == "ne") return "predicate." + PredicateType.NotEqual;
            if (family == "false") return "predicate." + PredicateType.IsFalse;
            if (family == "true") return "predicate." + PredicateType.IsTrue;
            return null;
        }

        private static PredicateType PredicateFromPredicateSpec(String tag)
        {
            tag.AssertNotNull().StartsWith("predicate.").AssertTrue();
            return (PredicateType)typeof(PredicateType).FromInvariantString(tag.Substring("predicate.".Length));
        }

        private static String OperatorSpecFromFamily(String family)
        {
            if (family == "add") return "operator." + OperatorType.Add;
            if (family == "and") return "operator." + OperatorType.And;
            if (family == "div") return "operator." + OperatorType.Divide;
            if (family == "ceq") return "operator." + OperatorType.Equal;
            if (family == "cgt") return "operator." + OperatorType.GreaterThan;
            if (family == "shl") return "operator." + OperatorType.LeftShift;
            if (family == "clt") return "operator." + OperatorType.LessThan;
            if (family == "rem") return "operator." + OperatorType.Modulo;
            if (family == "mul") return "operator." + OperatorType.Multiply;
            if (family == "neg") return "operator." + OperatorType.Negate;
            if (family == "not") return "operator." + OperatorType.Not;
            if (family == "or") return "operator." + OperatorType.Or;
            if (family == "shr") return "operator." + OperatorType.RightShift;
            if (family == "sub") return "operator." + OperatorType.Subtract;
            if (family == "xor") return "operator." + OperatorType.Xor;
            return null;
        }

        private static OperatorType OperatorFromOperatorSpec(String tag)
        {
            tag.AssertNotNull().StartsWith("operator.").AssertTrue();
            return (OperatorType)typeof(OperatorType).FromInvariantString(tag.Substring("operator.".Length));
        }

        private static String CapitalizeAfterPeriods(String tag)
        {
            var buffer = new StringBuilder();

            var capitalizeNext = true;
            foreach (var c in tag)
            {
                if (c != '.')
                {
                    var @char = capitalizeNext ? Char.ToUpper(c) : c;
                    buffer.Append(@char);
                }

                var isPeriod = c == '.';
                capitalizeNext = isPeriod;
            }

            return buffer.ToString();
        }
    }
}
