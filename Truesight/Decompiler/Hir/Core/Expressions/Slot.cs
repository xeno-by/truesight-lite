using System;
using System.Diagnostics;
using System.Reflection;
using Truesight.Decompiler.Hir.Traversal;
using XenoGears;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Reflection;
using XenoGears.Strings;
using XenoGears.Traits.Cloneable;

namespace Truesight.Decompiler.Hir.Core.Expressions
{
    [DebuggerDisplay("{ToDebugString_WithParentInfo(), nq}{\"\", nq}")]
    [DebuggerTypeProxy(typeof(SlotDebugView))]
    [DebuggerNonUserCode]
    public abstract class Slot : Expression
    {
        public Expression This
        {
            get { return (Expression)Children[0]; }
            set
            {
                SetProperty("This", 
                v =>
                {
                    Member.IsStatic().AssertImplies(v == null);
                    Children[0] = v;
                },
                Children[0], value);
            }
        }

        private MemberInfo _member;
        public MemberInfo Member
        {
            get { return _member; }
            set
            {
                SetProperty("Member",
                v =>
                {
                    v.IsStatic().AssertImplies(This == null);
                    _member = v;
                },
                _member, value);
            }
        }

        protected Slot(NodeType nodeType)
            : this(nodeType, null, null)
        {
        }

        protected Slot(NodeType nodeType, MemberInfo member)
            : this(nodeType, member, null)
        {
        }

        protected Slot(NodeType nodeType, MemberInfo member, Expression target)
            : base(nodeType, target)
        {
            Member = member;
        }

        protected override bool EigenEquiv(Node node)
        {
            if (!base.EigenEquiv(node)) return false;
            var other = node as Slot;
            return Equals(this.Member, other.Member);
        }

        protected override int EigenHashCode()
        {
            return base.EigenHashCode() ^ Member.SafeHashCode();
        }

        public new Slot DeepClone()
        {
            return ((ICloneable2)this).DeepClone<Slot>();
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class SlotDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Slot _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public SlotDebugView(Slot node) : this(node, null) { }
            public SlotDebugView(Slot node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public SlotDebugView(Slot node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithParentInfo(); }

            [DebuggerDisplay("{aNodeType, nq}{\"\", nq}", Name = "NodeType")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public NodeType aNodeType { get { return _node.NodeType; } }

            [DebuggerDisplay("{bParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object bParent { get { return (_node.Stmt().Fluent(e => e == _node ? null : e) ?? _node.Parent).CreateDebugProxy(this); } }

            [DebuggerDisplay("{cMember, nq}{\"\", nq}", Name = "Member")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public String cMember { get { return _node.Member == null ? "null" : _node.Member.GetCSharpRef(ToCSharpOptions.Informative); } }

            [DebuggerDisplay("{dThis, nq}{\"\", nq}", Name = "This")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object dThis { get { return _node.This.CreateDebugProxy(this); } }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class SlotDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Slot _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public SlotDebugView_NoParent(Slot node) : this(node, null) { }
            public SlotDebugView_NoParent(Slot node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public SlotDebugView_NoParent(Slot node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerDisplay("{aNodeType, nq}{\"\", nq}", Name = "NodeType")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public NodeType aNodeType { get { return _node.NodeType; } }

            [DebuggerDisplay("{cMember, nq}{\"\", nq}", Name = "Member")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public String cMember { get { return _node.Member == null ? "null" : _node.Member.GetCSharpRef(ToCSharpOptions.Informative); } }

            [DebuggerDisplay("{dThis, nq}{\"\", nq}", Name = "This")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object dThis { get { return _node.This.CreateDebugProxy(this); } }
        }
    }
}