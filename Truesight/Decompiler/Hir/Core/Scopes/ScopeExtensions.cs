using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Truesight.Decompiler.Hir.Core.Symbols;
using XenoGears.Functional;
using XenoGears.Traits.Hierarchy;

namespace Truesight.Decompiler.Hir.Core.Scopes
{
    [DebuggerNonUserCode]
    public static class ScopeExtensions
    {
        public static ReadOnlyCollection<Local> LocalsRecursive(this Scope scope)
        {
            return scope.Family2().SelectMany(b => b.Locals).ToReadOnly();
        }
    }
}