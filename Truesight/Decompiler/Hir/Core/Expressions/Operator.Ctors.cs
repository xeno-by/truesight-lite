using System;
using System.Collections.Generic;
using System.Linq;
using XenoGears.Functional;
using XenoGears.Assertions;
using CilOperatorType = Truesight.Parser.Api.Ops.OperatorType;
using HirOperatorType = Truesight.Decompiler.Hir.Core.Expressions.OperatorType;

namespace Truesight.Decompiler.Hir.Core.Expressions
{
    public abstract partial class Operator
    {
        public static Operator Create(OperatorType operatorType)
        {
            return Create(operatorType, operatorType.Arity().Times((Expression)null));
        }

        public static Operator Create(CilOperatorType operatorType)
        {
            var hirOperatorType = (OperatorType)Enum.Parse(typeof(OperatorType), operatorType.ToString());
            return Create(hirOperatorType);
        }

        public static Operator Create(OperatorType operatorType, params Expression[] children)
        {
            if (operatorType.Arity() == 1)
            {
                (children.Count() == 1).AssertTrue();
                return new UnaryOperator(operatorType, children.First());
            }
            else if (operatorType.Arity() == 2)
            {
                (children.Count() == 2).AssertTrue();
                return new BinaryOperator(operatorType, children.First(), children.Second());
            }
            else
            {
                throw AssertionHelper.Fail();
            }
        }

        public static Operator Create(CilOperatorType operatorType, params Expression[] children)
        {
            var hirOperatorType = (OperatorType)Enum.Parse(typeof(OperatorType), operatorType.ToString());
            return Create(hirOperatorType, children);
        }

        public static Operator Create(OperatorType operatorType, IEnumerable<Expression> children)
        {
            return Create(operatorType, children.ToArray());
        }

        public static Operator Create(CilOperatorType operatorType, IEnumerable<Expression> children)
        {
            var hirOperatorType = (OperatorType)Enum.Parse(typeof(OperatorType), operatorType.ToString());
            return Create(hirOperatorType, children);
        }

        public static BinaryOperator Add()
        {
            return new BinaryOperator(OperatorType.Add);
        }

        public static BinaryOperator Add(Expression lhs, Expression rhs)
        {
            return new BinaryOperator(OperatorType.Add, lhs, rhs);
        }

        public static BinaryOperator AddAssign()
        {
            return new BinaryOperator(OperatorType.AddAssign);
        }

        public static BinaryOperator AddAssign(Expression lhs, Expression rhs)
        {
            return new BinaryOperator(OperatorType.AddAssign, lhs, rhs);
        }

        public static BinaryOperator And()
        {
            return new BinaryOperator(OperatorType.And);
        }

        public static BinaryOperator And(Expression lhs, Expression rhs)
        {
            return new BinaryOperator(OperatorType.And, lhs, rhs);
        }

        public static BinaryOperator AndAlso()
        {
            return new BinaryOperator(OperatorType.AndAlso);
        }

        public static BinaryOperator AndAlso(Expression lhs, Expression rhs)
        {
            return new BinaryOperator(OperatorType.AndAlso, lhs, rhs);
        }

        public static BinaryOperator AndAssign()
        {
            return new BinaryOperator(OperatorType.AndAssign);
        }

        public static BinaryOperator AndAssign(Expression lhs, Expression rhs)
        {
            return new BinaryOperator(OperatorType.AndAssign, lhs, rhs);
        }

        public static BinaryOperator Divide()
        {
            return new BinaryOperator(OperatorType.Divide);
        }

        public static BinaryOperator Divide(Expression lhs, Expression rhs)
        {
            return new BinaryOperator(OperatorType.Divide, lhs, rhs);
        }

        public static BinaryOperator Coalesce()
        {
            return new BinaryOperator(OperatorType.Coalesce);
        }

        public static BinaryOperator Coalesce(Expression lhs, Expression rhs)
        {
            return new BinaryOperator(OperatorType.Coalesce, lhs, rhs);
        }

