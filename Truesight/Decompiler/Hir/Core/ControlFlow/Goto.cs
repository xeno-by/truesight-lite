using System;
using System.Diagnostics;
using Truesight.Decompiler.Hir.Traversal.Reducers;
using Truesight.Decompiler.Hir.Traversal.Transformers;
using Truesight.Decompiler.Hir.Traversal.Traversers;
using XenoGears.Traits.Cloneable;
using Truesight.Decompiler.Hir.Traversal;

namespace Truesight.Decompiler.Hir.Core.ControlFlow
{
    [DebuggerDisplay("{ToDebugString_WithParentInfo(), nq}{\"\", nq}")]
    [DebuggerTypeProxy(typeof(GotoDebugView))]
    [DebuggerNonUserCode]
    public class Goto : Node
    {
        private Guid _labelId; 
        public Guid LabelId
        {
            get { return _labelId; } 
            set { SetProperty("LabelId", v => _labelId = v, _labelId, value); }
        }

        public Goto()
            : this(Guid.Empty)
        {
        }

        public Goto(Guid labelId)
            : base(NodeType.Goto)
        {
            LabelId = labelId;
        }

        public Goto(Label label)
            : this(label.Id)
        {
        }

        protected override bool EigenEquiv(Node node)
        {
            if (!base.EigenEquiv(node)) return false;
            var other = node as Goto;
            return Equals(this.LabelId, other.LabelId);
        }

        protected override int EigenHashCode()
        {
            return base.EigenHashCode() ^ LabelId.GetHashCode();
        }

        public new Goto DeepClone()
        {
            return ((ICloneable2)this).DeepClone<Goto>();
        }

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceGoto(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseGoto(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                var visited = new Goto();
                visited.LabelId = LabelId;
                return visited.HasProto(this);
            }
            else
            {
                return transformer.TransformGoto(this).HasProto(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class GotoDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Goto _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public GotoDebugView(Goto node) : this(node, null) { }
            public GotoDebugView(Goto node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) { }
            public GotoDebugView(Goto node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithParentInfo(); }

            [DebuggerDisplay("{aParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object aParent { get { return _node.Parent.CreateDebugProxy(this); } }

            [DebuggerDisplay("{bLabel, nq}{\"\", nq}", Name = "Label")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object bLabel { get { return _node.ResolveLabel().CreateDebugProxy(null); } }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class GotoDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Goto _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public GotoDebugView_NoParent(Goto node) : this(node, null) { }
            public GotoDebugView_NoParent(Goto node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) { }
            public GotoDebugView_NoParent(Goto node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerDisplay("{bLabel, nq}{\"\", nq}", Name = "Label")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object bLabel { get { return _node.ResolveLabel().CreateDebugProxy(null); } }
        }
    }
}