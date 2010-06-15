using System;
using System.Diagnostics;
using Truesight.Decompiler.Pipeline.Flow.Cfg;
using XenoGears.Assertions;
using XenoGears.Strings;

namespace Truesight.Decompiler.Hir.Core.Expressions
{
    [DebuggerNonUserCode]
    public static class OperatorTypeExtensions
    {
        public static int Arity(this OperatorType opt)
        {
            switch (opt)
            {
                case OperatorType.Add:
                    return 2;
                case OperatorType.AddAssign:
                    return 2;
                case OperatorType.And:
                    return 2;
                case OperatorType.AndAlso:
                    return 2;
                case OperatorType.AndAssign:
                    return 2;
                case OperatorType.Coalesce:
                    return 2;
                case OperatorType.Divide:
                    return 2;
                case OperatorType.DivideAssign:
                    return 2;
                case OperatorType.Equal:
                    return 2;
                case OperatorType.GreaterThan:
                    return 2;
                case OperatorType.GreaterThanOrEqual:
                    return 2;
                case OperatorType.LeftShift:
                    return 2;
                case OperatorType.LeftShiftAssign:
                    return 2;
                case OperatorType.LessThan:
                    return 2;
                case OperatorType.LessThanOrEqual:
                    return 2;
                case OperatorType.NotEqual:
                    return 2;
                case OperatorType.Modulo:
                    return 2;
                case OperatorType.ModuloAssign:
                    return 2;
                case OperatorType.Multiply:
                    return 2;
                case OperatorType.MultiplyAssign:
                    return 2;
                case OperatorType.Negate:
                    return 1;
                case OperatorType.Not:
                    return 1;
                case OperatorType.Or:
                    return 2;
                case OperatorType.OrAssign:
                    return 2;
                case OperatorType.OrElse:
                    return 2;
                case OperatorType.PreDecrement:
                    return 1;
                case OperatorType.PreIncrement:
                    return 1;
                case OperatorType.PostDecrement:
                    return 1;
                case OperatorType.PostIncrement:
                    return 1;
                case OperatorType.RightShift:
                    return 2;
                case OperatorType.RightShiftAssign:
                    return 2;
                case OperatorType.Subtract:
                    return 2;
                case OperatorType.SubtractAssign:
                    return 2;
                case OperatorType.Xor:
                    return 2;
                case OperatorType.XorAssign:
                    return 2;
                default:
                    throw AssertionHelper.Fail();
            }
        }

