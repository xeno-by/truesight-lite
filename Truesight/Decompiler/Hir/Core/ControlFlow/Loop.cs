using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Core.Symbols;
using Truesight.Decompiler.Hir.Traversal.Reducers;
using Truesight.Decompiler.Hir.Traversal.Transformers;
using Truesight.Decompiler.Hir.Traversal.Traversers;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Strings;
using XenoGears.Traits.Cloneable;

namespace Truesight.Decompiler.Hir.Core.ControlFlow
{
    [DebuggerDisplay("{ToDebugString_WithParentInfo(), nq}{\"\", nq}")]
    [DebuggerTypeProxy(typeof(LoopDebugView))]
    [DebuggerNonUserCode]
    public partial class Loop : Node
    {
        private bool _isWhileDo = true;
        // todo. make both flags notify each other about their own changes
        public bool IsWhileDo { get { return _isWhileDo; } set { SetProperty("IsWhileDo", v => _isWhileDo = value, _isWhileDo, value); } }
        public bool IsDoWhile { get { return !_isWhileDo; } set { SetProperty("IsDoWhile", v => _isWhileDo = !value, !_isWhileDo, value); } }

        public Block Init { get { return (Block)Children[0]; } set { SetProperty("Init", v => Children[0] = v, Children[0], value ?? new Block()); } }
        public Expression Test { get { return (Expression)Children[1]; } set { SetProperty("Test", v => Children[1] = v, Children[1], value); } }
        public Block Body { get { return (Block)Children[2]; } set { SetProperty("Body", v => Children[2] = v, Children[2], value ?? new Block()); } }
        public Block Iter { get { return (Block)Children[3]; } set { SetProperty("Iter", v => Children[3] = v, Children[3], value ?? new Block()); } }

        public Loop()
            : this(true)
        {
        }

        public Loop(bool isWhileDo)
            : this(null, null, isWhileDo)
        {
        }

        public Loop(Expression test)
            : this(test, true)
        {
        }

        public Loop(Expression test, bool isWhileDo)
            : this(test, null, isWhileDo)
        {
        }

        public Loop(Expression test, Block body)
            : this(test, body, true)
        {
        }

        public Loop(Expression test, Block body, bool isWhileDo)
            : base(NodeType.Loop, new Block(), test, body ?? new Block(), new Block())
        {
            IsWhileDo = isWhileDo;
        }

        protected override bool EigenEquiv(Node node)
        {
            if (!base.EigenEquiv(node)) return false;
            var other = node as Loop;
            return Equals(this.IsWhileDo, other.IsWhileDo);
        }

        protected override int EigenHashCode()
        {
            return base.EigenHashCode() ^ IsWhileDo.GetHashCode();
        }

        public new Loop DeepClone()
        {
            return ((ICloneable2)this).DeepClone<Loop>();
        }

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceLoop(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseLoop(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                Block init; Expression test; Block body, iter;
                if (IsWhileDo)
                {
                    init = transformer.Transform(Init).AssertCast<Block>();
                    test = transformer.Transform(Test).AssertCast<Expression>();
                    body = transformer.Transform(Body).AssertCast<Block>();
                    iter = transformer.Transform(Iter).AssertCast<Block>();
                }
                else
                {
                    init = transformer.Transform(Init).AssertCast<Block>();
                    body = transformer.Transform(Body).AssertCast<Block>();
                    iter = transformer.Transform(Iter).AssertCast<Block>();
                    test = transformer.Transform(Test).AssertCast<Expression>();
                }

                var visited = new Loop(test, body, IsWhileDo) { Init = init, Iter = iter };
                visited.Locals.SetElements(Locals.Select(loc => loc.DeepClone()));
                return visited;
            }
            else
            {
                return transformer.TransformLoop(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class LoopDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Loop _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public LoopDebugView(Loop node) : this(node, null) { }
            public LoopDebugView(Loop node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public LoopDebugView(Loop node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithParentInfo(); }

            [DebuggerDisplay("{aParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object aParent { get { return _node.Parent.CreateDebugProxy(this); } }

            [DebuggerDisplay("{bLocals_DebuggerDisplay, nq}{\"\", nq}", Name = "Locals")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public IList<Local> bLocals { get { return _node.Locals; } }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private String bLocals_DebuggerDisplay
            {
                get
                {
                    if (bLocals == null) return null;
                    var fmt = String.Format("Count = {0}", bLocals.Count());
                    if (bLocals.Count() > 0) fmt += (" (" + bLocals.Select(l => String.Format("{0} {1}",
                        l.Type == null ? "?" : l.Type.GetCSharpRef(ToCSharpOptions.Informative), l.Name)).StringJoin(", ") + ")");
                    return fmt;
                }
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Object zBody
            {
                get
                {
                    var init = _node.Init.Children.CreateDebugProxy(this, (_, i) => "init");
                    var body = _node.Body.Children.CreateDebugProxy(this, (_, i) => "[" + i + "]");
                    var iter = _node.Iter.Children.CreateDebugProxy(this, (_, i) => "iter");
                    return Seq.Concat(init, body, iter).ToArray();
                }
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class LoopDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Loop _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public LoopDebugView_NoParent(Loop node) : this(node, null) { }
            public LoopDebugView_NoParent(Loop node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public LoopDebugView_NoParent(Loop node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerDisplay("{bLocals_DebuggerDisplay, nq}{\"\", nq}", Name = "Locals")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public IList<Local> bLocals { get { return _node.Locals; } }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private String bLocals_DebuggerDisplay
            {
                get
                {
                    if (bLocals == null) return null;
                    var fmt = String.Format("Count = {0}", bLocals.Count());
                    if (bLocals.Count() > 0) fmt += (" (" + bLocals.Select(l => String.Format("{0} {1}",
                        l.Type == null ? "?" : l.Type.GetCSharpRef(ToCSharpOptions.Informative), l.Name)).StringJoin(", ") + ")");
                    return fmt;
                }
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Object zBody
            {
                get
                {
                    var init = _node.Init.Children.CreateDebugProxy(this, (_, i) => "init");
                    var body = _node.Body.Children.CreateDebugProxy(this, (_, i) => "[" + i + "]");
                    var iter = _node.Iter.Children.CreateDebugProxy(this, (_, i) => "iter");
                    return Seq.Concat(init, body, iter).ToArray();
                }
            }
        }
    }
}