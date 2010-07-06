using System;
using System.Linq;
using System.Reflection;
using Truesight.Decompiler;
using Truesight.Decompiler.Domains;
using Truesight.Decompiler.Hir.Core.Functional;
using XenoGears.Playground.Framework;
using XenoGears.Traits.Dumpable;
using XenoGears.Functional;

namespace Truesight.Playground.Decompiler
{
    public abstract class Tests : BaseTests
    {
        protected void TestMethodDecompilation(MethodBase mb)
        {
            Func<Lambda> decompile = () => mb.Decompile(Semantics.CSharp35_WithDebugInfo);
            1.UpTo(10).Select(_ => decompile().Body).Ping();
            var lam = decompile();

            Func<String> dumpAsText = () => String.Format("{1}{0}{2}",
                Environment.NewLine, lam.Sig.DumpAsText(),  lam.Body.DumpAsText());
            1.UpTo(10).ForEach(_ => dumpAsText());
            var s_actual = dumpAsText();
            VerifyResult(s_actual, mb);
        }
    }
}
