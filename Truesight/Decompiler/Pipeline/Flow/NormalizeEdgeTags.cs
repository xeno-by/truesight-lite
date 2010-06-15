using System;
using System.Linq;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Pipeline.Attrs;
using Truesight.Decompiler.Pipeline.Flow.Cfg;
using XenoGears.Functional;
using XenoGears.Assertions;

namespace Truesight.Decompiler.Pipeline.Flow
{
    [Decompiler(Weight = (int)Stages.PostprocessCfg)]
    internal static class NormalizeEdgeTags
    {
        [DecompilationStep(Weight = 2)]
        public static void DoNormalizeEdgeTags(ControlFlowGraph cfg)
        {
            cfg.Vertices.AssertEach(v => cfg.Vedges(v, null).Count() <= 2);
            var binaryCondEdges = cfg.Edges().Where(e => e.Tag.Arity() == 2);
            var sourceVertices = binaryCondEdges.Select(e => e.Source).Distinct();
            sourceVertices.ForEach(v =>
            {
                var outEdges = cfg.Vedges(v, null);
                outEdges.AssertEach(e => e.Tag.Arity() == 2);
                var operatorType = outEdges.First().Tag.Value.ToOperatorType();
                var newTags = outEdges.Select(e =>
                    e.Tag == operatorType.ToPredicateType() ? PredicateType.IsTrue :
                    e.Tag == operatorType.ToPredicateType().Negate() ? PredicateType.IsFalse :
                    ((Func<PredicateType>)(() => { throw AssertionHelper.Fail(); }))());

                var joint = Operator.Create(operatorType, v.Residue);
                v.Residue.SetElements(joint);
                outEdges.Zip(newTags).ForEach(e => e.Item1.Tag = e.Item2);
            });
            cfg.Edges().AssertEach(e => e.Tag.Arity() <= 1);
            cfg.Vertices.Where(v => v.Residue.IsNotEmpty()).AssertEach(v => v.Residue.Count() == 1);
        }
    }
}