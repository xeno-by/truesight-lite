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
    [DebuggerTypeProxy(typeof(ConvertDebugView))]
    [DebuggerNonUserCode]
    public class Convert : Expression
    {
        private Type _type; public Type Type { get { return _type; } private set { SetProperty("Type", v => _type = v, _type, value); } }
        public Expression Source { get { return (Expression)Children[0]; } set { SetProperty("Source", v => Children[0] = v, Children[0], value); } }

        public Convert()
            : this(null, null)
        {
        }

        public Convert(Type targetType)
            : this(targetType, null)
        {
        }

        public Convert(Type targetType, Expression source)
            : base(NodeType.Convert, source.MkArray())
        {
            Type = targetType;
        }

        protected override bool EigenEquiv(Node node)
        {
            if (!base.EigenEquiv(node)) return false;
            var other = node as Convert;
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

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceConvert(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseConvert(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                var source = transformer.Transform(Source).AssertCast<Expression>();
                return new Convert(Type, source);
            }
            else
            {
                return transformer.TransformConvert(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class ConvertDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Convert _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public ConvertDebugView(Convert node) : this(node, null) { }
            public ConvertDebugView(Convert node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public ConvertDebugView(Convert node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithParentInfo(); }

            [DebuggerDisplay("{aParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object aParent { get { return (_node.Stmt().Fluent(e => e == _node ? null : e) ?? _node.Parent).CreateDebugProxy(this); } }

            [DebuggerDisplay("{bType, nq}{\"\", nq}", Name = "Type")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public String bType { get { return _node.Type == null ? "null" : _node.Type.GetCSharpRef(ToCSharpOptions.Informative); } }

            [DebuggerDisplay("{cSource, nq}{\"\", nq}", Name = "Source")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object cSource { get { return _node.Source.CreateDebugProxy(this); } }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class ConvertDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Convert _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public ConvertDebugView_NoParent(Convert node) : this(node, null) { }
            public ConvertDebugView_NoParent(Convert node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public ConvertDebugView_NoParent(Convert node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerDisplay("{bType, nq}{\"\", nq}", Name = "Type")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public String bType { get { return _node.Type == null ? "null" : _node.Type.GetCSharpRef(ToCSharpOptions.Informative); } }

            [DebuggerDisplay("{cSource, nq}{\"\", nq}", Name = "Source")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object cSource { get { return _node.Source.CreateDebugProxy(this); } }
        }
    }
}