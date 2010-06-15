using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Truesight.Decompiler.Hir.Core.Functional;
using Truesight.Decompiler.Hir.Traversal;
using Truesight.Decompiler.Hir.Traversal.Reducers;
using Truesight.Decompiler.Hir.Traversal.Transformers;
using Truesight.Decompiler.Hir.Traversal.Traversers;
using XenoGears;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Collections.Virtual;
using XenoGears.Traits.Cloneable;

namespace Truesight.Decompiler.Hir.Core.Expressions
{
    [DebuggerDisplay("{ToDebugString_WithParentInfo(), nq}{\"\", nq}")]
    [DebuggerTypeProxy(typeof(CollectionInitDebugView))]
    [DebuggerNonUserCode]
    public class CollectionInit : Expression
    {
        public Eval Ctor { get { return (Eval)Children[0]; } set { SetProperty("Ctor", v => Children[0] = v, Children[0], value); } }
        private IList<Expression> _elements;
        // todo. make this observable and also report changes using FirePropertyChanging/Changed providing ListChangeEventArgs as a tag to the event
        // note. to make this observable, please, do NOT just append .Observe() to the ctor
        // since it will just create an observable ***copy*** of current Children
        // when Children will change, the copy will not and you'll spend shitloads of time debugging that
        public IList<Expression> Elements
        {
            get
            {
                if (_elements == null)
                {
                    _elements = new VirtualList<Expression>(
                        () => this.Children.Skip(1).Cast<Expression>(),
                        (i, e) => this.Children.Insert(i + 1, e),
                        (i, e) => this.Children[i + 1] = e,
                        i => this.Children.RemoveAt(i + 1));
                }

                return _elements;
            }
        }

        public CollectionInit(Eval ctor, params Expression[] children)
            : this(ctor, (IEnumerable<Expression>)children)
        {
        }

        public CollectionInit(Eval ctor, IEnumerable<Expression> children)
            : base(NodeType.CollectionInit, ctor.Concat(children))
        {
        }

        public new CollectionInit DeepClone()
        {
            return ((ICloneable2)this).DeepClone<CollectionInit>();
        }

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceCollectionInit(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseCollectionInit(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                var elements = Elements.Select(el => transformer.Transform(el)).AssertCast<Expression>();
                var ctor = transformer.Transform(Ctor).AssertCast<Eval>();
                return new CollectionInit(ctor, elements);
            }
            else
            {
                return transformer.TransformCollectionInit(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class CollectionInitDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly CollectionInit _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public CollectionInitDebugView(CollectionInit node) : this(node, null) { }
            public CollectionInitDebugView(CollectionInit node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public CollectionInitDebugView(CollectionInit node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithParentInfo(); }

            [DebuggerDisplay("{aParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object aParent { get { return (_node.Stmt().Fluent(e => e == _node ? null : e) ?? _node.Parent).CreateDebugProxy(this); } }

            [DebuggerDisplay("{bCtor, nq}{\"\", nq}", Name = "Ctor")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object bCtor { get { return _node.Ctor.CreateDebugProxy(this); } }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Object zElements { get { return _node.Elements.CreateDebugProxy(this); } }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class CollectionInitDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly CollectionInit _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public CollectionInitDebugView_NoParent(CollectionInit node) : this(node, null) { }
            public CollectionInitDebugView_NoParent(CollectionInit node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public CollectionInitDebugView_NoParent(CollectionInit node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerDisplay("{bCtor, nq}{\"\", nq}", Name = "Ctor")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object bCtor { get { return _node.Ctor.CreateDebugProxy(this); } }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Object zElements { get { return _node.Elements.CreateDebugProxy(this); } }
        }
    }
}