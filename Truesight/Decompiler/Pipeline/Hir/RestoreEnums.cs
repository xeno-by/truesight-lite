using Truesight.Decompiler.Hir.Core.ControlFlow;
using Truesight.Decompiler.Pipeline.Attrs;

namespace Truesight.Decompiler.Pipeline.Hir
{
    [Decompiler(Weight = (int)Stages.PostprocessHir)]
    internal static class RestoreEnums
    {
        [DecompilationStep(Weight = 3)]
        public static Block DoRestoreEnums(Block hir)
        {
            // todo. to be implemented
            return hir;
        }
    }
}