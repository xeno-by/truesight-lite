using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Truesight.Decompiler;
using Truesight.Decompiler.Hir.Prettyprint;
using Truesight.Decompiler.Hir.TypeInference;
using Truesight.Parser.Api.Emit;
using XenoGears.Functional;
using XenoGears.Reflection;
using XenoGears.Reflection.Attributes;
using XenoGears.Reflection.Generics;
using XenoGears.Strings;

namespace Truesight.Playground
{
    [TestFixture]
    public class MetaTests
    {
        [Test]
        public void EnsureMostStuffIsMarkedWithDebuggerNonUserCode()
        {
            var asm = typeof(DecompilerApi).Assembly;
            var types = asm.GetTypes().Where(t => !t.IsInterface).ToReadOnly();
            var failed_types = types
                .Where(t => !t.HasAttr<DebuggerNonUserCodeAttribute>())
                .Where(t => !t.IsCompilerGenerated())
                .Where(t => !t.Name.Contains("<>"))
                .Where(t => !t.Name.Contains("__StaticArrayInit"))
                .Where(t => !t.IsEnum)
                .Where(t => !t.IsDelegate())
                // exceptions for decompiler
                .Where(t => !t.Namespace.StartsWith("Truesight.Decompiler.Pipeline"))
                .Where(t => t != typeof(TypeInferenceTraverser))
                .Where(t => t != typeof(CSharpPrettyprinter))
                .Where(t => t != typeof(CSharpParenthesesHelper))
                // exceptions for parser
                .Where(t => t != typeof(ILTrait))
                .ToReadOnly();

            if (failed_types.IsNotEmpty())
            {
                Trace.WriteLine(String.Format("{0} types in Truesight aren't marked with [DebuggerNonUserCode]:", failed_types.Count()));
                var messages = failed_types.Select(t => t.GetCSharpRef(ToCSharpOptions.InformativeWithNamespaces));
                messages.OrderDescending().ForEach(message => Trace.WriteLine(message));
                Assert.Fail();
            }
        }
    }
}