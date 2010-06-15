using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Truesight.Parser.Api.Ops;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Reflection.Emit;
using XenoGears.Reflection.Emit.Hackarounds;
using XenoGears.Reflection.Simple;

namespace Truesight.Parser.Api.Emit
{
    // todo. this doesn't take into account all the intricacies of ILGenerator
    // e.g. ain't update maxstack, tokenfixups and God knows what else
    
    public static class ILTrait
    {
        public static ILGenerator EmitClone(this ILGenerator il, IILOp op)
        {
            return il.clone(op);
        }

        public static ILGenerator clone(this ILGenerator il, IILOp op)
        {
            // todo #1. also take into account that we need to update stacksize
            // todo #2. what is RecordTokenFixup()?

            // load some internals
            var methodBuilder = il.Get("m_methodBuilder").AssertCast<MethodBuilder>();
            var typeBuilder = methodBuilder.Get("m_containingType").AssertCast<TypeBuilder>();
            var ilModule = typeBuilder.Get("m_module").AssertCast<ModuleBuilder>();

            // if op has some prefixes, clone them first
            op.Prefixes.ForEach(prefix => il.clone(prefix));

            // hello, Shlemiel the Painter...
            var operand = op.BytesOfOperand.ToArray();

            // fixup metadata tokens
            // 1. if op doesn't have a module specified, we just crash
            // 2. if op references a module builder, we verify that ilgen references it too
            //    since as of .NET 3.5 you cannot resolve a token in module builder
            // 3. if op has a regular module, we proceed to fixup
            var opModule = op.Get("Module").AssertCast<Module>();
            opModule.AssertNotNull();

            if (opModule is ModuleBuilder)
            {
                (opModule == ilModule).AssertTrue();
            }
            else
            {
                int newToken;

                // prepare to process ldtokens
                var ldc = op as Ldc;
                var ldc_value = ldc == null ? null : ldc.Value;
                var ldc_member = null as MemberInfo;
                if (ldc_value != null)
                {
                    if (ldc_value is RuntimeTypeHandle)
                    {
                        var rth = (RuntimeTypeHandle)ldc_value;
                        ldc_member = Type.GetTypeFromHandle(rth);
                    }
                    else if (ldc_value is RuntimeMethodHandle)
                    {
                        var rmh = (RuntimeMethodHandle)ldc_value;
                        ldc_member = MethodBase.GetMethodFromHandle(rmh);
                    }
                    else if (ldc_value is RuntimeFieldHandle)
                    {
                        var rfh = (RuntimeFieldHandle)ldc_value;
                        ldc_member = FieldInfo.GetFieldFromHandle(rfh);
                    }
                }

                // process special case - ldtoken
                var operandType = op.OpSpec.OperandType;
                if (operandType == OperandType.InlineTok)
                {
                    ldc_member.AssertNotNull();
                    if (ldc_member is Type)
                    {
                        operandType = OperandType.InlineType;
                    }
                    else if (ldc_member is MethodBase)
                    {
                        operandType = OperandType.InlineMethod;
                    }
                    else if (ldc_member is FieldInfo)
                    {
                        operandType = OperandType.InlineField;
                    }
                    else
                    {
                        throw AssertionHelper.Fail();
                    }
                }

                // process all other cases
                switch (operandType)
                {
                    case OperandType.InlineType:
                        var type = (ldc_member ?? op.GetOrDefault("Type") ?? op.GetOrDefault("Resolved")).AssertCast<Type>();
                        if (op.OpSpec.OpCode == OpCodes.Newarr) type = type.GetElementType();
                        newToken = ilModule.GetTypeToken(type).Token;
                        operand = BitConverter.GetBytes(newToken);
                        break;

                    case OperandType.InlineField:
                        var field = (ldc_member ?? op.GetOrDefault("Field") ?? op.GetOrDefault("Resolved")).AssertCast<FieldInfo>();
                        newToken = ilModule.GetFieldToken(field).Token;
                        operand = BitConverter.GetBytes(newToken);
                        break;

                    case OperandType.InlineMethod:
                        // note 1. Module::GetMethodToken won't work here 
                        // since it returns index of an entry in the MemberRef metadata table (0x0a)
                        // closed generic methods are stored a separate metadata table (MethodSpec, 0x2b)

                        // note 2. Neither will work ILGenerator::GetMethodToken
                        // since it uses System.Reflection.Emit.SignatureHelper that has a bug
                        // for more details see: http://xeno-by.livejournal.com/14460.html

                        var method = (ldc_member ?? op.GetOrDefault("Method") ?? op.GetOrDefault("Ctor") ?? op.GetOrDefault("Resolved")).AssertCast<MethodBase>();
                        newToken = il.GetMethodToken_Hackaround(method);
                        operand = BitConverter.GetBytes(newToken);
                        break;

                    case OperandType.InlineString:
                        var @string = (op.GetOrDefault("Value") ?? op.GetOrDefault("Resolved")).AssertCast<String>();
                        newToken = ilModule.GetStringConstant(@string).Token;
                        operand = BitConverter.GetBytes(newToken);
                        break;

                    case OperandType.InlineSig:
                        // todo. implement this
                        throw AssertionHelper.Fail();

                    case OperandType.InlineBrTarget:
                    case OperandType.InlineI:
                    case OperandType.InlineI8:
                    case OperandType.InlineNone:
#pragma warning disable 612, 618
                    case OperandType.InlinePhi:
#pragma warning restore 612, 618
                    case OperandType.InlineR:
                    case OperandType.InlineSwitch:
                    case OperandType.InlineVar:
                    case OperandType.ShortInlineBrTarget:
                    case OperandType.ShortInlineI:
                    case OperandType.ShortInlineR:
                    case OperandType.ShortInlineVar:
                        // do nothing - operand ain't needs a fixup
                        break;

                    default:
                        throw AssertionHelper.Fail();
                }
            }

            // write the op to the output stream
            return il.raw(op.OpSpec.OpCode, operand);
        }
    }
}
