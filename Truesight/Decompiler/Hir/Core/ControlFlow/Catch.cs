using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Truesight.Decompiler.Hir.Core.Functional;
using Truesight.Decompiler.Hir.Core.Symbols;
using Truesight.Decompiler.Hir.Traversal.Reducers;
using Truesight.Decompiler.Hir.Traversal.Transformers;
using Truesight.Decompiler.Hir.Traversal.Traversers;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Strings;
using XenoGears.Traits.Cloneable;

namespace Truesight.Decompiler.Hir.Core.ControlFlow
{
    // this is a very rare exception to a general rule followed by Truesight
    // that says that all node-like properties must be contained among Children
    // todo. evaluate this design decision and think about consequences

    [DebuggerDisplay("{ToDebugString_WithParentInfo(), nq}{\"\", nq}")]
    [DebuggerTypeProxy(typeof(CatchDebugView))]
    [DebuggerNonUserCode]
    public class Catch : Clause
    {
        // todo. preserve types integrity:
        // 1) ExceptionType changes => update type of the Exception local
        // 2) Exception local can never change type by itself (how to verify this?!)
        // 3) Maybe even propagate ExceptionType changes to Filter's signature?
        private Type _exceptionType; public Type ExceptionType { get { return _exceptionType; } set { SetProperty("ExceptionType", v => _exceptionType = v, _exceptionType, value); } }
        public Local Exception { get { return Locals.AssertFirst(); } set { SetProperty("Exception", v => Locals[0] = v, Locals[0], value); } }
        private Lambda _filter; public Lambda Filter { get { return _filter; } set { SetProperty("Filter", v => _filter = v, _filter, value); } }

        public Catch(params Node[] nodes)
            : this(null, null, nodes)
        {
        }

        public Catch(IEnumerable<Node> nodes)
            : this(null, null, nodes)
        {
        }

        public Catch(Type exception, params Node[] nodes)
            : this(exception, null, nodes)
        {
        }

        public Catch(Type exception, IEnumerable<Node> nodes)
            : this(exception, null, nodes)
        {
        }

        public Catch(Lambda filter, params Node[] nodes)
            : this(null, filter, nodes)
        {
        }

        public Catch(Lambda filter, IEnumerable<Node> nodes)
            : this(null, filter, nodes)
        {
        }

        public Catch(Type exception, Lambda filter, params Node[] nodes)
            : this(exception, filter, (IEnumerable<Node>)nodes)
        {
        }

        public Catch(Type exception, Lambda filter, IEnumerable<Node> nodes)
            : base(NodeType.Catch, nodes)
        {
            ExceptionType = exception;
            Filter = filter ?? new Lambda(typeof(Exception), typeof(bool));
            Locals.Add(new Local("$exn", null));
        }

        public new Catch DeepClone()
        {
            return ((ICloneable2)this).DeepClone<Catch>();
        }

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceCatch(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseCatch(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                // todo. think about how to implement this without multiple clone operations
                var clause = base.AcceptTransformer(transformer, false);
                if (clause is Block && !(clause is Clause))
                {
                    var filter = transformer.Transform(Filter).AssertCast<Lambda>();
                    var visited = new Catch(ExceptionType, filter, clause);
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
                return transformer.TransformCatch(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class CatchDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Catch _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public CatchDebugView(Catch node) : this(node, null) {}
            public CatchDebugView(Catch node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) { }
            public CatchDebugView(Catch node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithParentInfo(); }

            [DebuggerDisplay("{aParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object aParent { get { return _node.Parent.CreateDebugProxy(this); } }

            [DebuggerDisplay("{bException, nq}{\"\", nq}", Name = "ExceptionType")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public String bException { get { return _node.ExceptionType == null ? "null" : _node.ExceptionType.GetCSharpRef(ToCSharpOptions.Informative); } }

            [DebuggerDisplay("{cFilter, nq}{\"\", nq}", Name = "Filter")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object cFilter { get { return _node.Filter.CreateDebugProxy(this); } }

            [DebuggerDisplay("{dLocals_DebuggerDisplay, nq}{\"\", nq}", Name = "Locals")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public IList<Local> dLocals { get { return _node.Locals; } }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private String dLocals_DebuggerDisplay
            {
                get 
                {
                    if (dLocals == null) return null;
                    var fmt = String.Format("Count = {0}", dLocals.Count());
                    if (dLocals.Count() > 0) fmt += (" (" + dLocals.Select(l => String.Format("{0} {1}",
                        l.Type.GetCSharpRef(ToCSharpOptions.Informative), l.Name)).StringJoin(", ") + ")");
                    return fmt;
                }
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Object zChildren { get { return _node.Children.CreateDebugProxy(this); } }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class CatchDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Catch _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public CatchDebugView_NoParent(Catch node) : this(node, null) { }
            public CatchDebugView_NoParent(Catch node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) { }
            public CatchDebugView_NoParent(Catch node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerDisplay("{bException, nq}{\"\", nq}", Name = "ExceptionType")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public String bException { get { return _node.ExceptionType == null ? "null" : _node.ExceptionType.GetCSharpRef(ToCSharpOptions.Informative); } }

            [DebuggerDisplay("{cFilter, nq}{\"\", nq}", Name = "Filter")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object cFilter { get { return _node.Filter.CreateDebugProxy(this); } }

            [DebuggerDisplay("{dLocals_DebuggerDisplay, nq}{\"\", nq}", Name = "Locals")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public IList<Local> dLocals { get { return _node.Locals; } }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private String dLocals_DebuggerDisplay
            {
                get
                {
                    if (dLocals == null) return null;
                    var fmt = String.Format("Count = {0}", dLocals.Count());
                    if (dLocals.Count() > 0) fmt += (" (" + dLocals.Select(l => String.Format("{0} {1}",
                        l.Type.GetCSharpRef(ToCSharpOptions.Informative), l.Name)).StringJoin(", ") + ")");
                    return fmt;
                }
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Object zChildren { get { return _node.Children.CreateDebugProxy(this); } }
        }
    }
}