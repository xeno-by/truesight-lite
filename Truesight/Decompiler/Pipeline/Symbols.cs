using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Truesight.Decompiler.Hir.Core.Symbols;
using XenoGears.Functional;

namespace Truesight.Decompiler.Pipeline
{
    [DebuggerNonUserCode]
    internal class Symbols
    {
        private Context Ctx { get; set; }
        public Symbols(Context ctx)
        {
            Ctx = ctx;
            Params = ctx.Sig.Syms;
            Locals = ctx.Cil.Locals.Select(lv =>
            {
                var nameOfLocal = ctx.Cil.DebugInfo == null ? null :
                    ctx.Cil.DebugInfo.LocalNames.GetOrDefault(lv.Index);
                return new Local(nameOfLocal ?? ("loc" + lv.Index), lv.Type);
            }).ToReadOnly();
        }

        public ReadOnlyCollection<Param> Params { get; private set; }
        public Sym ResolveParam(int index) { return Params[index]; }

        public ReadOnlyCollection<Local> Locals { get; private set; }
        public Sym ResolveLocal(int index) { return Locals[index]; }
        public Sym IntroduceLocal(String name, Type type)
        {
            var local = new Local(name, type);
            Locals = Locals.Concat(local).ToReadOnly();
            return local;
        }
    }
}