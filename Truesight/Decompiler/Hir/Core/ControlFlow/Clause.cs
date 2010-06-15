using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Truesight.Decompiler.Hir.Core.Symbols;
using Truesight.Decompiler.Hir.Traversal.Reducers;
using Truesight.Decompiler.Hir.Traversal.Transformers;
using Truesight.Decompiler.Hir.Traversal.Traversers;
using XenoGears.Functional;
using XenoGears.Strings;
using XenoGears.Traits.Cloneable;

namespace Truesight.Decompiler.Hir.Core.ControlFlow
{
    [DebuggerDisplay("{ToDebugString_WithParentInfo(), nq}{\"\", nq}")]
    [DebuggerTypeProxy(typeof(ClauseDebugView))]
    [DebuggerNonUserCode]
    public abstract class Clause : Block
    {
        protected Clause(NodeType nodeType, params Node[] nodes)
            : this(nodeType, (IEnumerable<Node>)nodes)
        {
        }

        protected Clause(NodeType nodeType, IEnumerable<Node> nodes)
            : base(nodeType, nodes)
        {
        }

        public new Clause DeepClone()
        {
            return ((ICloneable2)this).DeepClone<Clause>();
        }

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceClause(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseClause(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                return base.AcceptTransformer(transformer, false);
            }
            else
            {
                return transformer.TransformClause(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class ClauseDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Clause _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public ClauseDebugView(Clause node) : this(node, null) {}
            public ClauseDebugView(Clause node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) { }
            public ClauseDebugView(Clause node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
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
            public Object zChildren { get { return _node.Children.CreateDebugProxy(this); } }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class ClauseDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Clause _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public ClauseDebugView_NoParent(Clause node) : this(node, null) { }
            public ClauseDebugView_NoParent(Clause node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) { }
            public ClauseDebugView_NoParent(Clause node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
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
            public Object zChildren { get { return _node.Children.CreateDebugProxy(this); } }
        }
    }
}