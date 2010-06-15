using System;
using System.Diagnostics;

namespace Truesight.Decompiler.Hir.Traversal.Exceptions
{
    [DebuggerNonUserCode]
    public class UnsupportedNodeException : HirTraversalException
    {
        public UnsupportedNodeException(Node node)
            : this(node, null)
        {
        }

        public UnsupportedNodeException(Node node, Exception innerException)
            : base(node, innerException)
        {
        }
    }
}