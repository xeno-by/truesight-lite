using Truesight.Decompiler.Pipeline.Attrs;
using Truesight.Decompiler.Pipeline.Flow.Cfg;

namespace Truesight.Decompiler.Pipeline.Cil
{
    internal static class RestoreObjectInitializers
    {
        [DecompilationStep(Weight = 1)]
        public static void DoRestoreObjectInitializers(ControlFlowGraph cfg)
        {
            // todo. to be implemented
        }
    }
}
