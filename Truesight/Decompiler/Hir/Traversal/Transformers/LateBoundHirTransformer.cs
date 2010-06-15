using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Truesight.Decompiler.Hir.Core.ControlFlow;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Core.Functional;
using Truesight.Decompiler.Hir.Core.Special;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Collections;
using Convert=Truesight.Decompiler.Hir.Core.Expressions.Convert;

namespace Truesight.Decompiler.Hir.Traversal.Transformers
{
    [DebuggerNonUserCode]
    public class LateBoundHirTransformer : AbstractHirTransformer
    {
        private static readonly ReadOnlyCollection<Type> _allowedTypes;
        static LateBoundHirTransformer()
        {
            var nodeTypes = Enum.GetNames(typeof(NodeType));
            var allowedTypes = nodeTypes.Select(nt => typeof(Node).Assembly.GetTypes().AssertSingle(
                t => typeof(Node).IsAssignableFrom(t) && t.Name == nt));
            allowedTypes = allowedTypes.Concat(typeof(Node), typeof(Expression), typeof(Clause));
            _allowedTypes = allowedTypes.ToReadOnly();
        }

        private readonly ReadOnlyDictionary<Type, Func<Node, Node>> _visitors;
        public LateBoundHirTransformer(IEnumerable<KeyValuePair<Type, Func<Node, Node>>> visitors)
        {
            _visitors = visitors.ToDictionary().ToReadOnly();
            _visitors.Keys.AssertEach(t => _allowedTypes.Contains(t));
        }

        private readonly Dictionary<Type, Type> _dispatchCache = new Dictionary<Type, Type>();
        private Node Dispatch(Node node)
        {
            if (node == null)
            {
                return null;
            }
            else
            {
                if (!_dispatchCache.ContainsKey(node.GetType()))
                {
                    var types = _visitors.Keys;
                    var thunk1 = new IsSuitable_Thunk(node);
                    var suitable = types.Where(thunk1.Snippet).ToReadOnly();
                    var thunk2 = new IsBetter_Thunk(suitable);
                    var better = suitable.Where(thunk2.Snippet).ToReadOnly();
                    _dispatchCache.Add(node.GetType(), better.SingleOrDefault());
                }

                var target = _dispatchCache[node.GetType()];
                return target != null ? _visitors[target](node) : node.DefaultTransform();
            }
        }

        [DebuggerNonUserCode]
        private class IsSuitable_Thunk
        {
            private readonly Node _node;
            public IsSuitable_Thunk(Node node) { _node = node; }

            public bool Snippet(Type t)
            {
                return t.IsAssignableFrom(_node.GetType());
            }
        }

        [DebuggerNonUserCode]
        private class IsBetter_Thunk
        {
            private readonly ReadOnlyCollection<Type> _suitable;
            public IsBetter_Thunk(ReadOnlyCollection<Type> suitable) { _suitable = suitable; }

            public bool Snippet(Type t)
            {
                var except = Enumerable.Except(_suitable, t.MkArray());
                return except.None(t.IsAssignableFrom);
            }
        }

        protected internal override Node TransformNull(Null @null)
        {
            return Dispatch(@null);
        }

        protected internal override Node TransformAddr(Addr addr)
        {
            return Dispatch(addr);
        }

        protected internal override Node TransformAssign(Assign ass)
        {
            return Dispatch(ass);
        }

        protected internal override Node TransformCollectionInit(CollectionInit ci)
        {
            return Dispatch(ci);
        }

        protected internal override Node TransformConditional(Conditional cond)
        {
            return Dispatch(cond);
        }

        protected internal override Node TransformConst(Const @const)
        {
            return Dispatch(@const);
        }

        protected internal override Node TransformConvert(Convert cvt)
        {
            return Dispatch(cvt);
        }

        protected internal override Node TransformDeref(Deref deref)
        {
            return Dispatch(deref);
        }

        protected internal override Node TransformFld(Fld fld)
        {
            return Dispatch(fld);
        }

        protected internal override Node TransformLoophole(Loophole loophole)
        {
            return Dispatch(loophole);
        }

        protected internal override Node TransformObjectInit(ObjectInit oi)
        {
            return Dispatch(oi);
        }

        protected internal override Node TransformOperator(Operator op)
        {
            return Dispatch(op);
        }

        protected internal override Node TransformProp(Prop prop)
        {
            return Dispatch(prop);
        }

        protected internal override Node TransformRef(Ref @ref)
        {
            return Dispatch(@ref);
        }

        protected internal override Node TransformSizeof(SizeOf @sizeof)
        {
            return Dispatch(@sizeof);
        }

        protected internal override Node TransformTypeIs(TypeIs typeIs)
        {
            return Dispatch(typeIs);
        }

        protected internal override Node TransformTypeAs(TypeAs typeAs)
        {
            return Dispatch(typeAs);
        }

        protected internal override Node TransformApply(Apply apply)
        {
            return Dispatch(apply);
        }

        protected internal override Node TransformEval(Eval eval)
        {
            return Dispatch(eval);
        }

        protected internal override Node TransformLambda(Lambda lambda)
        {
            return Dispatch(lambda);
        }

        protected internal override Node TransformBlock(Block block)
        {
            return Dispatch(block);
        }

        protected internal override Node TransformBreak(Break @break)
        {
            return Dispatch(@break);
        }

        protected internal override Node TransformCatch(Catch @catch)
        {
            return Dispatch(@catch);
        }

        protected internal override Node TransformClause(Clause clause)
        {
            return Dispatch(clause);
        }

        protected internal override Node TransformContinue(Continue @continue)
        {
            return Dispatch(@continue);
        }

        protected internal override Node TransformFinally(Finally @finally)
        {
            return Dispatch(@finally);
        }

        protected internal override Node TransformGoto(Goto @goto)
        {
            return Dispatch(@goto);
        }

        protected internal override Node TransformIf(If @if)
        {
            return Dispatch(@if);
        }

        protected internal override Node TransformIter(Iter iter)
        {
            return Dispatch(iter);
        }

        protected internal override Node TransformLabel(Label label)
        {
            return Dispatch(label);
        }

        protected internal override Node TransformLoop(Loop loop)
        {
            return Dispatch(loop);
        }

        protected internal override Node TransformReturn(Return @return)
        {
            return Dispatch(@return);
        }

        protected internal override Node TransformThrow(Throw @throw)
        {
            return Dispatch(@throw);
        }

        protected internal override Node TransformTry(Try @try)
        {
            return Dispatch(@try);
        }

        protected internal override Node TransformUsing(Using @using)
        {
            return Dispatch(@using);
        }
    }
}