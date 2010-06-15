using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Truesight.Decompiler.Hir;
using Truesight.Decompiler.Hir.Core.ControlFlow;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Core.Functional;
using XenoGears.Functional;
using XenoGears.Assertions;
using Convert=Truesight.Decompiler.Hir.Core.Expressions.Convert;

namespace Truesight.Decompiler.Pipeline.Cil
{
    internal static class CSharpEvaluationOrderHelper
    {
        public static ReadOnlyCollection<Node> CSharpEvaluationOrder(this Node n)
        {
            var eo = new List<Node>();
            Traverse(n, eo);
            return eo.ToReadOnly();
        }

        private static void Traverse(Node n, List<Node> log)
        {
            // this is a child-first traversal
            if (n == null)
            {
                // do nothing - nowhere to drill into
            }
            else if (n is Addr)
            {
                var addr = (Addr)n;
                Traverse(addr.Target, log);
            }
            else if (n is Assign)
            {
                var ass = (Assign)n;
                Traverse(ass.Rhs, log);
                Traverse(ass.Lhs, log);
            }
            else if (n is Operator)
            {
                var op = (Operator)n;
                op.Args.ForEach(a => Traverse(a, log));
            }
            else if (n is Conditional)
            {
                var cond = (Conditional)n;
                Traverse(cond.Test, log);
                Traverse(cond.IfTrue, log);
                Traverse(cond.IfFalse, log);
            }
            else if (n is Const)
            {
                // do nothing - nowhere to drill into
            }
            else if (n is Convert)
            {
                var cvt = (Convert)n;
                Traverse(cvt.Source, log);
            }
            else if (n is Deref)
            {
                var deref = (Deref)n;
                Traverse(deref.Target, log);
            }
            else if (n is Slot)
            {
                var slot = (Slot)n;
                Traverse(slot.This, log);
            }
            else if (n is Loophole)
            {
                // do nothing - nowhere to drill into
            }
            else if (n is Ref)
            {
                // do nothing - nowhere to drill into
            }
            else if (n is SizeOf)
            {
                // do nothing - nowhere to drill into
            }
            else if (n is TypeAs)
            {
                var typeAs = (TypeAs)n;
                Traverse(typeAs.Target, log);
            }
            else if (n is TypeIs)
            {
                var typeIs = (TypeIs)n;
                Traverse(typeIs.Target, log);
            }
            else if (n is CollectionInit)
            {
                var ci = (CollectionInit)n;
                ci.Elements.ForEach(el => Traverse(el, log));
                Traverse(ci.Ctor, log);
            }
            else if (n is ObjectInit)
            {
                var oi = (ObjectInit)n;
                oi.Members.ForEach(mi => Traverse(oi.MemberInits[mi], log));
                Traverse(oi.Ctor, log);
            }
            else if (n is Apply)
            {
                var app = (Apply)n;
                app.Args.ForEach(a => Traverse(a, log));
                Traverse(app.Callee, log);
            }
            else if (n is Eval)
            {
                var eval = (Eval)n;
                Traverse(eval.Callee, log);
            }
            else if (n is Lambda)
            {
                // do nothing - nowhere to drill into
            }
            else if (n is Catch)
            {
                var @catch = (Catch)n;
                Traverse(@catch.Filter, log);
                @catch.ForEach(c => Traverse(c, log));
            }
            else if (n is Block)
            {
                var block = (Block)n;
                block.ForEach(c => Traverse(c, log));
            }
            else if (n is If)
            {
                var @if = (If)n;
                Traverse(@if.Test, log);
                Traverse(@if.IfTrue, log);
                Traverse(@if.IfFalse, log);
            }
            else if (n is Loop)
            {
                var loop = (Loop)n;
                Traverse(loop.Init, log);
                if (loop.IsWhileDo) Traverse(loop.Test, log);
                Traverse(loop.Body, log);
                Traverse(loop.Iter, log);
                if (loop.IsDoWhile) Traverse(loop.Test, log);
            }
            else if (n is Break)
            {
                // do nothing - nowhere to drill into
            }
            else if (n is Continue)
            {
                // do nothing - nowhere to drill into
            }
            else if (n is Goto)
            {
                // todo. I don't really have time to implement this right now
                // neither I can think about the case in the near future when this will be useful
//                throw new NotImplementedException();
            }
            else if (n is Label)
            {
                // do nothing - nowhere to drill into
            }
            else if (n is Return)
            {
                var @return = (Return)n;
                Traverse(@return.Value, log);
            }
            else if (n is Throw)
            {
                var @throw = (Throw)n;
                Traverse(@throw.Exception, log);
            }
            else if (n is Try)
            {
                var @try = (Try)n;
                Traverse(@try.Body, log);
                @try.Clauses.ForEach(c => Traverse(c, log));
            }
            else if (n is Using)
            {
                var @using = (Using)n;
                Traverse(@using.Init, log);
                Traverse(@using.Body, log);
            }
            else if (n is Iter)
            {
                var iter = (Iter)n;
                Traverse(iter.Seq, log);
                Traverse(iter.Body, log);
            }
            else
            {
                throw AssertionHelper.Fail();
            }

            // this is a child-first traversal
            log.Add(n);
        }
    }
}