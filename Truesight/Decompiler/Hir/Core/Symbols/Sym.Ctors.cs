using System;

namespace Truesight.Decompiler.Hir.Core.Symbols
{
    public partial class Sym
    {
        public static Local Local(String name, Type type)
        {
            return new Local(name, type);
        }

        public static Param Param(String name, Type type)
        {
            return new Param(name, type);
        }
    }
}
