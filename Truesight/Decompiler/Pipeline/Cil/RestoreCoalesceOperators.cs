using Truesight.Decompiler.Pipeline.Attrs;
using Truesight.Decompiler.Pipeline.Flow.Cfg;

namespace Truesight.Decompiler.Pipeline.Cil
{
    [Decompiler(Weight = (int)Stages.PostprocessEarlyHir)]
    internal static class RestoreCoalesceOperators
    {
        [DecompilationStep(Weight = 5)]
        public static void DoRestoreCoalesceOperators(ControlFlowGraph cfg)
        {
            // todo. to be implemented
        }
    }
}