using Truesight.Decompiler.Pipeline.Attrs;
using Truesight.Decompiler.Pipeline.Flow.Cfg;

namespace Truesight.Decompiler.Pipeline.Cil
{
    [Decompiler(Weight = (int)Stages.PostprocessEarlyHir)]
    internal static class RestoreConditionalOperators
    {
        [DecompilationStep(Weight = 4)]
        public static void DoRestoreConditionalOperators(ControlFlowGraph cfg)
        {
            // todo. to be implemented
        }
    }
}