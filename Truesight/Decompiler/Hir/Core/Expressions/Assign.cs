using System;
using System.Diagnostics;
using Truesight.Decompiler.Hir.Traversal;
using Truesight.Decompiler.Hir.Traversal.Reducers;
using Truesight.Decompiler.Hir.Traversal.Transformers;
using Truesight.Decompiler.Hir.Traversal.Traversers;
using XenoGears;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Traits.Cloneable;

namespace Truesight.Decompiler.Hir.Core.Expressions
{
    [DebuggerDisplay("{ToDebugString_WithParentInfo(), nq}{\"\", nq}")]
    [DebuggerTypeProxy(typeof(AssignDebugView))]
    [DebuggerNonUserCode]
    public class Assign : Expression
    {
        public Expression Lhs { get { return (Expression)Children[0]; } set { SetProperty("Lhs", v => Children[0] = v, Children[0], value); } }
        public Expression Rhs { get { return (Expression)Children[1]; } set { SetProperty("Rhs", v => Children[1] = v, Children[1], value); } }

        public Assign()
            : this(null, null)
        {
        }

        public Assign(Expression lhs, Expression rhs)
            : base(NodeType.Assign, lhs, rhs)
        {
        }

        public new Assign DeepClone()
        {
            return ((ICloneable2)this).DeepClone<Assign>();
        }

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceAssign(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseAssign(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                var rhs = transformer.Transform(Rhs).AssertCast<Expression>();
                var lhs = transformer.Transform(Lhs).AssertCast<Expression>();
                return new Assign(lhs, rhs);
            }
            else
            {
                return transformer.TransformAssign(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class AssignDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Assign _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public AssignDebugView(Assign node) : this(node, null) { }
            public AssignDebugView(Assign node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public AssignDebugView(Assign node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithParentInfo(); }

            [DebuggerDisplay("{aParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object aParent { get { return (_node.Stmt().Fluent(e => e == _node ? null : e) ?? _node.Parent).CreateDebugProxy(this); } }

            [DebuggerDisplay("{bLhs, nq}{\"\", nq}", Name = "Lhs")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object bLhs { get { return _node.Lhs.CreateDebugProxy(this); } }

            [DebuggerDisplay("{cRhs, nq}{\"\", nq}", Name = "Rhs")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object cRhs { get { return _node.Rhs.CreateDebugProxy(this); } }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class AssignDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Assign _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public AssignDebugView_NoParent(Assign node) : this(node, null) { }
            public AssignDebugView_NoParent(Assign node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public AssignDebugView_NoParent(Assign node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerDisplay("{bLhs, nq}{\"\", nq}", Name = "Lhs")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object bLhs { get { return _node.Lhs.CreateDebugProxy(this); } }

            [DebuggerDisplay("{cRhs, nq}{\"\", nq}", Name = "Rhs")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object cRhs { get { return _node.Rhs.CreateDebugProxy(this); } }
        }
    }
}