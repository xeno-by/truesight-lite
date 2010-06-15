using System;
using System.Diagnostics;
using System.Linq;
using Truesight.Decompiler.Hir.Traversal;
using Truesight.Decompiler.Hir.TypeInference;
using XenoGears;
using XenoGears.Functional;
using XenoGears.Traits.Cloneable;
using XenoGears.Strings;

namespace Truesight.Decompiler.Hir.Core.Expressions
{
    [DebuggerDisplay("{ToDebugString_WithParentInfo(), nq}{\"\", nq}")]
    [DebuggerTypeProxy(typeof(BinaryOperatorDebugView))]
    [DebuggerNonUserCode]
    public class BinaryOperator : Operator
    {
        public Expression Lhs { get { return Args[0]; } set { SetProperty("Lhs", v => Args[0] = v, Args[0], value); } }
        public Expression Rhs { get { return Args[1]; } set { SetProperty("Rhs", v => Args[1] = v, Args[1], value); } }

        public BinaryOperator(OperatorType operatorType)
            : base(operatorType)
        {
        }

        public BinaryOperator(OperatorType operatorType, Expression lhs, Expression rhs)
            : base(operatorType, lhs, rhs)
        {
        }

        public new BinaryOperator DeepClone()
        {
            return ((ICloneable2)this).DeepClone<BinaryOperator>();
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class BinaryOperatorDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly BinaryOperator _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public BinaryOperatorDebugView(BinaryOperator node) : this(node, null) { }
            public BinaryOperatorDebugView(BinaryOperator node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public BinaryOperatorDebugView(BinaryOperator node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
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

            [DebuggerDisplay("{cLhs, nq}{\"\", nq}", Name = "Lhs")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object cLhs { get { return _node.Lhs.CreateDebugProxy(this); } }

            [DebuggerDisplay("{dRhs, nq}{\"\", nq}", Name = "Rhs")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object dRhs { get { return _node.Rhs.CreateDebugProxy(this); } }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class BinaryOperatorDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly BinaryOperator _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public BinaryOperatorDebugView_NoParent(BinaryOperator node) : this(node, null) { }
            public BinaryOperatorDebugView_NoParent(BinaryOperator node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public BinaryOperatorDebugView_NoParent(BinaryOperator node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
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

            [DebuggerDisplay("{cLhs, nq}{\"\", nq}", Name = "Lhs")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object cLhs { get { return _node.Lhs.CreateDebugProxy(this); } }

            [DebuggerDisplay("{dRhs, nq}{\"\", nq}", Name = "Rhs")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object dRhs { get { return _node.Rhs.CreateDebugProxy(this); } }
        }
    }
}