using System.Linq;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Pipeline.Attrs;
using Truesight.Decompiler.Pipeline.Flow.Cfg;
using XenoGears.Functional;
using XenoGears.Assertions;

namespace Truesight.Decompiler.Pipeline.Flow
{
    [Decompiler(Weight = (int)Stages.PostprocessCfg)]
    internal static class SplitBlocksIntoAssignmentsAndPredicates
    {
        [DecompilationStep(Weight = 6)]
        public static void DoSplitBlocksIntoAssignmentsAndPredicates(ControlFlowGraph cfg)
        {
            cfg.Vertices.ForEach(v =>
            {
                if (v.BalancedCode.IsEmpty() || v.Residue.IsEmpty()) return;

                var ass = (v.BalancedCode.Last() is Assign) ? v.BalancedCode.Last().AssertCast<Assign>() : null;
                var lhs = ass == null ? null : ass.Lhs.AssertCast<Ref>().Sym;
                var @ref = v.Residue.AssertSingle() is Ref ? v.Residue.AssertSingle().AssertCast<Ref>().Sym : null;
                if (lhs != null && @ref != null && lhs.ProtoId == @ref.ProtoId)
                {
                    // todo. this introduces a nasty bug if assigned variable is reused later
                    v.BalancedCode.RemoveLast();
                    v.Residue.SetElements(ass.Rhs);
                }

                if (v.BalancedCode.IsEmpty() || v.Residue.IsEmpty()) return;

                var v_test = new ControlFlowBlock();
                v_test.Residue.Add(v.Residue.AssertSingle());
                v.Residue.RemoveElements();

                var outEdges = cfg.Vedges(v, null);
                cfg.RemoveEdges(outEdges);

                cfg.AddVertex(v_test);
                cfg.AddEdge(new ControlFlowEdge(v, v_test));
                outEdges.ForEach(e => cfg.AddEdge(new ControlFlowEdge(v_test, e.Target, e.Tag)));
            });
        }
    }
}