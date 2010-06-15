using System;
using System.Diagnostics;
using System.Reflection;
using Truesight.Decompiler.Hir.Core.Symbols;
using XenoGears.Assertions;
using XenoGears.Functional;
using XenoGears.Strings;

namespace Truesight.Decompiler.Hir.Core.Functional
{
    [DebuggerNonUserCode]
    public class ParamInfo
    {
        public Sig Sig { get; private set; }
        public int Index { get; private set; }
        public String Name { get; private set; }
        public Type Type { get; private set; }
        public ParameterInfo Metadata { get; private set; }
        public Param Sym { get { return Sig.Syms[Index]; } }

        public ParamInfo(Sig sig, int index)
        {
            Sig = sig;
            Index = index;
            Name = Sym.Name;
            Type = Sym.Type;
        }

        // note. we need this constructor to provide information
        // about method's parameters without having to decompile it
        //
        // the point is that to get sig's symbols we've got to know their ids
        // but the ids need to be synchronized with Refs in decompiled body
        // that's why a simple call to Sig::Syms need to decompile the entire body
        public ParamInfo(Sig sig, int index, String name, Type type)
        {
            Sig = sig;
            Index = index;
            Name = name.IsNullOrEmpty() ? "$p" + index : name;
            Type = type;
        }

        // note. this one is necessary to preserve all metadata that we might need later on
        // e.g. for the purpose of determining whether this particular argument is an "out" one
        public ParamInfo(Sig sig, int index, ParameterInfo metadata)
            : this(sig, index, metadata.AssertNotNull().Name, metadata.AssertNotNull().ParameterType)
        {
            Metadata = metadata;
        }

        public override String ToString()
        {
            var self = String.Format("{0} {1}", Type.GetCSharpRef(ToCSharpOptions.Informative), Name);
            return String.Format("{0} | {1}", self, Sig);
        }
    }
}