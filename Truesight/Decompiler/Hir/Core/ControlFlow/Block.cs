using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Truesight.Decompiler.Hir.Core.Symbols;
using Truesight.Decompiler.Hir.Traversal.Reducers;
using Truesight.Decompiler.Hir.Traversal.Transformers;
using Truesight.Decompiler.Hir.Traversal.Traversers;
using XenoGears.Functional;
using XenoGears.Traits.Cloneable;
using XenoGears.Strings;

namespace Truesight.Decompiler.Hir.Core.ControlFlow
{
    // todo. the more I think about that, the more it becomes apparent
    // that Blocks should have possibility to be associated with a return value
    //
    // e.g. this would come in handy when implementing inliner
    // currently I have to do dirty tricks when there arises a need 
    // to expand single expression into multiple statements
    // but if Blocks just returned the value of the last expression, everything would be fine
    //
    // todo. however implementing this functionality would require a significant overhaul of decompiler:
    // and additionally it'd require one to rethink the entire concept of ControlFlow nodes, i.e. that of statements
    // e.g. if we make Block to be an Expression, what should we do with Ifs, Loops, Tries, Usings...
    // I could look up that in functional language specs (e.g. in F#), tho don't really have time for this
    // so I'm leaving this as it is for now and marking it as "to be implemented"

    [DebuggerDisplay("{ToDebugString_WithParentInfo(), nq}{\"\", nq}")]
    [DebuggerTypeProxy(typeof(BlockDebugView))]
    [DebuggerNonUserCode]
    public partial class Block : Node, IList<Node>
    {
        #region Conceptually impure, but pragmatic convenience methods

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        public IEnumerator<Node> GetEnumerator() { return Children.GetEnumerator(); }
        public void Add(Node item) { Children.Add(item); }
        public void Clear() { Children.Clear(); }
        public bool Contains(Node item) { return Children.Contains(item); }
        void ICollection<Node>.CopyTo(Node[] array, int arrayIndex) { Children.CopyTo(array, arrayIndex); }
        public bool Remove(Node item) { return Children.Remove(item); }
        int ICollection<Node>.Count { get { return Children.Count(); } }
        bool ICollection<Node>.IsReadOnly { get { return Children.IsReadOnly; } }
        public int IndexOf(Node item) { return Children.IndexOf(item); }
        public void Insert(int index, Node item) { Children.Insert(index, item); }
        public void RemoveAt(int index) { Children.RemoveAt(index); }
        public Node this[int index] { get { return Children[index]; } set { Children[index] = value; } }

        #endregion

        public Block(params Node[] nodes)
            : this((IEnumerable<Node>)nodes)
        {
        }

        public Block(IEnumerable<Node> nodes)
            : base(NodeType.Block, nodes)
        {
        }

        protected Block(NodeType nodeType, params Node[] nodes)
            : this(nodeType, (IEnumerable<Node>)nodes)
        {
        }

        protected Block(NodeType nodeType, IEnumerable<Node> nodes)
            : base(nodeType, nodes)
        {
        }

        public new Block DeepClone()
        {
            return ((ICloneable2)this).DeepClone<Block>();
        }

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceBlock(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseBlock(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                var stmts = this.Select(stmt => transformer.Transform(stmt));
                var visited = new Block(stmts.Where(stmt => stmt != null));
                visited.Locals.SetElements(Locals.Select(loc => loc.DeepClone()));
                return visited;
            }
            else
            {
                return transformer.TransformBlock(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class BlockDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Block _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public BlockDebugView(Block node) : this(node, null) {}
            public BlockDebugView(Block node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) { }
            public BlockDebugView(Block node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithParentInfo(); }

            [DebuggerDisplay("{aParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object aParent { get { return _node.Parent.CreateDebugProxy(this); } }

            [DebuggerDisplay("{bLocals_DebuggerDisplay, nq}{\"\", nq}", Name = "Locals")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public IList<Local> bLocals { get { return _node.Locals; } }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private String bLocals_DebuggerDisplay
            {
                get 
                {
                    if (bLocals == null) return null;
                    var fmt = String.Format("Count = {0}", bLocals.Count());
                    if (bLocals.Count() > 0) fmt += (" (" + bLocals.Select(l => String.Format("{0} {1}",
                        l.Type == null ? "?" : l.Type.GetCSharpRef(ToCSharpOptions.Informative), l.Name)).StringJoin(", ") + ")");
                    return fmt;
                }
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Object zChildren { get { return _node.Children.CreateDebugProxy(this); } }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class BlockDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Block _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public BlockDebugView_NoParent(Block node) : this(node, null) { }
            public BlockDebugView_NoParent(Block node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) { }
            public BlockDebugView_NoParent(Block node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerDisplay("{bLocals_DebuggerDisplay, nq}{\"\", nq}", Name = "Locals")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public IList<Local> bLocals { get { return _node.Locals; } }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private String bLocals_DebuggerDisplay
            {
                get
                {
                    if (bLocals == null) return null;
                    var fmt = String.Format("Count = {0}", bLocals.Count());
                    if (bLocals.Count() > 0) fmt += (" (" + bLocals.Select(l => String.Format("{0} {1}",
                        l.Type == null ? "?" : l.Type.GetCSharpRef(ToCSharpOptions.Informative), l.Name)).StringJoin(", ") + ")");
                    return fmt;
                }
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Object zChildren { get { return _node.Children.CreateDebugProxy(this); } }
        }
    }
}