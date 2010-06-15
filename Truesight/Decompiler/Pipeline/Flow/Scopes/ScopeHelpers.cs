using System.Collections.ObjectModel;
using XenoGears.Functional;

namespace Truesight.Decompiler.Pipeline.Flow.Scopes
{
    internal static class ScopeHelpers
    {
        public static ReadOnlyCollection<IScope> Parents(this IScope scope)
        {
            return scope.Unfolde(s => s.Parent, s => s != null).ToReadOnly();
        }

        public static ReadOnlyCollection<IScope> Hierarchy(this IScope scope)
        {
            return scope.Unfoldi(s => s.Parent, s => s != null).ToReadOnly();
        }
    }
}