using Truesight.Decompiler.Hir.Core.ControlFlow;
using Truesight.Decompiler.Pipeline.Attrs;

namespace Truesight.Decompiler.Pipeline.Hir
{
    [Decompiler(Weight = (int)Stages.PostprocessHir)]
    internal static class RestoreIters
    {
        [DecompilationStep(Weight = 6)]
        public static Block DoRestoreIters(Block hir)
        {
            // todo. to be implemented
            return hir;
        }
    }
}