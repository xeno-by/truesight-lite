using System;
using System.Diagnostics;
using Truesight.Decompiler.Hir.Core.Symbols;
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
    [DebuggerTypeProxy(typeof(RefDebugView))]
    [DebuggerNonUserCode]
    public class Ref : Expression
    {
        private Sym _sym; 
        public Sym Sym
        {
            get { return _sym; } 
            set { SetProperty("Sym", v => _sym = v, _sym, value); }
        }

        public Ref()
            : this(null)
        {
        }

        public Ref(Sym sym)
            : base(NodeType.Ref)
        {
            Sym = sym;
        }

        protected override bool EigenEquiv(Node node)
        {
            if (!base.EigenEquiv(node)) return false;
            var other = node as Ref;
            return Equals(this.Sym, other.Sym);
        }

        protected override int EigenHashCode()
        {
            return base.EigenHashCode() ^ Sym.SafeHashCode();
        }

        public new Ref DeepClone()
        {
            return ((ICloneable2)this).DeepClone<Ref>();
        }

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceRef(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseRef(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                var clone = new Ref(Sym.DeepClone());
                clone.Proto = this;
                return clone;
            }
            else
            {
                return transformer.TransformRef(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class RefDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Ref _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public RefDebugView(Ref node) : this(node, null) { }
            public RefDebugView(Ref node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public RefDebugView(Ref node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithParentInfo(); }

            [DebuggerDisplay("{aParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object aParent { get { return (_node.Stmt().Fluent(e => e == _node ? null : e) ?? _node.Parent).CreateDebugProxy(this); } }

            [DebuggerDisplay("{bSym == null ? null : bSym.ToString(), nq}{\"\", nq}", Name = "Sym")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Sym bSym { get { return _node.Sym; } }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class RefDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Ref _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public RefDebugView_NoParent(Ref node) : this(node, null) { }
            public RefDebugView_NoParent(Ref node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public RefDebugView_NoParent(Ref node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerDisplay("{bSym == null ? null : bSym.ToString(), nq}{\"\", nq}", Name = "Sym")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Sym bSym { get { return _node.Sym; } }
        }
    }
}