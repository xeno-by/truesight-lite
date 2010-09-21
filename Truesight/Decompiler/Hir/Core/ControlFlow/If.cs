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
    [DebuggerTypeProxy(typeof(IfDebugView))]
    [DebuggerNonUserCode]
    public class If : Node
    {
        public Expression Test { get { return (Expression)Children[0]; }  set { SetProperty("Test", v => Children[0] = v, Children[0], value); } }
        public Block IfTrue { get { return (Block)Children[1]; } set { SetProperty("IfTrue", v => Children[1] = v, Children[1], value ?? new Block()); } }
        public Block IfFalse { get { return (Block)Children[2]; } set { SetProperty("IfFalse", v => Children[2] = v, Children[2], value ?? new Block()); } }

        public If()
            : this(null, null, null)
        {
        }

        public If(Expression test)
            : this(test, null, null)
        {
        }

        public If(Expression test, Block ifTrue)
            : this(test, ifTrue, null)
        {
        }

        public If(Expression test, Block ifTrue, Block ifFalse)
            : base(NodeType.If, test, ifTrue ?? new Block(), ifFalse ?? new Block())
        {
        }

        public new If DeepClone()
        {
            return ((ICloneable2)this).DeepClone<If>();
        }

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceIf(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseIf(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                var test = transformer.Transform(Test).AssertCast<Expression>();
                var iftrue = transformer.Transform(IfTrue).AssertCast<Block>();
                var iffalse = transformer.Transform(IfFalse).AssertCast<Block>();
                return new If(test, iftrue, iffalse).HasProto(this);
            }
            else
            {
                return transformer.TransformIf(this).HasProto(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class IfDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly If _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public IfDebugView(If node) : this(node, null) { }
            public IfDebugView(If node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public IfDebugView(If node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithParentInfo(); }

            [DebuggerDisplay("{aParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object aParent { get { return _node.Parent.CreateDebugProxy(this); } }

            [DebuggerDisplay("{bTest, nq}{\"\", nq}", Name = "Test")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object bTest { get { return _node.Test.CreateDebugProxy(this); } }

            [DebuggerDisplay("{cIfTrue, nq}{\"\", nq}", Name = "IfTrue")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object cIfTrue { get { return _node.IfTrue.CreateDebugProxy(this); } }

            [DebuggerDisplay("{dIfFalse, nq}{\"\", nq}", Name = "IfFalse")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object dIfFalse { get { return _node.IfFalse.CreateDebugProxy(this); } }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class IfDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly If _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public IfDebugView_NoParent(If node) : this(node, null) { }
            public IfDebugView_NoParent(If node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public IfDebugView_NoParent(If node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerDisplay("{bTest, nq}{\"\", nq}", Name = "Test")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object bTest { get { return _node.Test.CreateDebugProxy(this); } }

            [DebuggerDisplay("{cIfTrue, nq}{\"\", nq}", Name = "IfTrue")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object cIfTrue { get { return _node.IfTrue.CreateDebugProxy(this); } }

            [DebuggerDisplay("{dIfFalse, nq}{\"\", nq}", Name = "IfFalse")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object dIfFalse { get { return _node.IfFalse.CreateDebugProxy(this); } }
        }
    }
}