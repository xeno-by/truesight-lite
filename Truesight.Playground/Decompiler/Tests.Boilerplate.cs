using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Truesight.Decompiler;
using Truesight.Decompiler.Domains;
using Truesight.Decompiler.Hir.Core.Functional;
using XenoGears.Playground.Framework;
using XenoGears.Reflection.Generics;
using XenoGears.Strings;
using XenoGears.Traits.Dumpable;
using XenoGears.Functional;
using XenoGears.Assertions;

namespace Truesight.Playground.Decompiler
{
    public abstract class Tests : BaseTests
    {
        protected void TestMethodDecompilation(MethodBase mb)
        {
            Flash["Method Being Decompiled"] = mb;
            Func<Lambda> decompile = () => mb.Decompile(Semantics.CSharp35_WithDebugInfo);
            1.UpTo(10).Select(_ => decompile().Body).Ping();
            var lam = decompile();

            Func<String> dumpAsText = () => String.Format("{1}{0}{2}",
                Environment.NewLine, lam.Sig.DumpAsText(),  lam.Body.DumpAsText());
            1.UpTo(10).ForEach(_ => dumpAsText());
            var s_actual = dumpAsText();
            VerifyResult(s_actual);
        }

        protected override ReadOnlyCollection<String> ReferenceWannabes()
        {
            var mb = Flash["Method Being Decompiled"].AssertCast<MethodBase>().AssertNotNull();
            var declt = mb.DeclaringType.AssertNotNull();

            var fnameWannabes = new List<String>();
            var cs_opt = ToCSharpOptions.Informative;
            cs_opt.EmitCtorNameAsClassName = true;
            var s_name = mb.IsConstructor ? mb.DeclaringType.GetCSharpRef(cs_opt).Replace("<", "[").Replace(">", "]").Replace("&", "!").Replace("*", "!") : mb.Name;
            var s_sig = mb.Params().Select(p => p.GetCSharpRef(cs_opt).Replace("<", "[").Replace(">", "]").Replace("&", "!").Replace("*", "!")).StringJoin("_");
            var s_declt = declt.GetCSharpRef(cs_opt).Replace("<", "[").Replace(">", "]").Replace("&", "!").Replace("*", "!");
            fnameWannabes.Add(s_name);
            fnameWannabes.Add(s_name + "_" + s_sig);
            fnameWannabes.Add(s_declt + "_" + s_name);
            fnameWannabes.Add(s_declt + "_" + s_name + "_" + s_sig);

            return fnameWannabes.ToReadOnly();
        }
    }
}
