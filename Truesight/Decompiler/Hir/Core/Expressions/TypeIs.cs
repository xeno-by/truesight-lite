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
    [DebuggerTypeProxy(typeof(TypeIsDebugView))]
    [DebuggerNonUserCode]
    public class TypeIs : Expression
    {
        private Type _type; public Type Type { get { return _type; } private set { SetProperty("Type", v => _type = v, _type, value); } }
        public Expression Target { get { return (Expression)Children[0]; } set { SetProperty("Target", v => Children[0] = v, Children[0], value); } }

        public TypeIs()
            : this(null, null)
        {
        }

        public TypeIs(Type targetType)
            : this(targetType, null)
        {
        }

        public TypeIs(Type targetType, Expression source)
            : base(NodeType.TypeIs, source.MkArray())
        {
            Type = targetType;
        }

        protected override bool EigenEquiv(Node node)
        {
            if (!base.EigenEquiv(node)) return false;
            var other = node as TypeIs;
            return Equals(this.Type, other.Type);
        }

        protected override int EigenHashCode()
        {
            return base.EigenHashCode() ^ Type.SafeHashCode();
        }

        public new TypeIs DeepClone()
        {
            return ((ICloneable2)this).DeepClone<TypeIs>();
        }

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceTypeIs(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseTypeIs(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                var source = transformer.Transform(Target).AssertCast<Expression>();
                return new TypeIs(Type, source);
            }
            else
            {
                return transformer.TransformTypeIs(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class TypeIsDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly TypeIs _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public TypeIsDebugView(TypeIs node) : this(node, null) { }
            public TypeIsDebugView(TypeIs node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public TypeIsDebugView(TypeIs node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
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
        internal class TypeIsDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly TypeIs _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public TypeIsDebugView_NoParent(TypeIs node) : this(node, null) { }
            public TypeIsDebugView_NoParent(TypeIs node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public TypeIsDebugView_NoParent(TypeIs node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
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