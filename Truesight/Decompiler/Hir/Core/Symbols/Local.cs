using System;
using System.Diagnostics;
using XenoGears.Strings;
using XenoGears.Traits.Cloneable;

namespace Truesight.Decompiler.Hir.Core.Symbols
{
    [DebuggerNonUserCode]
    public class Local : Sym
    {
        public Local(String name, Type type)
            : base(name, type)
        {
        }

        public new Local DeepClone()
        {
            return ((ICloneable2)this).DeepClone<Local>();
        }

        protected override String DumpImpl()
        {
            if (Type != null) return String.Format("Local '{0}' of type '{1}'", Name, Type.GetCSharpRef(ToCSharpOptions.Informative));
            else return String.Format("Local '{0}' of unknown type", Name);
        }
    }
}