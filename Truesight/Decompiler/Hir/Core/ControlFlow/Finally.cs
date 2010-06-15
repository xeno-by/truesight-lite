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
    [DebuggerTypeProxy(typeof(FinallyDebugView))]
    [DebuggerNonUserCode]
    public class Finally : Clause
    {
        public Finally(params Node[] nodes)
            : this((IEnumerable<Node>)nodes)
        {
        }

        public Finally(IEnumerable<Node> nodes)
            : base(NodeType.Finally, nodes)
        {
        }

        public new Finally DeepClone()
        {
            return ((ICloneable2)this).DeepClone<Finally>();
        }

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceFinally(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseFinally(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                // todo. think about how to implement this without multiple clone operations
                var clause = base.AcceptTransformer(transformer, false);
                if (clause is Block && !(clause is Clause))
                {
                    var visited = new Finally(clause);
                    visited.Locals.SetElements(Locals.Select(loc => loc.DeepClone()));
                    return visited;
                }
                else
                {
                    return clause;
                }
            }
            else
            {
                return transformer.TransformFinally(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class FinallyDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Finally _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public FinallyDebugView(Finally node) : this(node, null) {}
            public FinallyDebugView(Finally node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) { }
            public FinallyDebugView(Finally node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
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
        protected internal class FinallyDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Finally _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public FinallyDebugView_NoParent(Finally node) : this(node, null) { }
            public FinallyDebugView_NoParent(Finally node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) { }
            public FinallyDebugView_NoParent(Finally node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
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