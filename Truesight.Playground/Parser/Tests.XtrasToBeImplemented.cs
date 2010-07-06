using NUnit.Framework;
using Truesight.Parser;
using Truesight.Playground.Parser.ILRewriter;
using XenoGears.Functional;
using XenoGears.Reflection.Emit;
using XenoGears.Reflection.Emit2;
using XenoGears.Reflection.Generics;
using XenoGears.Reflection.Shortcuts;

namespace Truesight.Playground.Parser
{
    [TestFixture, Category("Xtras To Be Implemented")]
    public class XtrasToBeImplemented : Tests
    {
        [Test]
        public void ParseSmallSnippetWithoutSwitch_MethodBuilder_InProgress()
        {
            var snippet = typeof(Snippets).GetMethod("SmallSnippetWithoutSwitch", BF.All);
            var cu = Codegen.Units["Truesight.Playground.Reflection.Parse.Tests"];
            var t = cu.Module.DefineType("ParseSmallSnippetWithoutSwitch_MethodBuilder_InProgress");
            var m = t.DefineMethod("SmallSnippet", MA.Public, snippet.Ret(), snippet.Params());
            // todo. also preserve names, i.e. emit debug info for symbols
            snippet.ParseBody(true).Locals.ForEach(lv => m.il().DeclareLocal(lv.Type));
#pragma warning disable 618,612
            snippet.RewriteInto(m, (_, ctrl) => ctrl.Clone());
#pragma warning restore 618,612

            var parsed = m.ParseBody(true);
            VerifyResult(parsed);
        }

        [Test]
        public void ParseSnippetWithGuards()
        {
            var snippet = typeof(Snippets).GetMethod("SnippetWithGuards", BF.All);
            var parsed = snippet.ParseBody(true);
            // auto-generate test for tostring (check: args, locals, body, guards!)
        }
    }
}
