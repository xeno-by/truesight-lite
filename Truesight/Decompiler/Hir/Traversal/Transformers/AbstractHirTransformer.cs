using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Truesight.Decompiler.Hir.Core.ControlFlow;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Core.Functional;
using Truesight.Decompiler.Hir.Core.Special;
using Truesight.Decompiler.Hir.Traversal.Exceptions;
using XenoGears.Functional;
using Convert = Truesight.Decompiler.Hir.Core.Expressions.Convert;

namespace Truesight.Decompiler.Hir.Traversal.Transformers
{
    [DebuggerNonUserCode]
    public abstract class AbstractHirTransformer
    {
        private Stack<Node> _stack = new Stack<Node>();
        public ReadOnlyCollection<Node> Stack { get { return _stack.ToReadOnly(); } }

        public Node Transform(Node node) { return Transform(node, false); }
        public Node DefaultTransform(Node node) { return Transform(node, true); }
        private Node Transform(Node node, bool forceDefaultImpl)
        {
            using (HirTransformer.SetCurrent(this))
            {
                try
                {
                    _stack.Push(node);
                    return TransformNode(node, forceDefaultImpl);
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

        protected virtual Node TransformNode(Node node, bool forceDefaultImpl)
        {
            return (node ?? new Null()).AcceptTransformer(this, forceDefaultImpl);
        }

        protected internal virtual Node TransformNull(Null @null) { return null; }
        protected internal virtual Node TransformAddr(Addr addr) { return addr.AcceptTransformer(this, true); }
        protected internal virtual Node TransformAssign(Assign ass) { return ass.AcceptTransformer(this, true); }
        protected internal virtual Node TransformCollectionInit(CollectionInit ci) { return ci.AcceptTransformer(this, true); }
        protected internal virtual Node TransformConditional(Conditional cond) { return cond.AcceptTransformer(this, true); }
        protected internal virtual Node TransformConst(Const @const) { return @const.AcceptTransformer(this, true); }
        protected internal virtual Node TransformConvert(Convert cvt) { return cvt.AcceptTransformer(this, true); }
        protected internal virtual Node TransformDefault(Default @default) { return @default.AcceptTransformer(this, true); }
        protected internal virtual Node TransformDeref(Deref deref) { return deref.AcceptTransformer(this, true); }
        protected internal virtual Node TransformFld(Fld fld) { return fld.AcceptTransformer(this, true); }
        protected internal virtual Node TransformLoophole(Loophole loophole) { return loophole.AcceptTransformer(this, true); }
        protected internal virtual Node TransformObjectInit(ObjectInit oi) { return oi.AcceptTransformer(this, true); }
        protected internal virtual Node TransformOperator(Operator op) { return op.AcceptTransformer(this, true); }
        protected internal virtual Node TransformProp(Prop prop) { return prop.AcceptTransformer(this, true); }
        protected internal virtual Node TransformRef(Ref @ref) { return @ref.AcceptTransformer(this, true); }
        protected internal virtual Node TransformSizeof(SizeOf @sizeof) { return @sizeof.AcceptTransformer(this, true); }
        protected internal virtual Node TransformTypeIs(TypeIs typeIs) { return typeIs.AcceptTransformer(this, true); }
        protected internal virtual Node TransformTypeAs(TypeAs typeAs) { return typeAs.AcceptTransformer(this, true); }
        protected internal virtual Node TransformApply(Apply apply) { return apply.AcceptTransformer(this, true); }
        protected internal virtual Node TransformEval(Eval eval) { return eval.AcceptTransformer(this, true); }
        protected internal virtual Node TransformLambda(Lambda lambda) { return lambda.AcceptTransformer(this, true); }
        protected internal virtual Node TransformBlock(Block block) { return block.AcceptTransformer(this, true); }
        protected internal virtual Node TransformBreak(Break @break) { return @break.AcceptTransformer(this, true); }
        protected internal virtual Node TransformCatch(Catch @catch) { return @catch.AcceptTransformer(this, true); }
        protected internal virtual Node TransformClause(Clause clause) { return clause.AcceptTransformer(this, true); }
        protected internal virtual Node TransformContinue(Continue @continue) { return @continue.AcceptTransformer(this, true); }
        protected internal virtual Node TransformGoto(Goto @goto) { return @goto.AcceptTransformer(this, true); }
        protected internal virtual Node TransformFinally(Finally @finally) { return @finally.AcceptTransformer(this, true); }
        protected internal virtual Node TransformIf(If @if) { return @if.AcceptTransformer(this, true); }
        protected internal virtual Node TransformIter(Iter iter) { return iter.AcceptTransformer(this, true); }
        protected internal virtual Node TransformLabel(Label label) { return label.AcceptTransformer(this, true); }
        protected internal virtual Node TransformLoop(Loop loop) { return loop.AcceptTransformer(this, true); }
        protected internal virtual Node TransformReturn(Return @return) { return @return.AcceptTransformer(this, true); }
        protected internal virtual Node TransformThrow(Throw @throw) { return @throw.AcceptTransformer(this, true); }
        protected internal virtual Node TransformTry(Try @try) { return @try.AcceptTransformer(this, true); }
        protected internal virtual Node TransformUsing(Using @using) { return @using.AcceptTransformer(this, true); }
    }
}
