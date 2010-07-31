using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Truesight.Decompiler.Hir.Core.ControlFlow;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Core.Functional;
using Truesight.Decompiler.Hir.Core.Special;
using XenoGears.Collections.Dictionaries;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Collections;
using Truesight.Decompiler.Hir.Traversal.Exceptions;
using Convert = Truesight.Decompiler.Hir.Core.Expressions.Convert;

namespace Truesight.Decompiler.Hir.Traversal.Reducers
{
    [DebuggerNonUserCode]
    public class LateBoundHirReducer<T> : AbstractHirReducer<T>
    {
        private static readonly ReadOnlyCollection<Type> _allowedTypes;
        static LateBoundHirReducer()
        {
            var nodeTypes = Enum.GetNames(typeof(NodeType));
            var allowedTypes = nodeTypes.Select(nt => typeof(Node).Assembly.GetTypes().AssertSingle(
                t => typeof(Node).IsAssignableFrom(t) && t.Name == nt));
            allowedTypes = allowedTypes.Concat(typeof(Node), typeof(Expression), typeof(Clause));
            _allowedTypes = allowedTypes.ToReadOnly();
        }

        private readonly ReadOnlyDictionary<Type, Func<Node, T>> _visitors;
        public LateBoundHirReducer(IEnumerable<KeyValuePair<Type, Func<Node, T>>> visitors)
        {
            _visitors = visitors.ToDictionary().ToReadOnly();
            _visitors.Keys.AssertEach(t => _allowedTypes.Contains(t));
        }

        private readonly Dictionary<Type, Type> _dispatchCache = new Dictionary<Type, Type>();
        private T Dispatch(Node node)
        {
            if (node == null)
            {
                return ReduceNull(null);
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
                    return _visitors[target](node);
                }
                else
                {
                    throw node.Unsupported();
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

        protected internal override T ReduceNull(Null @null)
        {
            return Dispatch(@null);
        }

        protected internal override T ReduceAddr(Addr addr)
        {
            return Dispatch(addr);
        }

        protected internal override T ReduceAssign(Assign ass)
        {
            return Dispatch(ass);
        }

        protected internal override T ReduceCollectionInit(CollectionInit ci)
        {
            return Dispatch(ci);
        }

        protected internal override T ReduceConditional(Conditional cond)
        {
            return Dispatch(cond);
        }

        protected internal override T ReduceConst(Const @const)
        {
            return Dispatch(@const);
        }

        protected internal override T ReduceConvert(Convert cvt)
        {
            return Dispatch(cvt);
        }

        protected internal override T ReduceDefault(Default @default)
        {
            return Dispatch(@default);
        }

        protected internal override T ReduceDeref(Deref deref)
        {
            return Dispatch(deref);
        }

        protected internal override T ReduceFld(Fld fld)
        {
            return Dispatch(fld);
        }

        protected internal override T ReduceLoophole(Loophole loophole)
        {
            return Dispatch(loophole);
        }

        protected internal override T ReduceObjectInit(ObjectInit oi)
        {
            return Dispatch(oi);
        }

        protected internal override T ReduceOperator(Operator op)
        {
            return Dispatch(op);
        }

        protected internal override T ReduceProp(Prop prop)
        {
            return Dispatch(prop);
        }

        protected internal override T ReduceRef(Ref @ref)
        {
            return Dispatch(@ref);
        }

        protected internal override T ReduceSizeof(SizeOf @sizeof)
        {
            return Dispatch(@sizeof);
        }

        protected internal override T ReduceTypeIs(TypeIs typeIs)
        {
            return Dispatch(typeIs);
        }

        protected internal override T ReduceTypeAs(TypeAs typeAs)
        {
            return Dispatch(typeAs);
        }

        protected internal override T ReduceApply(Apply apply)
        {
            return Dispatch(apply);
        }

        protected internal override T ReduceEval(Eval eval)
        {
            return Dispatch(eval);
        }

        protected internal override T ReduceLambda(Lambda lambda)
        {
            return Dispatch(lambda);
        }

        protected internal override T ReduceBlock(Block block)
        {
            return Dispatch(block);
        }

        protected internal override T ReduceBreak(Break @break)
        {
            return Dispatch(@break);
        }

        protected internal override T ReduceCatch(Catch @catch)
        {
            return Dispatch(@catch);
        }

        protected internal override T ReduceClause(Clause clause)
        {
            return Dispatch(clause);
        }

        protected internal override T ReduceContinue(Continue @continue)
        {
            return Dispatch(@continue);
        }

        protected internal override T ReduceFinally(Finally @finally)
        {
            return Dispatch(@finally);
        }

        protected internal override T ReduceGoto(Goto @goto)
        {
            return Dispatch(@goto);
        }

        protected internal override T ReduceIf(If @if)
        {
            return Dispatch(@if);
        }

        protected internal override T ReduceIter(Iter iter)
        {
            return Dispatch(iter);
        }

        protected internal override T ReduceLabel(Label label)
        {
            return Dispatch(label);
        }

        protected internal override T ReduceLoop(Loop loop)
        {
            return Dispatch(loop);
        }

        protected internal override T ReduceReturn(Return @return)
        {
            return Dispatch(@return);
        }

        protected internal override T ReduceThrow(Throw @throw)
        {
            return Dispatch(@throw);
        }

        protected internal override T ReduceTry(Try @try)
        {
            return Dispatch(@try);
        }

        protected internal override T ReduceUsing(Using @using)
        {
            return Dispatch(@using);
        }
    }
}