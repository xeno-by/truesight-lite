using System;
using System.Diagnostics;
using XenoGears.Exceptions;

namespace Truesight.Decompiler.Hir.Traversal.Exceptions
{
    [DebuggerNonUserCode]
    public class HirTraversalException : BaseException
    {
        private readonly bool _isUnexpected;
        public override bool IsUnexpected { get { return _isUnexpected; } }

        [IncludeInMessage] public Node Node { get; protected set; }
        [IncludeInMessage] public NodeType? NodeType { get { return Node == null ? null : (NodeType?)Node.NodeType; } }

        protected HirTraversalException(Node node)
            : this(node, false)
        {
        }

        protected HirTraversalException(Node node, bool isUnexpected)
            : this(node, null, isUnexpected)
        {
        }

        public HirTraversalException(Node node, Exception innerException)
            : this(node, innerException, false)
        {
        }

        public HirTraversalException(Node node, Exception innerException, bool isUnexpected)
            : base(innerException)
        {
            _isUnexpected = isUnexpected;
            Node = node;
        }
    }
}