        public static BinaryOperator DivideAssign()
        {
            return new BinaryOperator(OperatorType.DivideAssign);
        }

        public static BinaryOperator DivideAssign(Expression lhs, Expression rhs)
        {
            return new BinaryOperator(OperatorType.DivideAssign, lhs, rhs);
        }

        public static BinaryOperator Equal()
        {
            return new BinaryOperator(OperatorType.Equal);
        }

        public static BinaryOperator Equal(Expression lhs, Expression rhs)
        {
            return new BinaryOperator(OperatorType.Equal, lhs, rhs);
        }

        public static BinaryOperator GreaterThan()
        {
            return new BinaryOperator(OperatorType.GreaterThan);
        }

        public static BinaryOperator GreaterThan(Expression lhs, Expression rhs)
        {
            return new BinaryOperator(OperatorType.GreaterThan, lhs, rhs);
        }

        public static BinaryOperator GreaterThanOrEqual()
        {
            return new BinaryOperator(OperatorType.GreaterThanOrEqual);
        }

        public static BinaryOperator GreaterThanOrEqual(Expression lhs, Expression rhs)
        {
            return new BinaryOperator(OperatorType.GreaterThanOrEqual, lhs, rhs);
        }

        public static BinaryOperator LeftShift()
        {
            return new BinaryOperator(OperatorType.LeftShift);
        }

        public static BinaryOperator LeftShift(Expression lhs, Expression rhs)
        {
            return new BinaryOperator(OperatorType.LeftShift, lhs, rhs);
        }

        public static BinaryOperator LeftShiftAssign()
        {
            return new BinaryOperator(OperatorType.LeftShiftAssign);
        }

        public static BinaryOperator LeftShiftAssign(Expression lhs, Expression rhs)
        {
            return new BinaryOperator(OperatorType.LeftShiftAssign, lhs, rhs);
        }

        public static BinaryOperator LessThan()
        {
            return new BinaryOperator(OperatorType.LessThan);
        }

        public static BinaryOperator LessThan(Expression lhs, Expression rhs)
        {
            return new BinaryOperator(OperatorType.LessThan, lhs, rhs);
        }

        public static BinaryOperator LessThanOrEqual()
        {
            return new BinaryOperator(OperatorType.LessThanOrEqual);
        }

        public static BinaryOperator LessThanOrEqual(Expression lhs, Expression rhs)
        {
            return new BinaryOperator(OperatorType.LessThanOrEqual, lhs, rhs);
        }

        public static BinaryOperator NotEqual()
        {
            return new BinaryOperator(OperatorType.NotEqual);
        }

        public static BinaryOperator NotEqual(Expression lhs, Expression rhs)
        {
            return new BinaryOperator(OperatorType.NotEqual, lhs, rhs);
        }

        public static BinaryOperator Modulo()
        {
            return new BinaryOperator(OperatorType.Modulo);
        }

        public static BinaryOperator Modulo(Expression lhs, Expression rhs)
        {
            return new BinaryOperator(OperatorType.Modulo, lhs, rhs);
        }

        public static BinaryOperator ModuloAssign()
        {
            return new BinaryOperator(OperatorType.ModuloAssign);
        }

        public static BinaryOperator ModuloAssign(Expression lhs, Expression rhs)
        {
            return new BinaryOperator(OperatorType.ModuloAssign, lhs, rhs);
        }

        public static BinaryOperator Multiply()
        {
            return new BinaryOperator(OperatorType.Multiply);
        }

        public static BinaryOperator Multiply(Expression lhs, Expression rhs)
        {
            return new BinaryOperator(OperatorType.Multiply, lhs, rhs);
        }

        public static BinaryOperator MultiplyAssign()
        {
            return new BinaryOperator(OperatorType.MultiplyAssign);
        }

        public static BinaryOperator MultiplyAssign(Expression lhs, Expression rhs)
        {
            return new BinaryOperator(OperatorType.MultiplyAssign, lhs, rhs);
        }

        public static UnaryOperator Negate()
        {
            return new UnaryOperator(OperatorType.Negate);
        }

