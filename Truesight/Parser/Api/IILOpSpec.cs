using System.Reflection.Emit;

namespace Truesight.Parser.Api
{
    public interface IILOpSpec
    {
        IILOp Op { get; }
        OpCode OpCode { get; }

        OpCodeType OpCodeType { get; }
        FlowControl FlowControl { get; }
        OperandType OperandType { get; }
        StackBehaviour PushBehavior { get; }
        StackBehaviour PopBehavior { get; }
        int Pushes { get; }
        int Pops { get; }
    }
}