using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Truesight.Decompiler.Domains;
using Truesight.Decompiler.Hir.Core.ControlFlow;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Core.Symbols;
using Truesight.Decompiler.Hir.Traversal.Reducers;
using Truesight.Decompiler.Hir.Traversal.Transformers;
using Truesight.Decompiler.Hir.Traversal.Traversers;
using Truesight.Decompiler.Pipeline;
using XenoGears;
using XenoGears.Assertions;
using XenoGears.Functional;
using XenoGears.Traits.Cloneable;
using XenoGears.Reflection;

namespace Truesight.Decompiler.Hir.Core.Functional
{
    [DebuggerDisplay("{ToDebugString_WithParentInfo(), nq}{\"\", nq}")]
    [DebuggerTypeProxy(typeof(LambdaDebugView))]
    [DebuggerNonUserCode]
    public class Lambda : Expression
    {
        public MethodBase Method { get; private set; }
        internal void EnsureSigAndBody()
        {
            if (_body == null)
            {
                if (Method != null)
                {
                    if (Method.GetMethodBody() != null)
                    {
                        if (Domain == null) Domain = Domain.Current;
                        var cached = Domain.DecompilationCache.GetOrCreate(Method, () =>
                        {
                            var pipeline = Domain.CreatePipeline();
                            var lambda = pipeline.Process(Method);
                            lambda.FreezeForever(); // so that it can be cached
                            return lambda;
                        });

                        // note. we DO need to change the signature here
                        // so that params' symbols get in sync with their usages within the body
                        // that's why Sig's accessor always calls EnsureSigAndBody first
                        Sig.Syms = cached.Sig.Syms;
                        _body = cached.Body;
                    }
                    else
                    {
                        _body = null;
                    }
                }
                else
                {
                    _body = new Block();
                }
            }
        }

        private Block _body;
        public Sig Sig { get; private set; }
        public Block Body
        {
            get
            {
                EnsureSigAndBody();
                return _body;
            }
            set
            {
                Method.AssertNull();
                _body = value ?? new Block();
            }
        }

        // note. aggregates values of InvokedAsVirtual and InvokedAsCtor flags
        // tbh, I really like the idea of moving these flags right into the Lambda
        public InvocationStyle InvocationStyle
        {
            get
            {
                (_invokedAsVirtual && _invokedAsCtor).AssertFalse();
                if (_invokedAsVirtual) return InvocationStyle.Virtual;
                else return _invokedAsCtor ? InvocationStyle.Ctor : InvocationStyle.NonVirtual;
            }
            set
            {
                SetProperty("InvocationStyle",
                v =>
                {
                    (Method != null).AssertTrue();
                    if (value == InvocationStyle.Virtual) Method.IsVirtual.AssertTrue();
                    if (value == InvocationStyle.Ctor) Method.IsConstructor.AssertTrue();
                    _invokedAsVirtual = value == InvocationStyle.Virtual;
                    _invokedAsCtor = value == InvocationStyle.Ctor;
                },
                InvocationStyle, value);
            }
        }

        // note. this property ain't equal to the obvious value of Method.IsVirtual
        // it rather tells us about the virtuality of this particular invocation/ldftn
        private bool _invokedAsVirtual;
        public bool InvokedAsVirtual
        {
            get { return _invokedAsVirtual; }
            set
            {
                // todo. also notify about changes to InvocationStyle
                SetProperty("InvokedAsVirtual",
                v =>
                {
                    (Method != null).AssertTrue();
                    if (value) Method.IsVirtual.AssertTrue();
                    _invokedAsCtor.AssertFalse();
                    _invokedAsVirtual = value;
                },
                _invokedAsVirtual, value);
            }
        }

        // note. this property ain't equal to the obvious value of Method.IsConstructor
        // it rather tells us about whether this particular invocation/ldftn is used as a constructor
        //
        // examples:
        // 1) "new Foo(bar, qux)" => lambda will have the InvokedAsCtor flag set to true
        // 2) "public Foo(Bar bar, Qux qux) : this(bar, qux, null)" => 
        //    lambda of the "this" (or "base") invocation will have the InvokedAsCtor flag set to false
        private bool _invokedAsCtor;
        public bool InvokedAsCtor
        {
            get { return _invokedAsCtor; }
            set
            {
                // todo. also notify about changes to InvocationStyle
                SetProperty("InvokedAsCtor",
                v =>
                {
                    (Method != null).AssertTrue();
                    if (value) Method.IsConstructor.AssertTrue();
                    _invokedAsVirtual.AssertFalse();
                    _invokedAsCtor = value;
                },
                _invokedAsCtor, value);
            }
        }

