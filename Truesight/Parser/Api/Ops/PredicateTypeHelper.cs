using System;
using System.Diagnostics;
using XenoGears.Assertions;

namespace Truesight.Parser.Api.Ops
{
    [DebuggerNonUserCode]
    public static class PredicateTypeHelper
    {
        public static int Arity(this PredicateType predType)
        {
            switch (predType)
            {
                case PredicateType.Equal:
                    return 2;
                case PredicateType.GreaterThan:
                    return 2;
                case PredicateType.GreaterThanOrEqual:
                    return 2;
                case PredicateType.LessThan:
                    return 2;
                case PredicateType.LessThanOrEqual:
                    return 2;
                case PredicateType.NotEqual:
                    return 2;
                case PredicateType.IsTrue:
                    return 1;
                case PredicateType.IsFalse:
                    return 1;
                default:
                    throw AssertionHelper.Fail();
            }
        }

        public static int Arity(this PredicateType? predType)
        {
            return predType == null ? 0 : predType.Value.Arity();
        }

        public static PredicateType Negate(this PredicateType predType)
        {
            switch (predType)
            {
                case PredicateType.Equal:
                    return PredicateType.NotEqual;
                case PredicateType.GreaterThan:
                    return PredicateType.LessThanOrEqual;
                case PredicateType.GreaterThanOrEqual:
                    return PredicateType.LessThan;
                case PredicateType.LessThan:
                    return PredicateType.GreaterThanOrEqual;
                case PredicateType.LessThanOrEqual:
                    return PredicateType.GreaterThan;
                case PredicateType.NotEqual:
                    return PredicateType.Equal;
                case PredicateType.IsTrue:
                    return PredicateType.IsFalse;
                case PredicateType.IsFalse:
                    return PredicateType.IsTrue;
                default:
                    throw AssertionHelper.Fail();
            }
        }

        public static PredicateType? Negate(this PredicateType? predType)
        {
            return predType == null ? null : (PredicateType?)predType.Value.Negate();
        }

        public static OperatorType ToOperatorType(this PredicateType predType)
        {
            switch (predType)
            {
                case PredicateType.Equal:
                    return OperatorType.Equal;
                case PredicateType.GreaterThan:
                    return OperatorType.GreaterThan;
                case PredicateType.GreaterThanOrEqual:
                    return OperatorType.GreaterThanOrEqual;
                case PredicateType.LessThan:
                    return OperatorType.LessThan;
                case PredicateType.LessThanOrEqual:
                    return OperatorType.LessThanOrEqual;
                case PredicateType.NotEqual:
                    return OperatorType.NotEqual;
                default:
                    throw AssertionHelper.Fail();
            }
        }

        public static String ToSymbol(this PredicateType predType)
        {
            switch (predType)
            {
                case PredicateType.Equal:
                    return "==";
                case PredicateType.GreaterThan:
                    return ">";
                case PredicateType.GreaterThanOrEqual:
                    return ">=";
                case PredicateType.LessThan:
                    return "<";
                case PredicateType.LessThanOrEqual:
                    return "<=";
                case PredicateType.NotEqual:
                    return "!=";
                case PredicateType.IsTrue:
                    return "true";
                case PredicateType.IsFalse:
                    return "false";
                default:
                    throw new ArgumentOutOfRangeException("predType");
            }
        }

        public static String ToSymbol(this PredicateType? predType)
        {
            return predType == null ? null : predType.Value.ToSymbol();
        }
    }
}
