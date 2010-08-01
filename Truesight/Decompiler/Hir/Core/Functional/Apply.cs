using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Traversal;
using Truesight.Decompiler.Hir.Traversal.Reducers;
using Truesight.Decompiler.Hir.Traversal.Transformers;
using Truesight.Decompiler.Hir.Traversal.Traversers;
using XenoGears;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Collections.Virtual;
using XenoGears.Traits.Cloneable;

namespace Truesight.Decompiler.Hir.Core.Functional
{
    [DebuggerDisplay("{ToDebugString_WithParentInfo(), nq}{\"\", nq}")]
    [DebuggerTypeProxy(typeof(ApplyDebugView))]
    [DebuggerNonUserCode]
    public class Apply : Expression
    {
        public Expression Callee { get { return (Expression)Children[0]; } set { SetProperty("Callee", v => Children[0] = v, Children[0], value); } }
        // todo. make this observable and also report changes using FirePropertyChanging/Changed providing ListChangeEventArgs as a tag to the event
        // note. to make this observable, please, do NOT just append .Observe() to the ctor
        // since it will just create an observable ***copy*** of current Children
        // when Children will change, the copy will not and you'll spend shitloads of time debugging that
        public IList<Expression> Args
        {
            get
            {
                return new VirtualList<Expression>(
                    () => Children.Skip(1).Cast<Expression>(),
                    (i, e) => Children.Insert(i + 1, e),
                    (i, e) => Children[i + 1] = e,
                    i => Children.RemoveAt(i + 1));
            }
        }

        public ArgsInfo ArgsInfo
        {
            get
            {
                return new ArgsInfo(this);
            }
        }

        public Apply(Expression callee, params Expression[] args)
            : base(NodeType.Apply, callee.Concat(args))
        {
        }

        public Apply(Expression callee, IEnumerable<Expression> args)
            : this(callee, args.ToArray())
        {
        }

        public new Apply DeepClone()
        {
            return ((ICloneable2)this).DeepClone<Apply>();
        }

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceApply(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseApply(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                var args = Args.Select(arg => transformer.Transform(arg)).AssertCast<Expression>();
                var callee = transformer.Transform(Callee).AssertCast<Expression>();
                return new Apply(callee, args);
            }
            else
            {
                return transformer.TransformApply(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class ApplyDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Apply _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public ApplyDebugView(Apply node) : this(node, null) { }
            public ApplyDebugView(Apply node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public ApplyDebugView(Apply node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithParentInfo(); }

            [DebuggerDisplay("{aParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object aParent
            {
                get
                {
                    var effectiveParent = _node.Parent;
                    if (effectiveParent is Eval) effectiveParent = effectiveParent.Parent;
                    return (_node.Stmt().Fluent(e => e == _node ? null : e) ?? effectiveParent).CreateDebugProxy(this);
                }
            }

            [DebuggerDisplay("{bCallee, nq}{\"\", nq}", Name = "Callee")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object bCallee { get { return _node.Callee.CreateDebugProxy(this); } }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Object zArgs
            {
                get
                {
                    var names = _node.ArgsInfo.Zip((e, pi, i) => pi != null ? pi.Name : ("arg" + i)).ToReadOnly();
                    return names.Zip(_node.Args, (name, node) => node.CreateDebugProxy(this, name)).ToArray();
                }
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class ApplyDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Apply _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public ApplyDebugView_NoParent(Apply node) : this(node, null) { }
            public ApplyDebugView_NoParent(Apply node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public ApplyDebugView_NoParent(Apply node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerDisplay("{bCallee, nq}{\"\", nq}", Name = "Callee")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object bCallee { get { return _node.Callee.CreateDebugProxy(this); } }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Object zArgs
            {
                get
                {
                    var names = _node.ArgsInfo.Zip((e, pi, i) => pi != null ? pi.Name : ("arg" + i)).ToReadOnly();
                    return names.Zip(_node.Args, (name, node) => node.CreateDebugProxy(this, name)).ToArray();
                }
            }
        }
    }
}