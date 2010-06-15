using System;
using System.Diagnostics;
using System.Linq;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Core.Symbols;
using Truesight.Decompiler.Hir.Traversal.Reducers;
using Truesight.Decompiler.Hir.Traversal.Transformers;
using Truesight.Decompiler.Hir.Traversal.Traversers;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Traits.Cloneable;

namespace Truesight.Decompiler.Hir.Core.ControlFlow
{
    [DebuggerDisplay("{ToDebugString_WithParentInfo(), nq}{\"\", nq}")]
    [DebuggerTypeProxy(typeof(IterDebugView))]
    [DebuggerNonUserCode]
    public partial class Iter : Node
    {
        // todo. preserve types integrity:
        // 1) Seq changes type => update type of the Element local
        // 2) Element local can never change type by itself (how to verify this?!)
        public Expression Seq { get { return (Expression)Children[0]; } set { SetProperty("Seq", v => Children[0] = v, Children[0], value); } }
        public Local Element { get { return Locals.AssertSingle(); } set { SetProperty("Element", v => Locals[0] = v, Locals[0], value); } }
        public Block Body { get { return (Block)Children[1]; } set { SetProperty("Body", v => Children[1] = v, Children[1], value ?? new Block()); } }

        public Iter()
            : this(null, new Block())
        {
        }

        public Iter(Expression init)
            : this(init, new Block())
        {
        }

        public Iter(Expression init, Block body)
            : base(NodeType.Iter, init, body ?? new Block())
        {
            Locals.Add(new Local("$el", null));
        }

        public new Iter DeepClone()
        {
            return ((ICloneable2)this).DeepClone<Iter>();
        }

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceIter(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseIter(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                var seq = transformer.Transform(Seq).AssertCast<Expression>();
                var body = transformer.Transform(Body).AssertCast<Block>();

                var visited = new Iter(seq, body);
                visited.Locals.SetElements(Locals.Select(loc => loc.DeepClone()));
                return visited;
            }
            else
            {
                return transformer.TransformIter(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class IterDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Iter _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public IterDebugView(Iter node) : this(node, null) { }
            public IterDebugView(Iter node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public IterDebugView(Iter node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithParentInfo(); }

            [DebuggerDisplay("{aParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object aParent { get { return _node.Parent.CreateDebugProxy(this); } }

            [DebuggerDisplay("{bSeq, nq}{\"\", nq}", Name = "Seq")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object bSeq { get { return _node.Seq.CreateDebugProxy(this); } }

            [DebuggerDisplay("{cElement, nq}{\"\", nq}", Name = "Element")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Local cElement { get { return _node.Element; } }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Object zBody { get { return _node.Body.CreateDebugProxy(this); } }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class IterDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Iter _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public IterDebugView_NoParent(Iter node) : this(node, null) { }
            public IterDebugView_NoParent(Iter node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public IterDebugView_NoParent(Iter node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerDisplay("{bSeq, nq}{\"\", nq}", Name = "Seq")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object bSeq { get { return _node.Seq.CreateDebugProxy(this); } }

            [DebuggerDisplay("{cElement, nq}{\"\", nq}", Name = "Element")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Local cElement { get { return _node.Element; } }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Object zBody { get { return _node.Body.CreateDebugProxy(this); } }
        }
    }
}