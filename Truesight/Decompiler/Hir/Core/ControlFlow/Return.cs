using System;
using System.Diagnostics;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Traversal.Reducers;
using Truesight.Decompiler.Hir.Traversal.Transformers;
using Truesight.Decompiler.Hir.Traversal.Traversers;
using XenoGears.Assertions;
using XenoGears.Traits.Cloneable;

namespace Truesight.Decompiler.Hir.Core.ControlFlow
{
    [DebuggerDisplay("{ToDebugString_WithParentInfo(), nq}{\"\", nq}")]
    [DebuggerTypeProxy(typeof(ReturnDebugView))]
    [DebuggerNonUserCode]
    public class Return : Node
    {
        public Expression Value
        {
            get { return (Expression)Children[0]; } 
            set { SetProperty("Value", v => Children[0] = v, Children[0], value); }
        }

        public Return()
            : this(null)
        {
        }

        public Return(Expression value)
            : base(NodeType.Return, value)
        {
        }

        public new Return DeepClone()
        {
            return ((ICloneable2)this).DeepClone<Return>();
        }

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceReturn(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseReturn(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                var value = transformer.Transform(Value).AssertCast<Expression>();
                return new Return(value);
            }
            else
            {
                return transformer.TransformReturn(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class ReturnDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Return _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public ReturnDebugView(Return node) : this(node, null) { }
            public ReturnDebugView(Return node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public ReturnDebugView(Return node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithParentInfo(); }

            [DebuggerDisplay("{aParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object aParent { get { return _node.Parent.CreateDebugProxy(this); } }

            [DebuggerDisplay("{bValue, nq}{\"\", nq}", Name = "Value")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object bValue { get { return _node.Value.CreateDebugProxy(this); } }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class ReturnDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Return _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public ReturnDebugView_NoParent(Return node) : this(node, null) { }
            public ReturnDebugView_NoParent(Return node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public ReturnDebugView_NoParent(Return node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerDisplay("{bValue, nq}{\"\", nq}", Name = "Value")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object bValue { get { return _node.Value.CreateDebugProxy(this); } }
        }
    }
}