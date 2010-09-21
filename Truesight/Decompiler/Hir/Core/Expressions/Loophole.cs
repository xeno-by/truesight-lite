using System;
using System.Diagnostics;
using Truesight.Decompiler.Hir.Traversal;
using Truesight.Decompiler.Hir.Traversal.Reducers;
using Truesight.Decompiler.Hir.Traversal.Transformers;
using Truesight.Decompiler.Hir.Traversal.Traversers;
using XenoGears;
using XenoGears.Functional;
using XenoGears.Traits.Cloneable;

namespace Truesight.Decompiler.Hir.Core.Expressions
{
    [DebuggerDisplay("{ToDebugString_WithParentInfo(), nq}{\"\", nq}")]
    [DebuggerTypeProxy(typeof(LoopholeDebugView))]
    [DebuggerNonUserCode]
    public class Loophole : Expression
    {
        private Guid _id = Guid.NewGuid();
        public Guid Id { get { return _id; } }

        private Object _tag;
        public Object Tag
        {
            get { return _tag; }
            set { SetProperty("Tag", v => _tag = v, _tag, value); }
        }

        public Loophole()
            : this(null)
        {
        }

        public Loophole(Object tag)
            : base(NodeType.Loophole)
        {
            Tag = tag;
        }

        protected override bool EigenEquiv(Node node)
        {
            if (!base.EigenEquiv(node)) return false;
            var other = node as Loophole;
            return Equals(this.Id, other.Id);
        }

        protected override int EigenHashCode()
        {
            return base.EigenHashCode() ^ Id.GetHashCode();
        }

        public new Loophole DeepClone()
        {
            return ((ICloneable2)this).DeepClone<Loophole>();
        }

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceLoophole(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseLoophole(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                var visited = new Loophole().HasProto(this);
                visited._id = _id;
                visited.Tag = Tag;
                return this;
            }
            else
            {
                return transformer.TransformLoophole(this).HasProto(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class LoopholeDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Loophole _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public LoopholeDebugView(Loophole node) : this(node, null) { }
            public LoopholeDebugView(Loophole node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public LoopholeDebugView(Loophole node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithParentInfo(); }

            [DebuggerDisplay("{aParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object aParent { get { return (_node.Stmt().Fluent(e => e == _node ? null : e) ?? _node.Parent).CreateDebugProxy(this); } }

            [DebuggerDisplay("{bId, nq}{\"\", nq}", Name = "Id")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Guid bId { get { return _node._id; } }

            [DebuggerDisplay("{cTag, nq}{\"\", nq}", Name = "Tag")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object cTag { get { return _node.Tag; } }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class LoopholeDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Loophole _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public LoopholeDebugView_NoParent(Loophole node) : this(node, null) { }
            public LoopholeDebugView_NoParent(Loophole node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public LoopholeDebugView_NoParent(Loophole node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerDisplay("{bId, nq}{\"\", nq}", Name = "Id")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Guid bId { get { return _node._id; } }

            [DebuggerDisplay("{cTag, nq}{\"\", nq}", Name = "Tag")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object cTag { get { return _node.Tag; } }
        }
    }
}