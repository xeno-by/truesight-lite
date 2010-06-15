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
    // note. similarly to Return this node has somewhat ambiguous semantics
    // when its Exception property is set to null, we've got two options:
    // 1) it hasn't been set or has been reset for a while, 2) it has rethrow semantics
    //
    // unlike in the case with Return, I could've implement this unambigously
    // by making two separate classes: Throw and Rethrow
    // but I decided to keep it simple and opted out of creating two different classes

    [DebuggerDisplay("{ToDebugString_WithParentInfo(), nq}{\"\", nq}")]
    [DebuggerTypeProxy(typeof(ThrowDebugView))]
    [DebuggerNonUserCode]
    public class Throw : Node
    {
        public Expression Exception
        {
            get { return (Expression)Children[0]; } 
            set { SetProperty("Exception", v => Children[0] = v, Children[0], value); }
        }

        public Throw()
            : this(null)
        {
        }

        public Throw(Expression value)
            : base(NodeType.Throw, value)
        {
        }

        public new Throw DeepClone()
        {
            return ((ICloneable2)this).DeepClone<Throw>();
        }

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceThrow(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseThrow(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                var exception = transformer.Transform(Exception).AssertCast<Expression>();
                return new Throw(exception);
            }
            else
            {
                return transformer.TransformThrow(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class ThrowDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Throw _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public ThrowDebugView(Throw node) : this(node, null) { }
            public ThrowDebugView(Throw node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public ThrowDebugView(Throw node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithParentInfo(); }

            [DebuggerDisplay("{aParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object aParent { get { return _node.Parent.CreateDebugProxy(this); } }

            [DebuggerDisplay("{bException, nq}{\"\", nq}", Name = "Exception")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object bException { get { return _node.Exception.CreateDebugProxy(this); } }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class ThrowDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Throw _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public ThrowDebugView_NoParent(Throw node) : this(node, null) { }
            public ThrowDebugView_NoParent(Throw node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public ThrowDebugView_NoParent(Throw node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerDisplay("{bException, nq}{\"\", nq}", Name = "Exception")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object bException { get { return _node.Exception.CreateDebugProxy(this); } }
        }
    }
}