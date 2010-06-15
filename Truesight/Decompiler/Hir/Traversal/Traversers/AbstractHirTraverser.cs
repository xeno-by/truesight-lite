using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Truesight.Decompiler.Hir.Core.ControlFlow;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Core.Functional;
using Truesight.Decompiler.Hir.Core.Special;
using Truesight.Decompiler.Hir.Traversal.Exceptions;
using Convert = Truesight.Decompiler.Hir.Core.Expressions.Convert;
using XenoGears.Functional;

namespace Truesight.Decompiler.Hir.Traversal.Traversers
{
    [DebuggerNonUserCode]
    public abstract class AbstractHirTraverser
    {
        private Stack<Node> _stack = new Stack<Node>();
        public ReadOnlyCollection<Node> Stack { get { return _stack.ToReadOnly(); } }

        protected virtual void TraverseNode(Node node) { (node ?? new Null()).AcceptTraverser(this); }
        public void Traverse(Node node)
        {
            using (HirTraverser.SetCurrent(this))
            {
                try
                {
                    _stack.Push(node);
                    TraverseNode(node);
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

        protected internal virtual void TraverseNull(Null @null) { @null.Unsupported(); }
        protected internal virtual void TraverseAddr(Addr addr) { addr.Unsupported(); }
        protected internal virtual void TraverseAssign(Assign ass) { ass.Unsupported(); }
        protected internal virtual void TraverseCollectionInit(CollectionInit ci) { ci.Unsupported(); }
        protected internal virtual void TraverseConditional(Conditional cond) { cond.Unsupported(); }
        protected internal virtual void TraverseConst(Const @const) { @const.Unsupported(); }
        protected internal virtual void TraverseConvert(Convert cvt) { cvt.Unsupported(); }
        protected internal virtual void TraverseDeref(Deref deref) { deref.Unsupported(); }
        protected internal virtual void TraverseFld(Fld fld) { fld.Unsupported(); } 
        protected internal virtual void TraverseLoophole(Loophole loophole) { loophole.Unsupported(); }
        protected internal virtual void TraverseObjectInit(ObjectInit oi) { oi.Unsupported(); }
        protected internal virtual void TraverseOperator(Operator op) { op.Unsupported(); } 
        protected internal virtual void TraverseProp(Prop prop) { prop.Unsupported(); } 
        protected internal virtual void TraverseRef(Ref @ref) { @ref.Unsupported(); }
        protected internal virtual void TraverseSizeof(SizeOf @sizeof) { @sizeof.Unsupported(); }
        protected internal virtual void TraverseTypeIs(TypeIs typeIs) { typeIs.Unsupported(); }
        protected internal virtual void TraverseTypeAs(TypeAs typeAs) { typeAs.Unsupported(); }
        protected internal virtual void TraverseApply(Apply apply) { apply.Unsupported(); }
        protected internal virtual void TraverseEval(Eval eval) { eval.Unsupported(); }
        protected internal virtual void TraverseLambda(Lambda lambda) { lambda.Unsupported(); } 
        protected internal virtual void TraverseBlock(Block block) { block.Unsupported(); } 
        protected internal virtual void TraverseBreak(Break @break) { @break.Unsupported(); }
        protected internal virtual void TraverseCatch(Catch @catch) { Traverse(@catch.Filter); TraverseClause(@catch); }
        protected internal virtual void TraverseClause(Clause clause) { TraverseBlock(clause); }
        protected internal virtual void TraverseContinue(Continue @continue) { @continue.Unsupported(); }
        protected internal virtual void TraverseFinally(Finally @finally) { TraverseClause(@finally); }
        protected internal virtual void TraverseGoto(Goto @goto) { @goto.Unsupported(); }
        protected internal virtual void TraverseIf(If @if) { @if.Unsupported(); }
        protected internal virtual void TraverseIter(Iter iter) { iter.Unsupported(); }
        protected internal virtual void TraverseLabel(Label label) { label.Unsupported(); }
        protected internal virtual void TraverseLoop(Loop loop) { loop.Unsupported(); }
        protected internal virtual void TraverseReturn(Return @return) { @return.Unsupported(); }
        protected internal virtual void TraverseThrow(Throw @throw) { @throw.Unsupported(); }
        protected internal virtual void TraverseTry(Try @try) { @try.Unsupported(); }
        protected internal virtual void TraverseUsing(Using @using) { @using.Unsupported(); }
    }
}
