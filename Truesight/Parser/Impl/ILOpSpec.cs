using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using XenoGears.Assertions;
using XenoGears.Reflection.Generics;
using Truesight.Parser.Api;
using Truesight.Parser.Api.Ops;
using System.Linq;

namespace Truesight.Parser.Impl
{
    [DebuggerNonUserCode]
    internal class ILOpSpec : IILOpSpec
    {
        IILOp IILOpSpec.Op { get { return Op; } }
        public ILOp Op { get; private set; }
        public OpCode OpCode { get; private set; }

        public OpCodeType OpCodeType { get { return OpCode.OpCodeType; } }
        public FlowControl FlowControl { get { return OpCode.FlowControl; } }
        public OperandType OperandType { get { return OpCode.OperandType; } }

        public StackBehaviour PushBehavior { get { return OpCode.StackBehaviourPush; } }
        public StackBehaviour PopBehavior { get { return OpCode.StackBehaviourPop; } }
        int IILOpSpec.Pushes { get { return Pushes; } }
        int IILOpSpec.Pops { get { return Pops; } }

        public ILOpSpec(ILOp op, OpCode opcode)
        {
            Op = op;
            OpCode = opcode;
        }

        public int Pushes
        {
            get
            {
                switch (PushBehavior)
                {
                    case StackBehaviour.Push0:
                        return 0;
                    case StackBehaviour.Push1:
                        return 1;
                    case StackBehaviour.Push1_push1:
                        return 2;
                    case StackBehaviour.Pushi:
                        return 1;
                    case StackBehaviour.Pushi8:
                        return 1;
                    case StackBehaviour.Pushr4:
                        return 1;
                    case StackBehaviour.Pushr8:
                        return 1;
                    case StackBehaviour.Pushref:
                        return 1;
                    case StackBehaviour.Varpush:
                        if (Op is Call)
                        {
                            var isVoid = Op.AssertCast<Call>().Method.Ret() == typeof(void);
                            return isVoid ? 0 : 1;
                        }
                        else
                        {
                            throw AssertionHelper.Fail();
                        }
                    default:
                        throw AssertionHelper.Fail();
                }
            }
        }

        public int Pops
        {
            get
            {
                switch (PopBehavior)
                {
                    case StackBehaviour.Pop0:
                        return 0;
                    case StackBehaviour.Pop1:
                        return 1;
                    case StackBehaviour.Pop1_pop1:
                        return 2;
                    case StackBehaviour.Popi:
                        return 1;
                    case StackBehaviour.Popi_pop1:
                        return 2;
                    case StackBehaviour.Popi_popi:
                        return 2;
                    case StackBehaviour.Popi_popi8:
                        return 2;
                    case StackBehaviour.Popi_popi_popi:
                        return 2;
                    case StackBehaviour.Popi_popr4:
                        return 2;
                    case StackBehaviour.Popi_popr8:
                        return 2;
                    case StackBehaviour.Popref:
                        return 1;
                    case StackBehaviour.Popref_pop1:
                        return 2;
                    case StackBehaviour.Popref_popi:
                        return 2;
                    case StackBehaviour.Popref_popi_popi:
                        return 3;
                    case StackBehaviour.Popref_popi_popi8:
                        return 3;
                    case StackBehaviour.Popref_popi_popr4:
                        return 3;
                    case StackBehaviour.Popref_popi_popr8:
                        return 3;
                    case StackBehaviour.Popref_popi_popref:
                        return 3;
                    case StackBehaviour.Varpop:
                        if (Op is Call)
                        {
                            var callee = Op.AssertCast<Call>().Method;
                            var @this = callee.IsStatic ? 0 : 1;
                            var argc = callee.GetParameters().Count();
                            return @this + argc;
                        }
                        else if (Op is New)
                        {
                            var ctor = Op.AssertCast<New>().Ctor;
                            return ctor == null ? 0 : ctor.GetParameters().Count();
                        }
                        else if (Op is Ret)
                        {
                            // todo. this is a hack -> see comments to IMethodBody for more info
                            if (Op.Source.Method is ConstructorInfo ||
                                Op.Source.Method is ConstructorBuilder)
                            {
                                return 0;
                            }
                            else
                            {
                                if (Op.Source.Ret == null)
                                {
                                    return int.MaxValue;
                                }
                                else
                                {
                                    var returnsVoid = Op.Source.Ret.ParameterType == typeof(void);
                                    return returnsVoid ? 0 : 1;
                                }
                            }
                        }
                        else
                        {
                            throw AssertionHelper.Fail();
                        }
                    case StackBehaviour.Popref_popi_pop1:
                        return 3;
                    default:
                        throw AssertionHelper.Fail();
                }
            }
        }
    }
}