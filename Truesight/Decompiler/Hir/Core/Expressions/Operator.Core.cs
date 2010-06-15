using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Truesight.Decompiler.Hir.Traversal;
using Truesight.Decompiler.Hir.Traversal.Reducers;
using Truesight.Decompiler.Hir.Traversal.Transformers;
using Truesight.Decompiler.Hir.Traversal.Traversers;
using Truesight.Decompiler.Hir.TypeInference;
using XenoGears;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Collections.Virtual;
using XenoGears.Strings;
using XenoGears.Traits.Cloneable;

namespace Truesight.Decompiler.Hir.Core.Expressions
{
    [DebuggerDisplay("{ToDebugString_WithParentInfo(), nq}{\"\", nq}")]
    [DebuggerTypeProxy(typeof(OperatorDebugView))]
    [DebuggerNonUserCode]
    public abstract partial class Operator : Expression
    {
        public OperatorType OperatorType { get; private set; }

        private IList<Expression> _args;
        // todo. make this observable and also report changes using FirePropertyChanging/Changed providing ListChangeEventArgs as a tag to the event
        // note. to make this observable, please, do NOT just append .Observe() to the ctor
        // since it will just create an observable ***copy*** of current Children
        // when Children will change, the copy will not and you'll spend shitloads of time debugging that
        public IList<Expression> Args
        {
            get
            {
                if (_args == null)
                {
                    _args = new VirtualList<Expression>(
                        () => this.Children.Cast<Expression>(),
                        (i, e) => this.Children.Insert(i, e),
                        (i, e) => this.Children[i] = e,
                        i => this.Children.RemoveAt(i));
                }

                return _args;
            }
        }

        protected Operator(OperatorType operatorType)
            : this(operatorType, operatorType.Arity().Times((Expression)null))
        {
        }

        protected Operator(OperatorType operatorType, params Expression[] children)
            : base(NodeType.Operator, children)
        {
            OperatorType = operatorType;

            var expectedArgc = operatorType.Arity();
            (children.Count() == expectedArgc).AssertTrue();
        }

        protected Operator(OperatorType operatorType, IEnumerable<Expression> children)
            : this(operatorType, children.ToArray())
        {
        }

        protected override bool EigenEquiv(Node node)
        {
            if (!base.EigenEquiv(node)) return false;
            var other = node as Operator;
            return Equals(this.OperatorType, other.OperatorType);
        }

        protected override int EigenHashCode()
        {
            return base.EigenHashCode() ^ OperatorType.GetHashCode();
        }

        public new Operator DeepClone()
        {
            return ((ICloneable2)this).DeepClone<Operator>();
        }

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceOperator(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseOperator(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                var args = Args.Select(arg => transformer.Transform(arg)).AssertCast<Expression>();
                return Operator.Create(OperatorType, args);
            }
            else
            {
                return transformer.TransformOperator(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class OperatorDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Operator _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public OperatorDebugView(Operator node) : this(node, null) { }
            public OperatorDebugView(Operator node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public OperatorDebugView(Operator node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithParentInfo(); }

            [DebuggerDisplay("{aNodeType, nq}{\"\", nq}", Name = "NodeType")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public NodeType aNodeType { get { return _node.NodeType; } }

            [DebuggerDisplay("{bParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object bParent { get { return (_node.Stmt().Fluent(e => e == _node ? null : e) ?? _node.Parent).CreateDebugProxy(this); } }

            [DebuggerDisplay("{cSig, nq}{\"\", nq}", Name = "Sig")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public String cSig
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

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Object zArgs { get { return _node.Args.CreateDebugProxy(this); } }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class OperatorDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Operator _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public OperatorDebugView_NoParent(Operator node) : this(node, null) { }
            public OperatorDebugView_NoParent(Operator node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public OperatorDebugView_NoParent(Operator node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerDisplay("{aNodeType, nq}{\"\", nq}", Name = "NodeType")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public NodeType aNodeType { get { return _node.NodeType; } }

            [DebuggerDisplay("{cSig, nq}{\"\", nq}", Name = "Sig")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public String cSig
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

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Object zArgs { get { return _node.Args.CreateDebugProxy(this); } }
        }
    }
}