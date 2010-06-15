using System.Diagnostics;
using System.IO;
using System.Linq;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Traversal.Traversers;
using Truesight.Decompiler.Hir.TypeInference;
using XenoGears.Functional;

namespace Truesight.Decompiler.Hir.Prettyprint
{
    public partial class CSharpPrettyprinter : AbstractHirTraverser
    {
        private readonly CSharpPrettyprintWriter _writer;
        public CSharpPrettyprinter(TextWriter writer)
        {
            _writer = new CSharpPrettyprintWriter(writer);
        }

        [DebuggerNonUserCode]
        protected override void TraverseNode(Node node)
        {
            // warm up caches before we traverse the HIR
            if (Stack.Skip(1).IsEmpty()) node.InferTypes();

            var needsParentheses = (node as Expression).NeedsParenthesesInCSharp();
            if (Stack.Skip(1).IsEmpty()) needsParentheses = false;
            if (needsParentheses) _writer.Write("(");
            base.TraverseNode(node);
            if (needsParentheses) _writer.Write(")");
        }
    }
}
