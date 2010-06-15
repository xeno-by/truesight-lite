using System.Diagnostics;
using Truesight.Decompiler.Hir;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Core.Functional;
using XenoGears.Functional;

namespace Truesight.Decompiler.Pipeline.Cil.OpAssign
{
    [DebuggerNonUserCode]
    internal static class AtomHelper
    {
        public static bool IsAtom(this Node n)
        {
            if (n is Ref) return true;
            if (n is Fld) return ((Fld)n).This is Ref;
            if (n is Prop) return ((Prop)n).This is Ref && ((Prop)n).Property.GetIndexParameters().IsEmpty();
            if (n is Apply) return ((Apply)n).Callee is Prop;
            return false;
        }
    }
}
