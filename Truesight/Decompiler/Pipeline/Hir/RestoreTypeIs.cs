using System.Linq;
using Truesight.Decompiler.Hir.Core.ControlFlow;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Pipeline.Attrs;
using XenoGears.Functional;
using XenoGears.Traits.Hierarchy;
using Truesight.Decompiler.Hir.Traversal.Transformers;
using XenoGears.Assertions;

namespace Truesight.Decompiler.Pipeline.Hir
{
    [Decompiler(Weight = (int)Stages.PostprocessHir)]
    internal static class RestoreTypeIs
    {
        [DecompilationStep(Weight = 4)]
        public static Block DoRestoreTypeIs(Block hir)
        {
            if (hir.Family().OfType<TypeAs>().None()) return hir;
            else
            {
                return hir.Transform((Operator op) =>
                {
                    var is_rel = op.OperatorType.IsRelational();
                    if (is_rel)
                    {
                        var bin = op.AssertCast<BinaryOperator>();
                        var opt = bin.OperatorType;

                        var needs_xform = bin.Lhs is TypeAs ^ bin.Rhs is TypeAs;
                        if (needs_xform)
                        {
                            var type_as = bin.Lhs as TypeAs ?? bin.Rhs as TypeAs;
                            var other = bin.Lhs == type_as ? bin.Rhs : bin.Lhs;
                            other.AssertCast<Const>().AssertThat(c => c.Value == null);

                            var pos = opt == OperatorType.NotEqual || opt == OperatorType.GreaterThan;
                            var neg = opt == OperatorType.Equal || opt == OperatorType.LessThanOrEqual;
                            (pos || neg).AssertTrue();

                            var type_is = new TypeIs(type_as.Type, type_as.Target) as Expression;
                            return pos ? type_is : Operator.Negate(type_is);
                        }
                        else
                        {
                            return op.DefaultTransform();
                        }
                    }
                    else
                    {
                        return op.DefaultTransform();
                    }
                }).AssertCast<Block>();
            }
        }
    }
}