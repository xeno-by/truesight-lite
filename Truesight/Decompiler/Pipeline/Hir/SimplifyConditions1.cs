using System;
using System.Linq;
using Truesight.Decompiler.Hir;
using Truesight.Decompiler.Hir.Core.ControlFlow;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Traversal.Transformers;
using Truesight.Decompiler.Pipeline.Attrs;
using XenoGears.Functional;
using XenoGears.Assertions;

namespace Truesight.Decompiler.Pipeline.Hir
{
    [Decompiler(Weight = (int)Stages.PostprocessHir)]
    internal static class SimplifyConditions1
    {
        [DecompilationStep(Weight = 7)]
        public static Block DoSimplifyConditions(Block hir)
        {
            return hir.SimplifyConditions().AssertCast<Block>();
        }

        public static Node SimplifyConditions(this Node node)
        {
            return node.Transform((Operator op) => SimplifyConditions(op));
        }

        private static Node SimplifyConditions(this Operator op)
        {
            var opt = op.OperatorType;
            if (opt == OperatorType.Equal ||
                opt == OperatorType.NotEqual)
            {
                var c_false = op.Children.SingleOrDefault(c => c is Const &&
                    ((Const)c).Value is bool && (bool)((Const)c).Value == false);
                var c_true = op.Children.SingleOrDefault(c => c is Const &&
                    ((Const)c).Value is bool && (bool)((Const)c).Value == true);
                var cmpWithFalse = c_false != null;
                var cmpWithTrue = c_true != null;

                var equivToNegation = (cmpWithFalse && opt == OperatorType.Equal) ||
                    (cmpWithTrue && opt == OperatorType.NotEqual);
                if (equivToNegation)
                {
                    var redux = cmpWithTrue ? op.Children.Except(c_true).Single() : op.Children.Except(c_false).Single();
                    return SimplifyConditions((Operator)Operator.Not(redux.AssertCast<Expression>()));
                }

                var redundantLogicalOp = (cmpWithTrue && opt == OperatorType.Equal) ||
                    (cmpWithFalse && opt == OperatorType.NotEqual);
                if (redundantLogicalOp)
                {
                    var redux = cmpWithTrue ? op.Children.Except(c_true).Single() : op.Children.Except(c_false).Single();
                    return redux.CurrentTransform();
                }
            }

            if (opt == OperatorType.Not)
            {
                var child_op = op.Children.Single() as Operator;
                if (child_op != null)
                {
                    var copt = child_op.OperatorType;
                    if (copt == OperatorType.Not)
                    {
                        var redux = child_op.Args.Single();
                        return redux.CurrentTransform();
                    }

                    Func<OperatorType, OperatorType?> negate = op_type =>
                    {
                        switch (op_type)
                        {
                            case OperatorType.Equal:
                                return OperatorType.NotEqual;
                            case OperatorType.GreaterThan:
                                return OperatorType.LessThanOrEqual;
                            case OperatorType.GreaterThanOrEqual:
                                return OperatorType.LessThan;
                            case OperatorType.LessThan:
                                return OperatorType.GreaterThanOrEqual;
                            case OperatorType.LessThanOrEqual:
                                return OperatorType.GreaterThan;
                            case OperatorType.NotEqual:
                                return OperatorType.Equal;
                            default:
                                return null;
                        }
                    };

                    var negated = negate(copt);
                    if (negated != null)
                    {
                        var equiv = Operator.Create(negated.Value, child_op.Args);
                        return equiv.SimplifyConditions();
                    }

                    var child_bop = child_op.AssertCast<BinaryOperator>();
                    var child_lhs = child_bop == null ? null : child_bop.Lhs;
                    var child_rhs = child_bop == null ? null : child_bop.Rhs;
                    if (copt == OperatorType.AndAlso)
                    {
                        var equiv = Operator.OrElse(Operator.Not(child_lhs), Operator.Not(child_rhs));
                        return equiv.SimplifyConditions();
                    }
                    else if (copt == OperatorType.OrElse)
                    {
                        var equiv = Operator.AndAlso(Operator.Not(child_lhs), Operator.Not(child_rhs));
                        return equiv.SimplifyConditions();
                    }
                    else if (copt == OperatorType.Xor)
                    {
                        var equiv = Operator.Xor(Operator.Not(child_lhs), child_rhs);
                        return equiv.SimplifyConditions();
                    }
                }
            }

            if (opt == OperatorType.Xor)
            {
                var bop = op.AssertCast<BinaryOperator>();
                var lhs = bop == null ? null : bop.Lhs as Operator;
                var rhs = bop == null ? null : bop.Rhs as Operator;

                if (lhs != null && lhs.OperatorType == OperatorType.Not &&
                    rhs != null && rhs.OperatorType == OperatorType.Not)
                {
                    var equiv = Operator.Xor(lhs.Args.Single(), rhs.Args.Single());
                    return equiv.SimplifyConditions();
                }
            }

            return (Operator)op.DefaultTransform();
        }
    }
}
