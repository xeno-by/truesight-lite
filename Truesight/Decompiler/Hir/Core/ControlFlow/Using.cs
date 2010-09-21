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
    [DebuggerTypeProxy(typeof(UsingDebugView))]
    [DebuggerNonUserCode]
    public partial class Using : Node
    {
        // todo. preserve types integrity:
        // 1) Init changes type => update type of the Resource local
        // 2) Resource local can never change type by itself (how to verify this?!)
        public Expression Init { get { return (Expression)Children[0]; } set { SetProperty("Init", v => Children[0] = v, Children[0], value); } }
        public Local Resource { get { return Locals.AssertSingle(); } set { SetProperty("Resource", v => Locals[0] = v, Locals[0], value); } }
        public Block Body { get { return (Block)Children[1]; } set { SetProperty("Body", v => Children[1] = v, Children[1], value ?? new Block()); } }

        public Using()
            : this(null, null)
        {
        }

        public Using(Expression init)
            : this(init, new Block())
        {
        }

        public Using(Expression init, Block body)
            : base(NodeType.Using, init, body ?? new Block())
        {
            Locals.Add(new Local("$res", null));
        }

        public new Using DeepClone()
        {
            return ((ICloneable2)this).DeepClone<Using>();
        }

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceUsing(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseUsing(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                var init = transformer.Transform(Init).AssertCast<Expression>();
                var body = transformer.Transform(Body).AssertCast<Block>();

                var visited = new Using(init, body);
                visited.Locals.SetElements(Locals.Select(loc => loc.DeepClone()));
                return visited.HasProto(this);
            }
            else
            {
                return transformer.TransformUsing(this).HasProto(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class UsingDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Using _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public UsingDebugView(Using node) : this(node, null) { }
            public UsingDebugView(Using node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public UsingDebugView(Using node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithParentInfo(); }

            [DebuggerDisplay("{aParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object aParent { get { return _node.Parent.CreateDebugProxy(this); } }

            [DebuggerDisplay("{bInit, nq}{\"\", nq}", Name = "Init")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object bInit { get { return _node.Init.CreateDebugProxy(this); } }

            [DebuggerDisplay("{cResource, nq}{\"\", nq}", Name = "Resource")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Local cResource { get { return _node.Resource; } }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Object zBody { get { return _node.Body.CreateDebugProxy(this); } }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class UsingDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Using _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public UsingDebugView_NoParent(Using node) : this(node, null) { }
            public UsingDebugView_NoParent(Using node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public UsingDebugView_NoParent(Using node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerDisplay("{bInit, nq}{\"\", nq}", Name = "Init")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object bInit { get { return _node.Init.CreateDebugProxy(this); } }

            [DebuggerDisplay("{cResource, nq}{\"\", nq}", Name = "Resource")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Local cResource { get { return _node.Resource; } }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Object zBody { get { return _node.Body.CreateDebugProxy(this); } }
        }
    }
}