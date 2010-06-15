using System;
using System.Diagnostics;
using XenoGears.Assertions;

namespace Truesight.Parser.Api.Ops
{
    [DebuggerNonUserCode]
    public static class OperatorTypeHelper
    {
        public static int Arity(this OperatorType opType)
        {
            switch (opType)
            {
                case OperatorType.Add:
                    return 2;
                case OperatorType.And:
                    return 2;
                case OperatorType.Divide:
                    return 2;
                case OperatorType.Equal:
                    return 2;
                case OperatorType.GreaterThan:
                    return 2;
                case OperatorType.GreaterThanOrEqual:
                    return 2;
                case OperatorType.LeftShift:
                    return 2;
                case OperatorType.LessThan:
                    return 2;
                case OperatorType.LessThanOrEqual:
                    return 2;
                case OperatorType.NotEqual:
                    return 2;
                case OperatorType.Modulo:
                    return 2;
                case OperatorType.Multiply:
                    return 2;
                case OperatorType.Negate:
                    return 1;
                case OperatorType.Not:
                    return 1;
                case OperatorType.Or:
                    return 2;
                case OperatorType.RightShift:
                    return 2;
                case OperatorType.Subtract:
                    return 2;
                case OperatorType.Xor:
                    return 2;
                default:
                    throw AssertionHelper.Fail();
            }
        }

        public static PredicateType ToPredicateType(this OperatorType opType)
        {
            switch (opType)
            {
                case OperatorType.Equal:
                    return PredicateType.Equal;
                case OperatorType.GreaterThan:
                    return PredicateType.GreaterThan;
                case OperatorType.GreaterThanOrEqual:
                    return PredicateType.GreaterThanOrEqual;
                case OperatorType.LessThan:
                    return PredicateType.LessThan;
                case OperatorType.LessThanOrEqual:
                    return PredicateType.LessThanOrEqual;
                case OperatorType.NotEqual:
                    return PredicateType.NotEqual;
                default:
                    throw AssertionHelper.Fail();
            }
        }

        public static String ToCSharpSymbol(this OperatorType opType)
        {
            switch (opType)
            {
                case OperatorType.Add:
                    return "+";
                case OperatorType.And:
                    return "&&";
                case OperatorType.Divide:
                    return "/";
                case OperatorType.Equal:
                    return "==";
                case OperatorType.GreaterThan:
                    return ">";
                case OperatorType.GreaterThanOrEqual:
                    return ">=";
                case OperatorType.LeftShift:
                    return "<<";
                case OperatorType.LessThan:
                    return "<";
                case OperatorType.LessThanOrEqual:
                    return "<=";
                case OperatorType.NotEqual:
                    return "!=";
                case OperatorType.Modulo:
                    return "%";
                case OperatorType.Multiply:
                    return "*";
                case OperatorType.Negate:
                    return "-";
                case OperatorType.Not:
                    return "!";
                case OperatorType.Or:
                    return "||";
                case OperatorType.RightShift:
                    return ">>";
                case OperatorType.Subtract:
                    return "-";
                case OperatorType.Xor:
                    return "^";
                default:
                    throw AssertionHelper.Fail();
            }
        }

        // http://msdn.microsoft.com/en-us/library/6a71f45d.aspx
        public static int CSharpPriority(this OperatorType opType)
        {
            switch (opType)
            {
                case OperatorType.Add:
                    return 12;
                case OperatorType.And:
                    return 8;
                case OperatorType.Divide:
                    return 13;
                case OperatorType.Equal:
                    return 9;
                case OperatorType.GreaterThan:
                    return 10;
                case OperatorType.GreaterThanOrEqual:
                    return 10;
                case OperatorType.LeftShift:
                    return 11;
                case OperatorType.LessThan:
                    return 10;
                case OperatorType.LessThanOrEqual:
                    return 10;
                case OperatorType.NotEqual:
                    return 9;
                case OperatorType.Modulo:
                    return 13;
                case OperatorType.Multiply:
                    return 13;
                case OperatorType.Negate:
                    return 14;
                case OperatorType.Not:
                    return 14;
                case OperatorType.Or:
                    return 6;
                case OperatorType.RightShift:
                    return 11;
                case OperatorType.Subtract:
                    return 12;
                case OperatorType.Xor:
                    return 7;
                default:
                    throw AssertionHelper.Fail();
            }
        }
    }
}