        public Lambda(MethodBase method)
            : this(method.AssertNotNull(), method.IsConstructor() ? InvocationStyle.Ctor : InvocationStyle.NonVirtual)
        {
        }

        public Lambda(MethodBase method, InvocationStyle invocationStyle)
            : base(NodeType.Lambda)
        {
            Method = method.AssertNotNull();
            InvocationStyle = invocationStyle;
            Sig = new Sig(this);
        }

        public Lambda(IEnumerable<Param> @params, Type ret)
            : this(new Sig(@params, ret))
        {
        }

        public Lambda(IEnumerable<Type> @params, Type ret)
            : this(new Sig(@params, ret))
        {
        }

        public Lambda(params Type[] paramsAndRet)
            : this(new Sig(paramsAndRet))
        {
        }

        public Lambda(Sig sig) 
            : base(NodeType.Lambda)
        {
            Sig = sig;
        }

        // note. this ctor is only necessary for the decompiler pipeline
        // so it's only internal but not public
        // ahhh... why don't we have friend classes/methods?!
        internal Lambda(MethodBase method, Sig decompiledSig, Block decompiledBody)
            : base(NodeType.Lambda)
        {
            Method = method;
            Sig = decompiledSig;
            _body = decompiledBody;
        }

        protected override bool EigenEquiv(Node node)
        {
            if (!base.EigenEquiv(node)) return false;
            var other = node as Lambda;
            return Equals(this.InvocationStyle, other.InvocationStyle) && 
                Equals(this.Sig, other.Sig) && Equals(this.Method, other.Method);
        }

        protected override int EigenHashCode()
        {
            return base.EigenHashCode() ^ InvocationStyle.SafeHashCode() ^ 
                Sig.SafeHashCode() ^ Method.SafeHashCode();
        }

        public new Lambda DeepClone()
        {
            return ((ICloneable2)this).DeepClone<Lambda>();
        }

        public override T AcceptReducer<T>(AbstractHirReducer<T> reducer) { return reducer.ReduceLambda(this); }
        public override void AcceptTraverser(AbstractHirTraverser traverser) { traverser.TraverseLambda(this); }
        public override Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl)
        {
            if (forceDefaultImpl)
            {
                var visited = Method != null ? new Lambda(Method) : new Lambda(Sig);
                visited.InvocationStyle = InvocationStyle;
                visited._body = transformer.Transform(_body).AssertCast<Block>();
                return visited.HasProto(this);
            }
            else
            {
                return transformer.TransformLambda(this).HasProto(this);
            }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class LambdaDebugView : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Lambda _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public LambdaDebugView(Lambda node) : this(node, null) { }
            public LambdaDebugView(Lambda node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) {}
            public LambdaDebugView(Lambda node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithParentInfo(); }

            [DebuggerDisplay("{aParent, nq}{\"\", nq}", Name = "Parent")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Object aParent { get { return _node.Parent.CreateDebugProxy(this); } }

            [DebuggerDisplay("{bSig, nq}{\"\", nq}", Name = "Sig")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Sig bSig { get { return _node.Sig; } }

            [DebuggerDisplay("{cInvocationStyle, nq}{\"\", nq}", Name = "InvocationStyle")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public InvocationStyle cInvocationStyle { get { return _node.InvocationStyle; } }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Object zBody { get { return _node.Body.CreateDebugProxy(this); } }
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class LambdaDebugView_NoParent : INodeDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Lambda _node;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Object _parentProxy;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] Node INodeDebugView.Node { get { return _node; } }
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] INodeDebugView INodeDebugView.Parent { get { return (INodeDebugView)_parentProxy; } }
            public LambdaDebugView_NoParent(Lambda node) : this(node, null) { }
            public LambdaDebugView_NoParent(Lambda node, Object parentProxy) : this(node, parentProxy, NodeDebuggabilityHelper.InferDebugProxyNameFromStackTrace()) { }
            public LambdaDebugView_NoParent(Lambda node, Object parentProxy, String name) { _node = node; _parentProxy = parentProxy; _name = name; }
            public override String ToString() { return _node == null ? null : _node.ToDebugString_WithoutParentInfo(); }

            [DebuggerDisplay("{bSig, nq}{\"\", nq}", Name = "Sig")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Sig bSig { get { return _node.Sig; } }

            [DebuggerDisplay("{cInvocationStyle, nq}{\"\", nq}", Name = "InvocationStyle")]
            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public InvocationStyle cInvocationStyle { get { return _node.InvocationStyle; } }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Object zBody { get { return _node.Body.CreateDebugProxy(this); } }
        }
    }
}