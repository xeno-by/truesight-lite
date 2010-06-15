using System.Linq;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Pipeline.Attrs;
using Truesight.Decompiler.Pipeline.Flow.Cfg;
using XenoGears.Functional;
using XenoGears.Assertions;

namespace Truesight.Decompiler.Pipeline.Flow
{
    [Decompiler(Weight = (int)Stages.PostprocessCfg)]
    internal static class RemoveReturnThunk
    {
        [DecompilationStep(Weight = 4)]
        public static void DoRemoveReturnThunk(ControlFlowGraph cfg)
        {
            var preRets = cfg.Vedges(null, cfg.Finish);
            if (preRets.Count() > 1) return;

            var wannabe = preRets.AssertSingle().Source;
            if (wannabe.BalancedCode.IsEmpty() &&
                wannabe.Residue.SingleOrDefault() is Ref)
            {
                var retThunk = wannabe;
                retThunk.BalancedCode.AssertEmpty();
                var auxLocal = retThunk.Residue.AssertSingle().AssertCast<Ref>().Sym;

                var rets = cfg.Vedges(null, retThunk);
                rets.AssertEach(ret => ret.Tag == null);

                cfg.RemoveVertex(retThunk);
                rets.ForEach(ret =>
                {
                    var src = ret.Source;
                    src.Residue.AssertEmpty();

                    var ass = src.BalancedCode.Last().AssertCast<Assign>();
                    var lhs = ass.Lhs.AssertCast<Ref>();
                    var rhs = ass.Rhs.AssertCast<Expression>();
                    (lhs.Sym.ProtoId == auxLocal.ProtoId).AssertTrue();

                    cfg.AddEdge(new ControlFlowEdge(src, cfg.Finish));
                    src.BalancedCode.RemoveLast();
                    src.Residue.Add(rhs);
                });
            }
        }
    }
}