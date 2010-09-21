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
    [DebuggerTypeProxy(typeof(ConditionalOperatorDebugView))]
    [DebuggerNonUserCode]
    public class Conditional : Expression
    {
        public Expression Test { get { return (Expression)Children[0]; } set { SetProperty("Test", v => Children[0] = v, Children[0], value); } }
        public Expression IfTrue { get { return (Expression)Children[1]; } set { SetProperty("IfTrue", v => Children[1] = v, Children[1], value); } }
        public Expression IfFalse { get { return (Expression)Children[2]; } set { SetProperty("IfFalse", v => Children[2] = v, Children[2], value); } }

        public Conditional()
            : this(null, null, null)
        {
        }

        public Conditional(Expression test)
            : this(test, null, null)
        {
        }

        public Conditional(Expression test, Expression ifTrue, Expression ifFalse)
            : base(NodeType.Conditional, test, ifTrue, ifFalse)
        {
        }

        public new Conditional DeepClone()
        {
            return ((ICloneable2)this).DeepClone<Conditional>();
        }

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceConditional(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseConditional(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                var test = transformer.Transform(Test).AssertCast<Expression>();
                var iftrue = transformer.Transform(IfTrue).AssertCast<Expression>();
                var iffalse = transformer.Transform(IfFalse).AssertCast<Expression>();
                return new Conditional(test, iftrue, iffalse).HasProto(this);
            }
            else
            {
                return transformer.TransformConditional(this).HasProto(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class ConditionalOperatorDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Conditional _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public ConditionalOperatorDebugView(Conditional node) : this(node, null) { }
            public ConditionalOperatorDebugView(Conditional node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public ConditionalOperatorDebugView(Conditional node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithParentInfo(); }

            [DebuggerDisplay("{aParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object aParent { get { return (_node.Stmt().Fluent(e => e == _node ? null : e) ?? _node.Parent).CreateDebugProxy(this); } }

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
        internal class ConditionalOperatorDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Conditional _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public ConditionalOperatorDebugView_NoParent(Conditional node) : this(node, null) { }
            public ConditionalOperatorDebugView_NoParent(Conditional node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public ConditionalOperatorDebugView_NoParent(Conditional node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
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