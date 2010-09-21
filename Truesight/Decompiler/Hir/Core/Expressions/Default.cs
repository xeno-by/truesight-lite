using System;
using System.Diagnostics;
using Truesight.Decompiler.Hir.Traversal;
using Truesight.Decompiler.Hir.Traversal.Reducers;
using Truesight.Decompiler.Hir.Traversal.Transformers;
using Truesight.Decompiler.Hir.Traversal.Traversers;
using XenoGears;
using XenoGears.Strings;
using XenoGears.Traits.Cloneable;

namespace Truesight.Decompiler.Hir.Core.Expressions
{
    [DebuggerDisplay("{ToDebugString_WithParentInfo(), nq}{\"\", nq}")]
    [DebuggerTypeProxy(typeof(DefaultDebugView))]
    [DebuggerNonUserCode]
    public class Default : Expression
    {
        private Type _type; 
        public Type Type
        {
            get { return _type; } 
            set { SetProperty("Type", v => _type = v, _type, value); }
        }

        public Default()
            : this(null)
        {
        }

        public Default(Type type)
            : base(NodeType.Default)
        {
            Type = type;
        }

        protected override bool EigenEquiv(Node node)
        {
            if (!base.EigenEquiv(node)) return false;
            var other = node as Default;
            return Equals(this.Type, other.Type);
        }

        protected override int EigenHashCode()
        {
            return base.EigenHashCode() ^ Type.SafeHashCode();
        }

        public new Convert DeepClone()
        {
            return ((ICloneable2)this).DeepClone<Convert>();
        }

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceDefault(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseDefault(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                return new Default(Type).HasProto(this);
            }
            else
            {
                return transformer.TransformDefault(this).HasProto(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class DefaultDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Default _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public DefaultDebugView(Default node) : this(node, null) { }
            public DefaultDebugView(Default node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) { }
            public DefaultDebugView(Default node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithParentInfo(); }

            [DebuggerDisplay("{aParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object aParent { get { return (_node.Stmt().Fluent(e => e == _node ? null : e) ?? _node.Parent).CreateDebugProxy(this); } }

            [DebuggerDisplay("{bType, nq}{\"\", nq}", Name = "Type")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public String bType { get { return _node.Type == null ? "null" : _node.Type.GetCSharpRef(ToCSharpOptions.Informative); } }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class DefaultDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Default _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public DefaultDebugView_NoParent(Default node) : this(node, null) { }
            public DefaultDebugView_NoParent(Default node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) { }
            public DefaultDebugView_NoParent(Default node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerDisplay("{bType, nq}{\"\", nq}", Name = "Type")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public String bType { get { return _node.Type == null ? "null" : _node.Type.GetCSharpRef(ToCSharpOptions.Informative); } }
        }
    }
}