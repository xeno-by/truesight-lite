using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Truesight.Decompiler.Hir.Core.Functional;
using Truesight.Decompiler.Hir.Traversal;
using Truesight.Decompiler.Hir.Traversal.Reducers;
using Truesight.Decompiler.Hir.Traversal.Transformers;
using Truesight.Decompiler.Hir.Traversal.Traversers;
using XenoGears;
using XenoGears.Functional;
using XenoGears.Collections.Virtual;
using XenoGears.Traits.Cloneable;
using XenoGears.Assertions;

namespace Truesight.Decompiler.Hir.Core.Expressions
{
    [DebuggerDisplay("{ToDebugString_WithParentInfo(), nq}{\"\", nq}")]
    [DebuggerTypeProxy(typeof(ObjectInitDebugView))]
    [DebuggerNonUserCode]
    public class ObjectInit : Expression
    {
        public Eval Ctor { get { return (Eval)Children[0]; } set { SetProperty("Ctor", v => Children[0] = v, Children[0], value); } }
        // todo. make this observable and also report changes using FirePropertyChanging/Changed providing ListChangeEventArgs as a tag to the event
        private readonly List<MemberInfo> _impl = new List<MemberInfo>();
        public ReadOnlyCollection<MemberInfo> Members { get { return _impl.ToReadOnly(); } }
        public IDictionary<MemberInfo, Expression> MemberInits
        {
            get
            {
                return new VirtualDictionary<MemberInfo, Expression>(
                    () => 0.UpTo(_impl.Count() - 1).ToDictionary(i => _impl[i], i => (Expression)Children[i]),
                    (k, v) =>
                    {
                        _impl.Contains(k).AssertFalse();
                        _impl.Add(k);
                        Children.Add(v);
                    },
                    (k, v) =>
                    {
                        var i = _impl.IndexOf(k);
                        if (i == -1)
                        {
                            _impl.Add(k);
                            Children.Add(v);
                        }
                        else
                        {
                            Children[i + 1] = v;
                        }
                    },
                    k =>
                    {
                        var i = _impl.IndexOf(k);
                        if (i != -1)
                        {
                            Children.RemoveAt(i + 1);
                        }
                    });
            }
        }

        public ObjectInit(Eval ctor, IEnumerable<KeyValuePair<MemberInfo, Expression>> memberInits)
            : base(NodeType.ObjectInit, ctor)
        {
            memberInits.ForEach(MemberInits.Add);
        }

        public ObjectInit(Eval ctor, params IEnumerable<KeyValuePair<MemberInfo, Expression>>[] memberInits)
            : this(ctor, memberInits.SelectMany(_ => _))
        {
        }

        protected override bool EigenEquiv(Node node)
        {
            if (!base.EigenEquiv(node)) return false;
            var other = node as ObjectInit;
            return this._impl.AllMatch(other._impl, Equals);
        }

        protected override int EigenHashCode()
        {
            return _impl.Fold(base.EigenHashCode(), (h, m) => h ^ m.SafeHashCode());
        }

        public new ObjectInit DeepClone()
        {
            return ((ICloneable2)this).DeepClone<ObjectInit>();
        }

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceObjectInit(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseObjectInit(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                var members = Members.ToDictionary(mi => mi, mi => transformer.Transform(MemberInits[mi]).AssertCast<Expression>());
                var ctor = transformer.Transform(Ctor).AssertCast<Eval>();
                return new ObjectInit(ctor, members);
            }
            else
            {
                return transformer.TransformObjectInit(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class ObjectInitDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly ObjectInit _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public ObjectInitDebugView(ObjectInit node) : this(node, null) { }
            public ObjectInitDebugView(ObjectInit node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public ObjectInitDebugView(ObjectInit node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithParentInfo(); }

            [DebuggerDisplay("{aParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object aParent { get { return (_node.Stmt().Fluent(e => e == _node ? null : e) ?? _node.Parent).CreateDebugProxy(this); } }

            [DebuggerDisplay("{bCtor, nq}{\"\", nq}", Name = "Ctor")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object bCtor { get { return _node.Ctor.CreateDebugProxy(this); } }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Object zMembers
            {
                get
                {
                    var ordered = _node._impl.Select(mi => _node.MemberInits[mi]).ToReadOnly();
                    return ordered.CreateDebugProxy(this, (_, i) => _node._impl[i].Name);
                }
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class ObjectInitDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly ObjectInit _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public ObjectInitDebugView_NoParent(ObjectInit node) : this(node, null) { }
            public ObjectInitDebugView_NoParent(ObjectInit node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) { }
            public ObjectInitDebugView_NoParent(ObjectInit node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerDisplay("{bCtor, nq}{\"\", nq}", Name = "Ctor")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object bCtor { get { return _node.Ctor.CreateDebugProxy(this); } }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Object zMembers
            {
                get
                {
                    var ordered = _node._impl.Select(mi => _node.MemberInits[mi]).ToReadOnly();
                    return ordered.CreateDebugProxy(this, (_, i) => _node._impl[i].Name);
                }
            }
        }
    }
}