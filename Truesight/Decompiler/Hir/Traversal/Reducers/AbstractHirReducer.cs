using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;
using Truesight.Decompiler.Hir.Core.ControlFlow;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Core.Functional;
using Truesight.Decompiler.Hir.Core.Special;
using Truesight.Decompiler.Hir.Traversal.Exceptions;
using XenoGears.Functional;
using XenoGears.Assertions;
using Convert = Truesight.Decompiler.Hir.Core.Expressions.Convert;

namespace Truesight.Decompiler.Hir.Traversal.Reducers
{
    [DebuggerNonUserCode]
    public abstract class AbstractHirReducer
    {
        public AbstractHirReducer<T> Cast<T>() { return this.AssertCast<AbstractHirReducer<T>>(); }
    }

    [DebuggerNonUserCode]
    public abstract class AbstractHirReducer<T> : AbstractHirReducer
    {
        private Stack<Node> _stack = new Stack<Node>();
        public ReadOnlyCollection<Node> Stack { get { return _stack.ToReadOnly(); } }

        protected virtual T ReduceNode(Node node) { return (node ?? new Null()).AcceptReducer(this); }
        public T Reduce(Node node)
        {
            using (HirReducer.SetCurrent(this))
            {
                try
                {
                    _stack.Push(node);
                    return ReduceNode(node);
                }
                catch (HirTraversalException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new HirTraversalException(node, ex, true);
                }
                finally
                {
                    _stack.Pop();
                }
            }
        }

        protected internal virtual T ReduceNull(Null @null) { throw @null.Unsupported(); }
        protected internal virtual T ReduceAddr(Addr addr) { throw addr.Unsupported(); }
        protected internal virtual T ReduceAssign(Assign ass) { throw ass.Unsupported(); }
        protected internal virtual T ReduceCollectionInit(CollectionInit ci) { throw ci.Unsupported(); }
        protected internal virtual T ReduceConditional(Conditional cond) { throw cond.Unsupported(); }
        protected internal virtual T ReduceConst(Const @const) { throw @const.Unsupported(); }
        protected internal virtual T ReduceConvert(Convert cvt) { throw cvt.Unsupported(); }
        protected internal virtual T ReduceDefault(Default @default) { throw @default.Unsupported(); }
        protected internal virtual T ReduceDeref(Deref deref) { throw deref.Unsupported(); }
        protected internal virtual T ReduceFld(Fld fld) { throw fld.Unsupported(); }
        protected internal virtual T ReduceLoophole(Loophole loophole) { throw loophole.Unsupported(); }
        protected internal virtual T ReduceObjectInit(ObjectInit oi) { throw oi.Unsupported(); }
        protected internal virtual T ReduceOperator(Operator op) { throw op.Unsupported(); }
        protected internal virtual T ReduceProp(Prop prop) { throw prop.Unsupported(); }
        protected internal virtual T ReduceRef(Ref @ref) { throw @ref.Unsupported(); }
        protected internal virtual T ReduceSizeof(SizeOf @sizeof) { throw @sizeof.Unsupported(); }
        protected internal virtual T ReduceTypeIs(TypeIs typeIs) { throw typeIs.Unsupported(); }
        protected internal virtual T ReduceTypeAs(TypeAs typeAs) { throw typeAs.Unsupported(); }
        protected internal virtual T ReduceApply(Apply apply) { throw apply.Unsupported(); }
        protected internal virtual T ReduceEval(Eval eval) { throw eval.Unsupported(); }
        protected internal virtual T ReduceLambda(Lambda lambda) { throw lambda.Unsupported(); }
        protected internal virtual T ReduceBlock(Block block) { throw block.Unsupported(); }
        protected internal virtual T ReduceBreak(Break @break) { throw @break.Unsupported(); }
        protected internal virtual T ReduceCatch(Catch @catch) { return ReduceClause(@catch); }
        protected internal virtual T ReduceClause(Clause clause) { return ReduceBlock(clause); }
        protected internal virtual T ReduceContinue(Continue @continue) { throw @continue.Unsupported(); }
        protected internal virtual T ReduceFinally(Finally @finally) { return ReduceClause(@finally); }
        protected internal virtual T ReduceGoto(Goto @goto) { throw @goto.Unsupported(); }
        protected internal virtual T ReduceIf(If @if) { throw @if.Unsupported(); }
        protected internal virtual T ReduceIter(Iter iter) { throw iter.Unsupported(); }
        protected internal virtual T ReduceLabel(Label label) { throw label.Unsupported(); }
        protected internal virtual T ReduceLoop(Loop loop) { throw loop.Unsupported(); }
        protected internal virtual T ReduceReturn(Return @return) { throw @return.Unsupported(); }
        protected internal virtual T ReduceThrow(Throw @throw) { throw @throw.Unsupported(); }
        protected internal virtual T ReduceTry(Try @try) { throw @try.Unsupported(); }
        protected internal virtual T ReduceUsing(Using @using) { throw @using.Unsupported(); }
    }
}
