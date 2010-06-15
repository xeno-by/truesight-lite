using System;
using System.Diagnostics;
using System.Linq;
using Truesight.Decompiler.Hir.Traversal;
using XenoGears;
using XenoGears.Functional;
using XenoGears.Strings;
using XenoGears.Traits.Cloneable;
using Truesight.Decompiler.Hir.TypeInference;

namespace Truesight.Decompiler.Hir.Core.Expressions
{
    [DebuggerDisplay("{ToDebugString_WithParentInfo(), nq}{\"\", nq}")]
    [DebuggerTypeProxy(typeof(UnaryOperatorDebugView))]
    [DebuggerNonUserCode]
    public class UnaryOperator : Operator
    {
        public Expression Target
        {
            get { return (Expression)Children[0]; }
            set { SetProperty("Target", v => Children[0] = v, Children[0], value); }
        }

        public UnaryOperator(OperatorType operatorType)
            : base(operatorType)
        {
        }

        public UnaryOperator(OperatorType operatorType, Expression target)
            : base(operatorType, target)
        {
        }

        public new UnaryOperator DeepClone()
        {
            return ((ICloneable2)this).DeepClone<UnaryOperator>();
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class UnaryOperatorDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly UnaryOperator _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public UnaryOperatorDebugView(UnaryOperator node) : this(node, null) { }
            public UnaryOperatorDebugView(UnaryOperator node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public UnaryOperatorDebugView(UnaryOperator node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithParentInfo(); }

            [DebuggerDisplay("{aParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object aParent { get { return (_node.Stmt().Fluent(e => e == _node ? null : e) ?? _node.Parent).CreateDebugProxy(this); } }

            [DebuggerDisplay("{bSig, nq}{\"\", nq}", Name = "Sig")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public String bSig
            {
                get
                {
                    var types = _node.InferTypes();
                    var t_args = (_node.Args ?? Seq.Empty<Expression>()).Select(arg =>
                        arg == null ? "?" : types[arg] == null ? "?" : types[arg].GetCSharpRef(ToCSharpOptions.Informative)).ToReadOnly();
                    var t_ret = types[_node] == null ? "?" : types[_node].GetCSharpRef(ToCSharpOptions.Informative);
                    return String.Format("{0} :: {1}", _node.OperatorType.ToCSharpSymbol(), t_args.Concat(t_ret).StringJoin(" -> "));
                }
            }

            [DebuggerDisplay("{cTarget, nq}{\"\", nq}", Name = "Target")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object cTarget { get { return _node.Target.CreateDebugProxy(this); } }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class UnaryOperatorDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly UnaryOperator _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public UnaryOperatorDebugView_NoParent(UnaryOperator node) : this(node, null) { }
            public UnaryOperatorDebugView_NoParent(UnaryOperator node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public UnaryOperatorDebugView_NoParent(UnaryOperator node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerDisplay("{bSig, nq}{\"\", nq}", Name = "Sig")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public String bSig
            {
                get
                {
                    var types = _node.InferTypes();
                    var t_args = (_node.Args ?? Seq.Empty<Expression>()).Select(arg =>
                        arg == null ? "?" : types[arg] == null ? "?" : types[arg].GetCSharpRef(ToCSharpOptions.Informative)).ToReadOnly();
                    var t_ret = types[_node] == null ? "?" : types[_node].GetCSharpRef(ToCSharpOptions.Informative);
                    return String.Format("{0} :: {1}", _node.OperatorType.ToCSharpSymbol(), t_args.Concat(t_ret).StringJoin(" -> "));
                }
            }

            [DebuggerDisplay("{cTarget, nq}{\"\", nq}", Name = "Target")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object cTarget { get { return _node.Target.CreateDebugProxy(this); } }
        }
    }
}