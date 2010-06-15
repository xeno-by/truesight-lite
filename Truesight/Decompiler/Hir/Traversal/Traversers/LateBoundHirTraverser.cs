using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Truesight.Decompiler.Hir.Core.ControlFlow;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Core.Functional;
using Truesight.Decompiler.Hir.Core.Special;
using Truesight.Decompiler.Hir.Traversal.Exceptions;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Collections;
using Convert=Truesight.Decompiler.Hir.Core.Expressions.Convert;

namespace Truesight.Decompiler.Hir.Traversal.Traversers
{
    [DebuggerNonUserCode]
    public class LateBoundHirTraverser : AbstractHirTraverser
    {
        private static readonly ReadOnlyCollection<Type> _allowedTypes;
        static LateBoundHirTraverser()
        {
            var nodeTypes = Enum.GetNames(typeof(NodeType));
            var allowedTypes = nodeTypes.Select(nt => typeof(Node).Assembly.GetTypes().AssertSingle(
                t => typeof(Node).IsAssignableFrom(t) && t.Name == nt));
            allowedTypes = allowedTypes.Concat(typeof(Node), typeof(Expression), typeof(Clause));
            _allowedTypes = allowedTypes.ToReadOnly();
        }

        private readonly ReadOnlyDictionary<Type, Action<Node>> _visitors;
        public LateBoundHirTraverser(IEnumerable<KeyValuePair<Type, Action<Node>>> visitors)
        {
            _visitors = visitors.ToDictionary().ToReadOnly();
            _visitors.Keys.AssertEach(t => _allowedTypes.Contains(t));
        }

        private readonly Dictionary<Type, Type> _dispatchCache = new Dictionary<Type, Type>();
        private void Dispatch(Node node)
        {
            if (node == null)
            {
                TraverseNull(null);
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
                if (target != null)
                {
                    _visitors[target](node);
                }
                else
                {
                    node.Unsupported();
                }
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

        protected internal override void TraverseNull(Null @null)
        {
            Dispatch(@null);
        }

        protected internal override void TraverseAddr(Addr addr)
        {
            Dispatch(addr);
        }

        protected internal override void TraverseAssign(Assign ass)
        {
            Dispatch(ass);
        }

        protected internal override void TraverseCollectionInit(CollectionInit ci)
        {
            Dispatch(ci);
        }

        protected internal override void TraverseConditional(Conditional cond)
        {
            Dispatch(cond);
        }

        protected internal override void TraverseConst(Const @const)
        {
            Dispatch(@const);
        }

        protected internal override void TraverseConvert(Convert cvt)
        {
            Dispatch(cvt);
        }

        protected internal override void TraverseDeref(Deref deref)
        {
            Dispatch(deref);
        }

        protected internal override void TraverseFld(Fld fld)
        {
            Dispatch(fld);
        }

        protected internal override void TraverseLoophole(Loophole loophole)
        {
            Dispatch(loophole);
        }

        protected internal override void TraverseObjectInit(ObjectInit oi)
        {
            Dispatch(oi);
        }

        protected internal override void TraverseOperator(Operator op)
        {
            Dispatch(op);
        }

        protected internal override void TraverseProp(Prop prop)
        {
            Dispatch(prop);
        }

        protected internal override void TraverseRef(Ref @ref)
        {
            Dispatch(@ref);
        }

        protected internal override void TraverseSizeof(SizeOf @sizeof)
        {
            Dispatch(@sizeof);
        }

        protected internal override void TraverseTypeIs(TypeIs typeIs)
        {
            Dispatch(typeIs);
        }

        protected internal override void TraverseTypeAs(TypeAs typeAs)
        {
            Dispatch(typeAs);
        }

        protected internal override void TraverseApply(Apply apply)
        {
            Dispatch(apply);
        }

        protected internal override void TraverseEval(Eval eval)
        {
            Dispatch(eval);
        }

        protected internal override void TraverseLambda(Lambda lambda)
        {
            Dispatch(lambda);
        }

        protected internal override void TraverseBlock(Block block)
        {
            Dispatch(block);
        }

        protected internal override void TraverseBreak(Break @break)
        {
            Dispatch(@break);
        }

        protected internal override void TraverseCatch(Catch @catch)
        {
            Dispatch(@catch);
        }

        protected internal override void TraverseClause(Clause clause)
        {
            Dispatch(clause);
        }

        protected internal override void TraverseContinue(Continue @continue)
        {
            Dispatch(@continue);
        }

        protected internal override void TraverseFinally(Finally @finally)
        {
            Dispatch(@finally);
        }

        protected internal override void TraverseGoto(Goto @goto)
        {
            Dispatch(@goto);
        }

        protected internal override void TraverseIf(If @if)
        {
            Dispatch(@if);
        }

        protected internal override void TraverseIter(Iter iter)
        {
            Dispatch(iter);
        }

        protected internal override void TraverseLabel(Label label)
        {
            Dispatch(label);
        }

        protected internal override void TraverseLoop(Loop loop)
        {
            Dispatch(loop);
        }

        protected internal override void TraverseReturn(Return @return)
        {
            Dispatch(@return);
        }

        protected internal override void TraverseThrow(Throw @throw)
        {
            Dispatch(@throw);
        }

        protected internal override void TraverseTry(Try @try)
        {
            Dispatch(@try);
        }

        protected internal override void TraverseUsing(Using @using)
        {
            Dispatch(@using);
        }
    }
}