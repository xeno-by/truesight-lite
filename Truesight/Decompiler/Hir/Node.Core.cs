using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Truesight.Decompiler.Hir.Prettyprint;
using XenoGears.Traits.Cloneable;
using XenoGears.Traits.Dumpable;
using XenoGears.Functional;

namespace Truesight.Decompiler.Hir
{
    [DumpFormat(NullObjectFormat = "null", DefaultExtension = "hir")]
    [DebuggerNonUserCode]
    public abstract partial class Node : IDumpableAsText, ICloneable2
    {
        public NodeType NodeType { get; private set; }

        protected Node(NodeType nodeType, IEnumerable<Node> children)
            : base(children)
        {
            NodeType = nodeType;
            SetChangeTrackingHooks();
        }

        protected Node(NodeType nodeType, params Node[] children)
            : this(nodeType, (IEnumerable<Node>)children)
        {
        }

        void IDumpableAsText.DumpAsText(TextWriter writer)
        {
            var cached = Domain == null ? null : Domain.DumpAsTextCache.GetOrDefault(this);
            if (cached != null) writer.Write(cached);
            else
            {
                // caching of this very invocation is performed within prettyprinter
                new CSharpPrettyprinter(writer).Traverse(this);
            }
        }
    }
}