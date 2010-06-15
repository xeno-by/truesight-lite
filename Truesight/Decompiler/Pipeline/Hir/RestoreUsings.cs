using Truesight.Decompiler.Hir.Core.ControlFlow;
using Truesight.Decompiler.Pipeline.Attrs;

namespace Truesight.Decompiler.Pipeline.Hir
{
    [Decompiler(Weight = (int)Stages.PostprocessHir)]
    internal static class RestoreUsings
    {
        [DecompilationStep(Weight = 5)]
        public static Block DoRestoreUsings(Block hir)
        {
            // todo. to be implemented
            return hir;
        }
    }
}