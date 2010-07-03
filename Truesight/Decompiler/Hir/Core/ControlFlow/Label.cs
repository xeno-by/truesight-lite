using System;
using System.Diagnostics;
using Truesight.Decompiler.Hir.Traversal.Reducers;
using Truesight.Decompiler.Hir.Traversal.Transformers;
using Truesight.Decompiler.Hir.Traversal.Traversers;
using XenoGears.Traits.Cloneable;

namespace Truesight.Decompiler.Hir.Core.ControlFlow
{
    [DebuggerDisplay("{ToDebugString_WithParentInfo(), nq}{\"\", nq}")]
    [DebuggerTypeProxy(typeof(LabelDebugView))]
    [DebuggerNonUserCode]
    public class Label : Node
    {
        public Guid Id { get; private set; }

        private String _name;
        public String Name
        {
            get { return _name; }
            private set { SetProperty("Name", v => _name = v, _name, value); }
        }

        public Label()
            : this(Guid.NewGuid(), null)
        {
        }

        public Label(Guid id)
            : this(id, null)
        {
            
        }

        public Label(String name)
            : this(Guid.NewGuid(), name)
        {
        }

        public Label(Guid id, String name)
            : base(NodeType.Label)
        {
            Id = id;
            Name = name ?? ("$" + Id.ToString().Substring(0, 4));
        }

        protected override bool EigenEquiv(Node node)
        {
            if (!base.EigenEquiv(node)) return false;
            var other = node as Label;
            return Equals(this.Id, other.Id);
        }

        protected override int EigenHashCode()
        {
            return base.EigenHashCode() ^ Id.GetHashCode();
        }

        public new Label DeepClone()
        {
            return ((ICloneable2)this).DeepClone<Label>();
        }

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceLabel(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseLabel(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                var visited = new Label();
                visited.Id = Id;
                return visited;
            }
            else
            {
                return transformer.TransformLabel(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class LabelDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Label _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public LabelDebugView(Label node) : this(node, null) { }
            public LabelDebugView(Label node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) { }
            public LabelDebugView(Label node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithParentInfo(); }

            [DebuggerDisplay("{aParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object aParent { get { return _node.Parent.CreateDebugProxy(this); } }

            [DebuggerDisplay("{bId, nq}{\"\", nq}", Name = "Id")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Guid bId { get { return _node.Id; } }

            [DebuggerDisplay("{cName, nq}{\"\", nq}", Name = "Name")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public String cName { get { return _node.Name; } }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class LabelDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Label _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public LabelDebugView_NoParent(Label node) : this(node, null) { }
            public LabelDebugView_NoParent(Label node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) { }
            public LabelDebugView_NoParent(Label node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerDisplay("{bId, nq}{\"\", nq}", Name = "Id")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Guid bId { get { return _node.Id; } }

            [DebuggerDisplay("{cName, nq}{\"\", nq}", Name = "Name")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public String cName { get { return _node.Name; } }
        }
    }
}