using Truesight.Decompiler.Hir.Core.ControlFlow;
using Truesight.Decompiler.Pipeline.Flow.Cfg;

namespace Truesight.Decompiler.Pipeline.Flow.Scopes
{
    internal static class ScopeApi
    {
        public static Block DecompileScopes(this ControlFlowGraph cfg)
        {
            return LambdaScope.Decompile(cfg).Hir;
        }
    }
}
