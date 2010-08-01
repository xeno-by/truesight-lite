using System;
using System.Diagnostics;
using System.Linq;
using Truesight.Decompiler.Hir.Core.ControlFlow;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Core.Functional;
using Truesight.Decompiler.Hir.Core.Special;
using XenoGears.Assertions;
using XenoGears.Traits.Dumpable;
using Truesight.Decompiler.Hir.Traversal;
using XenoGears.Functional;

namespace Truesight.Decompiler.Hir
{
    [DebuggerDisplay("{ToDebugString_WithParentInfo(), nq}{\"\", nq}")]
    [DebuggerTypeProxy(typeof(NodeDebugView))]
    public abstract partial class Node
    {
        public sealed override String ToString() { return ToDebugString_WithParentInfo(); }
//        public String ToDebugString_WithoutParentInfo() { return base.ToString(); }
//        public String ToDebugString_WithParentInfo() { return base.ToString(); }
        public String ToDebugString_WithoutParentInfo() { return ToDebugString(false); }
        public String ToDebugString_WithParentInfo() { return ToDebugString(true); }
        private String ToDebugString(bool withParentInfo)
        {
            if (this is Lambda)
            {
                var lam = this.AssertCast<Lambda>();
                return lam.Sig.DumpAsText();
            }
            else if (this is Expression || this is Return || this is Throw ||
                this is Break || this is Continue || this is Label || this is Goto)
            {
                var dump = this.DumpAsText();
                if (withParentInfo)
                {
                    var stmt = this.Stmt();
                    if (stmt != null && stmt != this)
                    {
                        dump += (" | " + stmt.DumpAsText());
                    }
                }

                return dump;
            }
            else if (this is Block || this is If || this is Loop ||
                this is Try || this is Using || this is Iter)
            {
                var head = this.GetType().Name.ToLower();
                if (head.EndsWith("1")) head = head.Substring(0, head.Length - 1);

                if (this is Block)
                {
                    var block = (Block)this;
                    head += String.Format(" ({0})", block.Count());
                }
                else if (this is If)
                {
                    var @if = (If)this;
                    var worthReversing = @if.IfTrue.IsNullOrEmpty() && @if.IfFalse.IsNeitherNullNorEmpty();
                    var fst = (worthReversing ? @if.IfFalse : @if.IfTrue) ?? new Block();
                    var snd = (worthReversing ? @if.IfTrue : @if.IfFalse) ?? new Block();
                    head += String.Format(" ({0}+{1})", fst.Count(), snd.Count());
                }
                else if (this is Loop)
                {
                    var loop = (Loop)this;
                    var init = loop.Init ?? new Block();
                    var body = loop.Body ?? new Block();
                    var iter = loop.Iter ?? new Block();
                    head = (loop.IsWhileDo ? "while-do" : "do-while") +
                        String.Format(" ({0}+{1}+{2})", init.Count(), body.Count(), iter.Count());
                }
                else if (this is Try)
                {
                    var @try = (Try)this;
                    var body = @try.Body ?? new Block();
                    var clauses = @try.Clauses ?? Seq.Empty<Clause>();
                    head = String.Format(" ({0}) + {1} clause{2}",
                        body.Count(), clauses.Count(), clauses.Count() != 1 ? "s" : "");
                }
                else if (this is Using)
                {
                    var @using = (Using)this;
                    var body = @using.Body ?? new Block();
                    head = String.Format(" ({0})", body.Count());
                }
                else if (this is Iter)
                {
                    var loop_each = (Iter)this;
                    var body = loop_each.Body ?? new Block();
                    head = String.Format(" ({0})", body.Count());
                }
                else
                {
                    throw AssertionHelper.Fail();
                }

                return head;
            }
            else if (this is Null)
            {
                return "null";
            }
            else
            {
                throw AssertionHelper.Fail();
            }
        }

        internal interface INodeDebugView
        {
            Node Node { get; }
            INodeDebugView Parent { get; }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class NodeDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Node _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public NodeDebugView(Node node) : this(node, null) {}
            public NodeDebugView(Node node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public NodeDebugView(Node node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerDisplay("{aNodeType, nq}{\"\", nq}", Name = "NodeType")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public NodeType aNodeType { get { return _node.NodeType; } }

            [DebuggerDisplay("{bParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object bParent { get { return _node.Parent.CreateDebugProxy(this); } }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Object zChildren { get { return _node.Children.CreateDebugProxy(this); } }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class NodeDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Node _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public NodeDebugView_NoParent(Node node) : this(node, null) { }
            public NodeDebugView_NoParent(Node node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public NodeDebugView_NoParent(Node node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerDisplay("{aNodeType, nq}{\"\", nq}", Name = "NodeType")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public NodeType aNodeType { get { return _node.NodeType; } }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Object zChildren { get { return _node.Children.CreateDebugProxy(this); } }
        }
    }
}
