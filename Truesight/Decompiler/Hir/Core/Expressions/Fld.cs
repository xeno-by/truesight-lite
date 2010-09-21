using System;
using System.Diagnostics;
using System.Reflection;
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
    [DebuggerTypeProxy(typeof(FldDebugView))]
    [DebuggerNonUserCode]
    public class Fld : Slot
    {
        public FieldInfo Field { get { return (FieldInfo)Member; } set { SetProperty("Field", v => Member = v, Field, value); } }

        public Fld()
            : this(null, null)
        {
        }

        public Fld(FieldInfo field)
            : this(field.AssertNotNull(), null)
        {
        }

        public Fld(FieldInfo field, Expression target)
            : base(NodeType.Fld, field.AssertNotNull(), target)
        {
        }

        public new Fld DeepClone()
        {
            return ((ICloneable2)this).DeepClone<Fld>();
        }

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceFld(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseFld(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                var @this = transformer.Transform(This).AssertCast<Expression>();
                return new Fld(Field, @this).HasProto(this);
            }
            else
            {
                return transformer.TransformFld(this).HasProto(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class FldDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Fld _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public FldDebugView(Fld node) : this(node, null) { }
            public FldDebugView(Fld node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public FldDebugView(Fld node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithParentInfo(); }

            [DebuggerDisplay("{aParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object aParent { get { return (_node.Stmt().Fluent(e => e == _node ? null : e) ?? _node.Parent).CreateDebugProxy(this); } }

            [DebuggerDisplay("{bField, nq}{\"\", nq}", Name = "Field")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public String bField { get { return _node.Field == null ? "null" : _node.Field.GetCSharpRef(ToCSharpOptions.Informative); } }

            [DebuggerDisplay("{cThis, nq}{\"\", nq}", Name = "This")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object cThis { get { return _node.This.CreateDebugProxy(this); } }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class FldDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Fld _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public FldDebugView_NoParent(Fld node) : this(node, null) { }
            public FldDebugView_NoParent(Fld node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public FldDebugView_NoParent(Fld node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerDisplay("{bField, nq}{\"\", nq}", Name = "Field")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public String bField { get { return _node.Field == null ? "null" : _node.Field.GetCSharpRef(ToCSharpOptions.Informative); } }

            [DebuggerDisplay("{cThis, nq}{\"\", nq}", Name = "This")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object cThis { get { return _node.This.CreateDebugProxy(this); } }
        }
    }
}