        public static UnaryOperator Negate(Expression target)
        {
            return new UnaryOperator(OperatorType.Negate, target);
        }

        public static UnaryOperator Not()
        {
            return new UnaryOperator(OperatorType.Not);
        }

        public static UnaryOperator Not(Expression target)
        {
            return new UnaryOperator(OperatorType.Not, target);
        }

        public static BinaryOperator Or()
        {
            return new BinaryOperator(OperatorType.Or);
        }

        public static BinaryOperator Or(Expression lhs, Expression rhs)
        {
            return new BinaryOperator(OperatorType.Or, lhs, rhs);
        }

        public static BinaryOperator OrAssign()
        {
            return new BinaryOperator(OperatorType.OrAssign);
        }

        public static BinaryOperator OrAssign(Expression lhs, Expression rhs)
        {
            return new BinaryOperator(OperatorType.OrAssign, lhs, rhs);
        }

        public static BinaryOperator OrElse()
        {
            return new BinaryOperator(OperatorType.OrElse);
        }

        public static BinaryOperator OrElse(Expression lhs, Expression rhs)
        {
            return new BinaryOperator(OperatorType.OrElse, lhs, rhs);
        }

        public static UnaryOperator PreDecrement()
        {
            return new UnaryOperator(OperatorType.PreDecrement);
        }

        public static UnaryOperator PreDecrement(Expression target)
        {
            return new UnaryOperator(OperatorType.PreDecrement, target);
        }

        public static UnaryOperator PreIncrement()
        {
            return new UnaryOperator(OperatorType.PreIncrement);
        }

        public static UnaryOperator PreIncrement(Expression target)
        {
            return new UnaryOperator(OperatorType.PreIncrement, target);
        }

        public static UnaryOperator PostDecrement()
        {
            return new UnaryOperator(OperatorType.PostDecrement);
        }

        public static UnaryOperator PostDecrement(Expression target)
        {
            return new UnaryOperator(OperatorType.PostDecrement, target);
        }

        public static UnaryOperator PostIncrement()
        {
            return new UnaryOperator(OperatorType.PostIncrement);
        }

        public static UnaryOperator PostIncrement(Expression target)
        {
            return new UnaryOperator(OperatorType.PostIncrement, target);
        }

        public static BinaryOperator RightShift()
        {
            return new BinaryOperator(OperatorType.RightShift);
        }

        public static BinaryOperator RightShift(Expression lhs, Expression rhs)
        {
            return new BinaryOperator(OperatorType.RightShift, lhs, rhs);
        }

        public static BinaryOperator RightShiftAssign()
        {
            return new BinaryOperator(OperatorType.RightShiftAssign);
        }

        public static BinaryOperator RightShiftAssign(Expression lhs, Expression rhs)
        {
            return new BinaryOperator(OperatorType.RightShiftAssign, lhs, rhs);
        }

        public static BinaryOperator Subtract()
        {
            return new BinaryOperator(OperatorType.Subtract);
        }

        public static BinaryOperator Subtract(Expression lhs, Expression rhs)
        {
            return new BinaryOperator(OperatorType.Subtract, lhs, rhs);
        }

        public static BinaryOperator SubtractAssign()
        {
            return new BinaryOperator(OperatorType.SubtractAssign);
        }

        public static BinaryOperator SubtractAssign(Expression lhs, Expression rhs)
        {
            return new BinaryOperator(OperatorType.SubtractAssign, lhs, rhs);
        }

        public static BinaryOperator Xor()
        {
            return new BinaryOperator(OperatorType.Xor);
        }

        public static BinaryOperator Xor(Expression lhs, Expression rhs)
        {
            return new BinaryOperator(OperatorType.Xor, lhs, rhs);
        }

        public static BinaryOperator XorAssign()
        {
            return new BinaryOperator(OperatorType.XorAssign);
        }

        public static BinaryOperator XorAssign(Expression lhs, Expression rhs)
        {
            return new BinaryOperator(OperatorType.XorAssign, lhs, rhs);
        }
    }
}