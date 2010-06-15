using System;
using System.Diagnostics;
using Truesight.Decompiler.Hir.Traversal;
using Truesight.Decompiler.Hir.Traversal.Reducers;
using Truesight.Decompiler.Hir.Traversal.Transformers;
using Truesight.Decompiler.Hir.Traversal.Traversers;
using XenoGears;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Strings;
using XenoGears.Traits.Cloneable;

namespace Truesight.Decompiler.Hir.Core.Expressions
{
    [DebuggerDisplay("{ToDebugString_WithParentInfo(), nq}{\"\", nq}")]
    [DebuggerTypeProxy(typeof(TypeAsDebugView))]
    [DebuggerNonUserCode]
    public class TypeAs : Expression
    {
        private Type _type; public Type Type { get { return _type; } private set { SetProperty("Type", v => _type = v, _type, value); } }
        public Expression Target { get { return (Expression)Children[0]; } set { SetProperty("Target", v => Children[0] = v, Children[0], value); } }

        public TypeAs()
            : this(null, null)
        {
        }

        public TypeAs(Type targetType)
            : this(targetType, null)
        {
        }

        public TypeAs(Type targetType, Expression source)
            : base(NodeType.TypeAs, source.MkArray())
        {
            Type = targetType;
        }

        protected override bool EigenEquiv(Node node)
        {
            if (!base.EigenEquiv(node)) return false;
            var other = node as TypeAs;
            return Equals(this.Type, other.Type);
        }

        protected override int EigenHashCode()
        {
            return base.EigenHashCode() ^ Type.SafeHashCode();
        }

        public new TypeAs DeepClone()
        {
            return ((ICloneable2)this).DeepClone<TypeAs>();
        }

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceTypeAs(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseTypeAs(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                var source = transformer.Transform(Target).AssertCast<Expression>();
                return new TypeAs(Type, source);
            }
            else
            {
                return transformer.TransformTypeAs(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class TypeAsDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly TypeAs _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public TypeAsDebugView(TypeAs node) : this(node, null) { }
            public TypeAsDebugView(TypeAs node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public TypeAsDebugView(TypeAs node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithParentInfo(); }

            [DebuggerDisplay("{aParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object aParent { get { return (_node.Stmt().Fluent(e => e == _node ? null : e) ?? _node.Parent).CreateDebugProxy(this); } }

            [DebuggerDisplay("{bType, nq}{\"\", nq}", Name = "Type")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public String bType { get { return _node.Type == null ? "null" : _node.Type.GetCSharpRef(ToCSharpOptions.Informative); } }

            [DebuggerDisplay("{cTarget, nq}{\"\", nq}", Name = "Target")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object cTarget { get { return _node.Target.CreateDebugProxy(this); } }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class TypeAsDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly TypeAs _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public TypeAsDebugView_NoParent(TypeAs node) : this(node, null) { }
            public TypeAsDebugView_NoParent(TypeAs node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public TypeAsDebugView_NoParent(TypeAs node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerDisplay("{bType, nq}{\"\", nq}", Name = "Type")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public String bType { get { return _node.Type == null ? "null" : _node.Type.GetCSharpRef(ToCSharpOptions.Informative); } }

            [DebuggerDisplay("{cTarget, nq}{\"\", nq}", Name = "Target")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object cTarget { get { return _node.Target.CreateDebugProxy(this); } }
        }
    }
}