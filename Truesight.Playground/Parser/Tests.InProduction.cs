using NUnit.Framework;
using Truesight.Parser;
using Truesight.Playground.Parser.ILRewriter;
using XenoGears.Reflection.Emit2;
using XenoGears.Reflection.Shortcuts;
using XenoGears.Functional;
using XenoGears.Reflection.Emit;
using XenoGears.Reflection.Generics;

namespace Truesight.Playground.Parser
{
    [TestFixture, Category("In Production")]
    public class InProduction : Tests
    {
        [Test]
        public void ParseSmallSnippet_NormalCase()
        {
            var snippet = typeof(Snippets).GetMethod("SmallSnippetWithoutSwitch", BF.All);
            var parsed = snippet.ParseBody(true);
            TestParseResult(parsed);
        }

        [Test]
        public void ParseSmallSnippetWithoutSwitch_MethodBuilder_Baked()
        {
            var snippet = typeof(Snippets).GetMethod("SmallSnippetWithoutSwitch", BF.All);
            var cu = Codegen.Units["Truesight.Playground.Reflection.Parse.Tests"];
            var t = cu.Module.DefineType("ParseSmallSnippetWithoutSwitch_MethodBuilder_Baked");
            var m = t.DefineMethod("SmallSnippet", MA.Public, snippet.Ret(), snippet.Params());
            // todo. also preserve names, i.e. emit debug info for symbols
            snippet.ParseBody(true).Locals.ForEach(lv => m.il().DeclareLocal(lv.Type));
#pragma warning disable 618,612
            snippet.RewriteInto(m, (_, ctrl) => ctrl.Clone());
#pragma warning restore 618,612

            var t_created = t.CreateType();
            var m_created = t_created.GetMethod("SmallSnippet");
            var parsed = m_created.ParseBody(true);
            TestParseResult(parsed);
        }

        [Test]
        public void ParseSmallSnippet_AsByteSequence_WithModule()
        {
            var snippet = typeof(Snippets).GetMethod("SmallSnippetWithoutSwitch", BF.All);
            var rawIL = snippet.GetMethodBody().GetILAsByteArray();
            var parsed = rawIL.ParseRawIL(snippet.Module);
            TestParseResult(parsed);
        }

        [Test]
        public void ParseSmallSnippet_AsByteSequence_WithoutModule()
        {
            var snippet = typeof(Snippets).GetMethod("SmallSnippetWithoutSwitch", BF.All);
            var rawIL = snippet.GetMethodBody().GetILAsByteArray();
            var parsed = rawIL.ParseRawIL();
            TestParseResult(parsed);
        }
    }
}