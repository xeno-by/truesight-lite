using System;
using System.Linq;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Core.Functional;
using Truesight.Decompiler.Hir.Traversal;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Reflection;
using XenoGears.Reflection.Generics;
using Convert = Truesight.Decompiler.Hir.Core.Expressions.Convert;

namespace Truesight.Decompiler.Hir.Prettyprint
{
    public static class CSharpParenthesesHelper
    {
        public static bool NeedsParenthesesInCSharp(this Expression child)
        {
            if (child == null) return false;
            return child.NeedsParenthesesInCSharp(child.Parent as Expression);
        }

        private static bool NeedsParenthesesInCSharp(this Expression child, Expression parent)
        {
            if (parent is Eval) parent = ((Eval)parent).Parent as Expression;
            if (child == null || parent == null) return false;

            var p_app = parent as Apply;
            if (p_app != null)
            {
                if (p_app.Callee == child)
                {
                    return false;
                }
                else
                {
                    var m = p_app.InvokedMethod();
                    if (m != null)
                    {
                        var reallySpecial = m.IsSpecialName && !m.IsConstructor && m.EnclosingProperty() == null;
                        var childIsFirstArg = p_app.Args.FirstOrDefault() == child;
                        var explicitThis = m.IsInstance() || m.IsExtension();
                        if (!reallySpecial && (!childIsFirstArg || !explicitThis)) return false;
                    }
                    else
                    {
                        var p = p_app.InvokedProperty();
                        if (p != null)
                        {
                            return false;
                        }
                        else
                        {
                            (p_app.Callee == null).AssertTrue();
                            return false;
                        }
                    }
                }
            }

            if (child is Apply)
            {
                var m = child.InvokedMethod();
                if (m != null)
                {
                    if (m.IsUserDefinedOperator())
                    {
                        // todo. add support for op_UnaryPlus
                        var op_type = (OperatorType)Enum.Parse(typeof(OperatorType), m.UserDefinedOperatorType());
                        var equiv = Operator.Create(op_type, ((Apply)child).Args);
                        return equiv.NeedsParenthesesInCSharp(parent);
                    }
                    else if (m.IsUserDefinedCast())
                    {
                        (m.Paramc() == 1).AssertTrue();
                        var targetType = m.Ret();
                        var source = ((Apply)child).Args.Single();

                        // todo. think about cases when we can omit the cast
                        // and, thus, need other algorithm of determining
                        // whether we need parentheses or not
                        var equiv = new Convert(targetType, source);
                        return equiv.NeedsParenthesesInCSharp(parent);
                    }
                }
            }

            // heuristics: this makes prettyprints more readable
            // todo. some day think this over again
            // check out examples in Truesight.Playground\Decompiler\Reference
            // for more thinking material
            if (parent is Operator && child is Operator)
            {
                var p_op = ((Operator)parent).OperatorType;
                var c_op = ((Operator)child).OperatorType;

                if (c_op.IsRelational() && p_op.IsEquality()) return true;

                if (p_op == c_op) return false;

                if (c_op == OperatorType.PostDecrement ||
                    c_op == OperatorType.PostIncrement ||
                    c_op == OperatorType.PreDecrement ||
                    c_op == OperatorType.PreIncrement ||
                    c_op == OperatorType.Negate ||
                    c_op == OperatorType.Not)
                {
                    return false;
                }

                if (p_op == OperatorType.AndAlso ||
                    p_op == OperatorType.OrElse)
                {
                    return true;
                }
            }

            if (child is Operator)
            {
                if (child.CSharpPriority() == Operator.PreIncrement().CSharpPriority())
                {
                    return child.CSharpPriority() < parent.CSharpPriority();
                }
                else
                {
                    return child.CSharpPriority() <= parent.CSharpPriority();
                }
            }
            else if (child is Assign)
            {
                return child.CSharpPriority() <= parent.CSharpPriority();
            }
            else if (child is Addr)
            {
                return child.CSharpPriority() <= parent.CSharpPriority();
            }
            else if (child is CollectionInit)
            {
                return false;
            }
            else if (child is Conditional)
            {
                return child.CSharpPriority() <= parent.CSharpPriority();
            }
            else if (child is Const)
            {
                return false;
            }
            else if (child is Convert)
            {
                return child.CSharpPriority() <= parent.CSharpPriority() && !(parent is Convert);
            }
            else if (child is Deref)
            {
                return child.CSharpPriority() <= parent.CSharpPriority();
            }
            else if (child is Slot)
            {
                return false;
            }
            else if (child is Loophole)
            {
                return false;
            }
            else if (child is ObjectInit)
            {
                return false;
            }
            else if (child is Ref)
            {
                return false;
            }
            else if (child is SizeOf)
            {
                return false;
            }
            else if (child is TypeAs)
            {
                return child.CSharpPriority() <= parent.CSharpPriority();
            }
            else if (child is TypeIs)
            {
                return child.CSharpPriority() <= parent.CSharpPriority();
            }
            else if (child is Apply)
            {
                return false;
            }
            else if (child is Eval)
            {
                return false;
            }
            else if (child is Lambda)
            {
                return false;
            }
            else
            {
                throw AssertionHelper.Fail();
            }
        }

        private static int CSharpPriority(this Expression e)
        {
            var prios = Enum.GetValues(typeof(OperatorType)).Cast<OperatorType>()
                .ToDictionary(ot => ot, ot => ot.CSharpPriority()).ToReadOnly();
            var lessThanMin = prios.Values.Min() - 1;
            var assignPrio = prios[OperatorType.AddAssign];
            var condPrio = 3;
            var relPrio = prios[OperatorType.GreaterThan];
            var unaryPrio = prios[OperatorType.PreIncrement];
            var primaryPrio = prios[OperatorType.PostIncrement];
            (unaryPrio + 1 == primaryPrio).AssertTrue();

            if (e == null)
            {
                return primaryPrio;
            }
            else if (e is Addr)
            {
                return unaryPrio;
            }
            else if (e is Assign)
            {
                return assignPrio;
            }
            else if (e is CollectionInit)
            {
                return primaryPrio;
            }
            else if (e is Conditional)
            {
                return condPrio;
            }
            else if (e is Deref)
            {
                return unaryPrio;
            }
            else if (e is Const)
            {
                return primaryPrio;
            }
            else if (e is Convert)
            {
                return unaryPrio;
            }
            else if (e is Loophole)
            {
                return primaryPrio;
            }
            else if (e is ObjectInit)
            {
                return primaryPrio;
            }
            else if (e is Operator)
            {
                return prios[((Operator)e).OperatorType];
            }
            else if (e is Ref)
            {
                return primaryPrio;
            }
            else if (e is SizeOf)
            {
                return unaryPrio;
            }
            else if (e is Slot)
            {
                return primaryPrio;
            }
            else if (e is TypeAs)
            {
                return relPrio;
            }
            else if (e is TypeIs)
            {
                return relPrio;
            }
            else if (e is Apply)
            {
                return primaryPrio;
            }
            else if (e is Eval)
            {
                return primaryPrio;
            }
            else if (e is Lambda)
            {
                return lessThanMin;
            }
            else
            {
                throw AssertionHelper.Fail();
            }
        }
    }
}