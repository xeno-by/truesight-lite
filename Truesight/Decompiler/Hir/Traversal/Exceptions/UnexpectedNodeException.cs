using System;
using System.Diagnostics;
using XenoGears.Exceptions;

namespace Truesight.Decompiler.Hir.Traversal.Exceptions
{
    [DebuggerNonUserCode]
    public class UnexpectedNodeException : HirTraversalException
    {
        [IncludeInMessage]
        public new Object Node { get; private set; }

        [IncludeInMessage]
        public Type Type { get; private set; }

        public UnexpectedNodeException(Object node, Type type)
            : base(node as Node, true)
        {
            Node = node;
            Type = type;
        }
    }
}