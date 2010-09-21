using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Truesight.Decompiler.Hir.Traversal.Reducers;
using Truesight.Decompiler.Hir.Traversal.Transformers;
using Truesight.Decompiler.Hir.Traversal.Traversers;
using XenoGears.Functional;
using XenoGears.Collections.Virtual;
using XenoGears.Traits.Cloneable;
using XenoGears.Assertions;
using XenoGears.Strings;

namespace Truesight.Decompiler.Hir.Core.ControlFlow
{
    [DebuggerDisplay("{ToDebugString_WithParentInfo(), nq}{\"\", nq}")]
    [DebuggerTypeProxy(typeof(TryDebugView))]
    [DebuggerNonUserCode]
    public class Try : Node
    {
        public Block Body { get { return (Block)Children[0]; } set { SetProperty("Body", v => Children[0] = v, Children[0], value); } }
        // todo. make this observable and also report changes using FirePropertyChanging/Changed providing ListChangeEventArgs as a tag to the event
        // note. to make this observable, please, do NOT just append .Observe() to the ctor
        // since it will just create an observable ***copy*** of current Children
        // when Children will change, the copy will not and you'll spend shitloads of time debugging that
        public IList<Clause> Clauses
        {
            get
            {
                return new VirtualList<Clause>(
                    () => Children.Skip(1).Cast<Clause>(),
                    (i, e) => Children.Insert(i + 1, e),
                    (i, e) => Children[i + 1] = e,
                    i => Children.RemoveAt(i + 1));
            }
        }

        public Try(params Clause[] clauses)
            : this(null, clauses)
        {
        }

        public Try(IEnumerable<Clause> clauses)
            : this(null, clauses)
        {
        }

        public Try(Block body, params Clause[] clauses)
            : base(NodeType.Try, (body ?? new Block()).Concat(clauses).Cast<Node>())
        {
        }

        public Try(Block body, IEnumerable<Clause> clauses)
            : this(body, clauses.ToArray())
        {
        }

        public new Try DeepClone()
        {
            return ((ICloneable2)this).DeepClone<Try>();
        }

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceTry(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseTry(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                var body = transformer.Transform(Body).AssertCast<Block>();
                var clauses = Clauses.Select(c => transformer.Transform(c)).AssertCast<Clause>();
                return new Try(body, clauses).HasProto(this);
            }
            else
            {
                return transformer.TransformTry(this).HasProto(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class TryDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Try _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public TryDebugView(Try node) : this(node, null) {}
            public TryDebugView(Try node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) { }
            public TryDebugView(Try node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithParentInfo(); }

            [DebuggerDisplay("{aParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object aParent { get { return _node.Parent.CreateDebugProxy(this); } }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Object zChildren
            {
                get
                {
                    var body = _node.Body.Children.CreateDebugProxy(this, (_, i) => "[" + i + "]");
                    var clauses = _node.Clauses.CreateDebugProxy(this, (c, i) => c == null ? "null" : c.GetType().Name.ToLower());
                    return Seq.Concat(body, clauses).ToArray();
                }
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class TryDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Try _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public TryDebugView_NoParent(Try node) : this(node, null) { }
            public TryDebugView_NoParent(Try node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public TryDebugView_NoParent(Try node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Object zChildren
            {
                get
                {
                    var body = _node.Body.Children.CreateDebugProxy(this, (_, i) => "[" + i + "]");
                    var clauses = _node.Clauses.CreateDebugProxy(this, (c, i) => c == null ? "null" : c.GetType().Name.ToLower());
                    return Seq.Concat(body, clauses).ToArray();
                }
            }
        }
    }
}