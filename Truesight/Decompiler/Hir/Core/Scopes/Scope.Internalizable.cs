using System.Collections.Generic;
using System.Collections.ObjectModel;
using XenoGears.Traits.Hierarchy;

// note. this boilerplate is necessary in order to expose XenoGears' API to outer world
// the problem is that when we ilmerge and internalize XenoGears
// then all of a sudden a bunch of APIs become unavailable to our users

namespace Truesight.Decompiler.Hir.Core.Scopes
{
    public partial interface Scope
    {
        new Scope Parent { get; }
        new ReadOnlyCollection<Scope> Children { get; }

        new int Index { get; }
        new Scope Prev { get; }
        new Scope Next { get; }
    }

    public static partial class ScopeExtensions
    {
        public static IEnumerable<Scope> Children2(this Scope node)
        {
            return IHierarchyExtensions.Children2(node);
        }

        public static ReadOnlyCollection<Scope> Parents2(this Scope node)
        {
            return IHierarchyExtensions.Parents2(node);
        }

        public static ReadOnlyCollection<Scope> Hierarchy2(this Scope node)
        {
            return IHierarchyExtensions.Hierarchy2(node);
        }

        public static ReadOnlyCollection<Scope> ChildrenRecursive2(this Scope node)
        {
            return IHierarchyExtensions.ChildrenRecursive2(node);
        }

        public static ReadOnlyCollection<Scope> Family2(this Scope node)
        {
            return IHierarchyExtensions.Family2(node);
        }
    }

}
