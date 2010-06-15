using System;
using System.Diagnostics;
using XenoGears.Strings;
using XenoGears.Traits.Cloneable;

namespace Truesight.Decompiler.Hir.Core.Symbols
{
    [DebuggerNonUserCode]
    public class Param : Sym
    {
        public Param(String name, Type type)
            : base(name, type)
        {
        }

        public new Param DeepClone()
        {
            return ((ICloneable2)this).DeepClone<Param>();
        }

        protected override String DumpImpl()
        {
            return String.Format("Param '{0}' of type '{1}'", 
                Name, Type.GetCSharpRef(ToCSharpOptions.Informative));
        }
    }
}