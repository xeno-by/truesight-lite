using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Traversal;
using Truesight.Decompiler.Hir.Traversal.Reducers;
using Truesight.Decompiler.Hir.Traversal.Transformers;
using Truesight.Decompiler.Hir.Traversal.Traversers;
using XenoGears;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Traits.Cloneable;

namespace Truesight.Decompiler.Hir.Core.Functional
{
    [DebuggerDisplay("{ToDebugString_WithParentInfo(), nq}{\"\", nq}")]
    [DebuggerTypeProxy(typeof(EvalDebugView))]
    [DebuggerNonUserCode]
    public class Eval : Expression
    {
        public Apply Callee
        {
            get { return (Apply)Children[0]; } 
            set { SetProperty("Callee", v => Children[0] = v, Children[0], value); }
        }

        public Eval()
            : this(null)
        {
        }

        public Eval(Apply callee)
            : base(NodeType.Eval, callee)
        {
        }

        public new Eval DeepClone()
        {
            return ((ICloneable2)this).DeepClone<Eval>();
        }

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceEval(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseEval(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                var callee = transformer.Transform(Callee).AssertCast<Apply>();
                return new Eval(callee);
            }
            else
            {
                return transformer.TransformEval(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class EvalDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Eval _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public EvalDebugView(Eval node) : this(node, null) { }
            public EvalDebugView(Eval node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) { }
            public EvalDebugView(Eval node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithParentInfo(); }

            [DebuggerDisplay("{aParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object aParent { get { return (_node.Stmt().Fluent(e => e == _node ? null : e) ?? _node.Parent).CreateDebugProxy(this); } }

            [DebuggerDisplay("{bCallee, nq}{\"\", nq}", Name = "Callee")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object bCallee { get { return (_node.Callee == null ? null : _node.Callee.Callee).CreateDebugProxy(this); } }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Object zArgs
            {
                get
                {
                    var app = _node.Callee;
                    if (app == null) return new Object[0];

                    var names = app.ArgsInfo.Zip((e, pi, i) => pi != null ? pi.Name : ("arg" + i)).ToReadOnly();
                    return names.Zip(app.Args, (name, node) => node.CreateDebugProxy(this, name)).ToArray();
                }
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class EvalDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Eval _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public EvalDebugView_NoParent(Eval node) : this(node, null) { }
            public EvalDebugView_NoParent(Eval node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) { }
            public EvalDebugView_NoParent(Eval node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerDisplay("{bCallee, nq}{\"\", nq}", Name = "Callee")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object bCallee { get { return _node.Callee.CreateDebugProxy(this); } }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Object zArgs
            {
                get
                {
                    var app = _node.Callee;
                    if (app == null) return new Object[0];

                    var names = app.ArgsInfo.Zip((e, pi, i) => pi != null ? pi.Name : ("arg" + i)).ToReadOnly();
                    return names.Zip(app.Args, (name, node) => node.CreateDebugProxy(this, name)).ToArray();
                }
            }
        }
    }
}
