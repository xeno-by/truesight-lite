using System.Diagnostics;
using System.Linq;
using XenoGears.Assertions;

namespace Truesight.Decompiler.Hir.Core.Expressions
{
    [DebuggerNonUserCode]
    public static class OperatorExtensions
    {
        public static UnaryOperator Unary(this Operator op)
        {
            return op.AssertCast<UnaryOperator>();
        }

        public static bool IsUnary(this Operator op)
        {
            return op.Args.Count() == 1;
        }

        public static BinaryOperator Binary(this Operator op)
        {
            return op.AssertCast<BinaryOperator>();
        }

        public static bool IsBinary(this Operator op)
        {
            return op.Args.Count() == 2;
        }
    }
}