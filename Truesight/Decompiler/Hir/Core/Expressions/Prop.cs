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
using XenoGears.Reflection;

namespace Truesight.Decompiler.Hir.Core.Expressions
{
    [DebuggerDisplay("{ToDebugString_WithParentInfo(), nq}{\"\", nq}")]
    [DebuggerTypeProxy(typeof(PropDebugView))]
    [DebuggerNonUserCode]
    public class Prop : Slot
    {
        // note. this property ain't equal to the obvious value of Property.IsVirtual
        // it rather tells us about the virtuality of this particular invocation
        private bool _invokedAsVirtual;
        public bool InvokedAsVirtual
        {
            get { return _invokedAsVirtual; } 
            set
            {
                if (value) (Property == null || Property.IsVirtual()).AssertTrue();
                SetProperty("InvokedAsVirtual", v => _invokedAsVirtual = v, _invokedAsVirtual, value);
            }
        }

        public PropertyInfo Property
        {
            get { return (PropertyInfo)Member; } 
            set
            {
                if (value != null) (InvokedAsVirtual && !Property.IsVirtual()).AssertFalse();
                SetProperty("Property", v => Member = v, Property, value);
            }
        }

        public Prop()
            : this(false)
        {
        }

        public Prop(bool isVirtual)
            : this(null, null, isVirtual)
        {
        }

        public Prop(PropertyInfo property)
            : this(property.AssertNotNull(), false)
        {
        }

        public Prop(PropertyInfo property, bool isVirtual)
            : this(property.AssertNotNull(), null, isVirtual)
        {
        }

        public Prop(PropertyInfo property, Expression target)
            : this(property.AssertNotNull(), target, false)
        {
        }

        public Prop(PropertyInfo property, Expression target, bool isVirtual)
            : base(NodeType.Prop, property.AssertNotNull(), target)
        {
            InvokedAsVirtual = isVirtual;
        }

        protected override bool EigenEquiv(Node node)
        {
            if (!base.EigenEquiv(node)) return false;
            var other = node as Prop;
            return Equals(this.InvokedAsVirtual, other.InvokedAsVirtual);
        }

        protected override int EigenHashCode()
        {
            return base.EigenHashCode() ^ InvokedAsVirtual.GetHashCode();
        }

        public new Prop DeepClone()
        {
            return ((ICloneable2)this).DeepClone<Prop>();
        }

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceProp(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseProp(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                var @this = transformer.Transform(This).AssertCast<Expression>();
                return new Prop(Property, @this, InvokedAsVirtual);
            }
            else
            {
                return transformer.TransformProp(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class PropDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Prop _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public PropDebugView(Prop node) : this(node, null) { }
            public PropDebugView(Prop node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public PropDebugView(Prop node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithParentInfo(); }

            [DebuggerDisplay("{aParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object aParent { get { return (_node.Stmt().Fluent(e => e == _node ? null : e) ?? _node.Parent).CreateDebugProxy(this); } }

            [DebuggerDisplay("{bProperty, nq}{\"\", nq}", Name = "Property")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public String bProperty
            {
                get
                {
                    var virt = _node.InvokedAsVirtual ? "virtual " : "";
                    var sig = _node.Property == null ? "?" : _node.Property.GetCSharpRef(ToCSharpOptions.Informative);
                    return virt + sig;
                }
            }

            [DebuggerDisplay("{cThis, nq}{\"\", nq}", Name = "This")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object cThis { get { return _node.This.CreateDebugProxy(this); } }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class PropDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Prop _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public PropDebugView_NoParent(Prop node) : this(node, null) { }
            public PropDebugView_NoParent(Prop node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public PropDebugView_NoParent(Prop node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerDisplay("{bProperty, nq}{\"\", nq}", Name = "Property")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public String bProperty
            {
                get
                {
                    var virt = _node.InvokedAsVirtual ? "virtual " : "";
                    var sig = _node.Property == null ? "?" : _node.Property.GetCSharpRef(ToCSharpOptions.Informative);
                    return virt + sig;
                }
            }

            [DebuggerDisplay("{cThis, nq}{\"\", nq}", Name = "This")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object cThis { get { return _node.This.CreateDebugProxy(this); } }
        }
    }
}