using System;
using System.Diagnostics;
using Truesight.Decompiler.Hir.Traversal;
using Truesight.Decompiler.Hir.Traversal.Reducers;
using Truesight.Decompiler.Hir.Traversal.Transformers;
using Truesight.Decompiler.Hir.Traversal.Traversers;
using XenoGears;
using XenoGears.Traits.Cloneable;
using XenoGears.Functional;

namespace Truesight.Decompiler.Hir.Core.Expressions
{
    [DebuggerDisplay("{ToDebugString_WithParentInfo(), nq}{\"\", nq}")]
    [DebuggerTypeProxy(typeof(ConstDebugView))]
    [DebuggerNonUserCode]
    public class Const : Expression
    {
        private Type _type; public Type Type { get { return _type; } private set { SetProperty("Type", v => _type = v, _type, value); } }
        private Object _value; public Object Value { get { return _value; } private set { SetProperty("Value", v => _value = v, _value, value); } }
        public void SetValue(Object value) { SetValue(value, value == null ? null : value.GetType()); }
        public void SetValue(Object value, Type type) { Value = value; Type = type; }

        public Const(Object value)
            : this(value, value == null ? null : value.GetType())
        {
        }

        public Const(Object value, Type type)
            : base(NodeType.Const)
        {
            Value = value;
            Type = type;
        }

        protected override bool EigenEquiv(Node node)
        {
            if (!base.EigenEquiv(node)) return false;
            var other = node as Const;
            return Equals(this.Type, other.Type) && Equals(this.Value, other.Value);
        }

        protected override int EigenHashCode()
        {
            return base.EigenHashCode() ^ Type.SafeHashCode() ^ Value.SafeHashCode();
        }

        public new Const DeepClone()
        {
            return ((ICloneable2)this).DeepClone<Const>();
        }

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceConst(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseConst(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                return new Const(Value, Type);
            }
            else
            {
                return transformer.TransformConst(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class ConstDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Const _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public ConstDebugView(Const node) : this(node, null) { }
            public ConstDebugView(Const node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public ConstDebugView(Const node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithParentInfo(); }

            [DebuggerDisplay("{aParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object aParent { get { return (_node.Stmt().Fluent(e => e == _node ? null : e) ?? _node.Parent).CreateDebugProxy(this); } }

            [DebuggerDisplay("{bType, nq}{\"\", nq}", Name = "Type")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Type bType { get { return _node.Type; } }

            [DebuggerDisplay("{cValue, nq}{\"\", nq}", Name = "Value")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object cValue { get { return _node.Value; } }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class ConstDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Const _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public ConstDebugView_NoParent(Const node) : this(node, null) { }
            public ConstDebugView_NoParent(Const node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public ConstDebugView_NoParent(Const node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerDisplay("{bType, nq}{\"\", nq}", Name = "Type")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Type bType { get { return _node.Type; } }

            [DebuggerDisplay("{cValue, nq}{\"\", nq}", Name = "Value")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object cValue { get { return _node.Value; } }
        }
    }
}