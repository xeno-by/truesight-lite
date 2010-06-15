using System.Diagnostics;

namespace Truesight.Decompiler.Hir.Core.Symbols
{
    [DebuggerNonUserCode]
    public static class SymExtensions
    {
        public static bool IsLocal(this Sym sym)
        {
            return sym is Local;
        }

        public static bool IsParam(this Sym sym)
        {
            return sym is Param;
        }
    }
}