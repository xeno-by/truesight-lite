using Truesight.Decompiler.Hir.Core.Symbols;
using XenoGears.Collections.Observable;
using XenoGears.Traits.Hierarchy;

namespace Truesight.Decompiler.Hir.Core.Scopes
{
    public partial interface Scope : IImmutableHierarchy2<Scope>
    {
        IObservableList<Local> Locals { get; }
    }
}