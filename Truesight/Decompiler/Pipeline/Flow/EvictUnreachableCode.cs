using System.Linq;
using Truesight.Decompiler.Pipeline.Attrs;
using Truesight.Decompiler.Pipeline.Flow.Cfg;
using XenoGears.Assertions;

namespace Truesight.Decompiler.Pipeline.Flow
{
    [Decompiler(Weight = (int)Stages.PostprocessCfg)]
    internal static class EvictUnreachableCode
    {
        [DecompilationStep(Weight = 1)]
        public static void DoEvictUnreachableCode(ControlFlowGraph cfg)
        {
            // todo. this still doesn't completely fix code like "while(false)"
            var unreachable = cfg.Vertices.Except(cfg.Cflow());
            unreachable.Contains(cfg.Finish).AssertFalse();
            cfg.RemoveVertices(unreachable);
        }
    }
}
