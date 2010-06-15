using System.Diagnostics;
using Truesight.Decompiler.Hir;
using Truesight.Decompiler.Hir.Core.Expressions;
using XenoGears.Reflection;

namespace Truesight.Decompiler.Pipeline.Cil.OpAssign
{
    [DebuggerNonUserCode]
    internal static class OperatorHelper
    {
        public static Operator CreateOpPreAssign(this Node lhs, OperatorType op, Node rhs)
        {
            var e_lhs = lhs as Expression;
            var e_rhs = rhs as Expression;
            if (e_lhs == null || e_rhs == null) return null;

            OperatorType opAssign;
            if (EnumHelper.TryParse(op + "Assign", out opAssign))
            {
                if (op == OperatorType.Add && e_rhs.IsConstOne())
                {
                    return Operator.PreIncrement(e_lhs);
                }
                else if (op == OperatorType.Subtract && e_rhs.IsConstOne())
                {
                    return Operator.PreDecrement(e_lhs);
                }
                else
                {
                    return Operator.Create(opAssign, e_lhs, e_rhs);
                }
            }
            else
            {
                return null;
            }
        }

        public static Operator CreateOpPostAssign(this Node lhs, OperatorType op, Node rhs)
        {
            var e_lhs = lhs as Expression;
            var e_rhs = rhs as Expression;
            if (e_lhs == null || e_rhs == null) return null;

            OperatorType opAssign;
            if (EnumHelper.TryParse(op + "Assign", out opAssign))
            {
                if (op == OperatorType.Add && e_rhs.IsConstOne())
                {
                    return Operator.PostIncrement(e_lhs);
                }
                else if (op == OperatorType.Subtract && e_rhs.IsConstOne())
                {
                    return Operator.PostDecrement(e_lhs);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private static bool IsConstOne(this Node n)
        {
            return n is Const &&
              ((Const)n).Value != null &&
              ((Const)n).Value.GetType().IsNumeric() &&
              ((Const)n).Value.ToString() == "1";
        }
    }
}