        internal static PredicateType ToPredicateType(this OperatorType opt)
        {
            switch (opt)
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

        public static String ToCSharpSymbol(this OperatorType opt)
        {
            switch (opt)
            {
                case OperatorType.Add:
                    return "+";
                case OperatorType.AddAssign:
                    return "+=";
                case OperatorType.And:
                    return "&";
                case OperatorType.AndAlso:
                    return "&&";
                case OperatorType.AndAssign:
                    return "&=";
                case OperatorType.Coalesce:
                    return "??";
                case OperatorType.Divide:
                    return "/";
                case OperatorType.DivideAssign:
                    return "/=";
                case OperatorType.Equal:
                    return "==";
                case OperatorType.GreaterThan:
                    return ">";
                case OperatorType.GreaterThanOrEqual:
                    return ">=";
                case OperatorType.LeftShift:
                    return "<<";
                case OperatorType.LeftShiftAssign:
                    return "<<=";
                case OperatorType.LessThan:
                    return "<";
                case OperatorType.LessThanOrEqual:
                    return "<=";
                case OperatorType.NotEqual:
                    return "!=";
                case OperatorType.Modulo:
                    return "%";
                case OperatorType.ModuloAssign:
                    return "%=";
                case OperatorType.Multiply:
                    return "*";
                case OperatorType.MultiplyAssign:
                    return "*=";
                case OperatorType.Negate:
                    return "-";
                case OperatorType.Not:
                    return "!";
                case OperatorType.Or:
                    return "|";
                case OperatorType.OrAssign:
                    return "|=";
                case OperatorType.OrElse:
                    return "||";
                case OperatorType.PreDecrement:
                    return "--";
                case OperatorType.PreIncrement:
                    return "++";
                case OperatorType.PostDecrement:
                    return "--";
                case OperatorType.PostIncrement:
                    return "++";
                case OperatorType.RightShift:
                    return ">>";
                case OperatorType.RightShiftAssign:
                    return ">>=";
                case OperatorType.Subtract:
                    return "-";
                case OperatorType.SubtractAssign:
                    return "-=";
                case OperatorType.Xor:
                    return "^";
                case OperatorType.XorAssign:
                    return "^=";
                default:
                    throw AssertionHelper.Fail();
            }
        }

        // http://msdn.microsoft.com/en-us/library/6a71f45d.aspx
        public static int CSharpPriority(this OperatorType opt)
        {
            switch (opt)
            {
                case OperatorType.Add:
                    return 12;
                case OperatorType.AddAssign:
                    return 2;
                case OperatorType.And:
                    return 8;
                case OperatorType.AndAlso:
                    return 5;
                case OperatorType.AndAssign:
                    return 2;
                case OperatorType.Coalesce:
                    return 1;
                case OperatorType.Divide:
                    return 13;
                case OperatorType.DivideAssign:
                    return 2;
                case OperatorType.Equal:
                    return 9;
                case OperatorType.GreaterThan:
                    return 10;
                case OperatorType.GreaterThanOrEqual:
                    return 10;
                case OperatorType.LeftShift:
                    return 11;
                case OperatorType.LeftShiftAssign:
                    return 2;
                case OperatorType.LessThan:
                    return 10;
                case OperatorType.LessThanOrEqual:
                    return 10;
                case OperatorType.NotEqual:
                    return 9;
                case OperatorType.Modulo:
                    return 13;
                case OperatorType.ModuloAssign:
                    return 2;
                case OperatorType.Multiply:
                    return 13;
                case OperatorType.MultiplyAssign:
                    return 2;
                case OperatorType.Negate:
                    return 14;
                case OperatorType.Not:
                    return 14;
                case OperatorType.Or:
                    return 6;
                case OperatorType.OrAssign:
                    return 2;
                case OperatorType.OrElse:
                    return 4;
                case OperatorType.PreDecrement:
                    return 14;
                case OperatorType.PreIncrement:
                    return 14;
                case OperatorType.PostDecrement:
                    return 15;
                case OperatorType.PostIncrement:
                    return 15;
                case OperatorType.RightShift:
                    return 11;
                case OperatorType.RightShiftAssign:
                    return 2;
                case OperatorType.Subtract:
                    return 12;
                case OperatorType.SubtractAssign:
                    return 2;
                case OperatorType.Xor:
                    return 7;
                case OperatorType.XorAssign:
                    return 2;
                default:
                    throw AssertionHelper.Fail();
            }
        }

        public static bool IsRelational(this OperatorType opt)
        {
            switch (opt)
            {
                case OperatorType.Equal:
                case OperatorType.GreaterThan:
                case OperatorType.GreaterThanOrEqual:
                case OperatorType.LessThan:
                case OperatorType.LessThanOrEqual:
                case OperatorType.NotEqual:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsEquality(this OperatorType opt)
        {
            switch (opt)
            {
                case OperatorType.Equal:
                case OperatorType.NotEqual:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsAssign(this OperatorType opt)
        {
            if (opt == OperatorType.PreDecrement || opt == OperatorType.PostDecrement ||
                opt == OperatorType.PreIncrement || opt == OperatorType.PostIncrement) return true;
            return opt.ToString().EndsWith("Assign");
        }

        public static OperatorType Unassign(this OperatorType opt)
        {
            if (opt.IsAssign())
            {
                if (opt == OperatorType.PreDecrement || opt == OperatorType.PostDecrement)
                    return OperatorType.Subtract;

                if (opt == OperatorType.PreIncrement || opt == OperatorType.PostIncrement)
                    return OperatorType.Add;

                var name = opt.ToString().Slice(0, -"Assign".Length);
                return (OperatorType)Enum.Parse(typeof(OperatorType), name);
            }
            else
            {
                return opt;
            }
        }

        public static OperatorType Assign(this OperatorType opt)
        {
            if (opt.IsAssign())
            {
                return opt;
            }
            else
            {
                var name = opt + "Assign";
                return (OperatorType)Enum.Parse(typeof(OperatorType), name);
            }
        }
    }
}