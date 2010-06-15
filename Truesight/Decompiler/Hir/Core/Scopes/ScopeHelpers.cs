using System.Diagnostics;
using System.Linq;
using XenoGears.Traits.Hierarchy;

namespace Truesight.Decompiler.Hir.Core.Scopes
{
    [DebuggerNonUserCode]
    public static class ScopeHelpers
    {
        public static Scope Scope(this Node n)
        {
            return n.Hierarchy().OfType<Scope>().FirstOrDefault();
        }

        public static Scope ParentScope(this Node n)
        {
            return n.Parents().OfType<Scope>().FirstOrDefault();
        }
    }
}