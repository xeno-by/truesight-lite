using System;
using System.Linq;
using Truesight.Decompiler.Hir.Core.ControlFlow;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Core.Functional;
using Truesight.Decompiler.Hir.Core.Symbols;
using Truesight.Decompiler.Hir.Traversal;
using Truesight.Decompiler.Pipeline.Attrs;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Reflection;

namespace Truesight.Decompiler.Pipeline.Hir
{
    [Decompiler(Weight = (int)Stages.PostprocessHir)]
    internal static class InferLoopIters
    {
        [DecompilationStep(Weight = 1)]
        public static void DoInferLoopIters(Block hir)
        {
            foreach (var loop in hir.Family().OfType<Loop>())
            {
                if (loop.Iter.IsNotEmpty())
                {
                    loop.Iter.RemoveElements(n => n == null);
                }
                else
                {
                    if (loop.Init.IsNotEmpty())
                    {
                        Func<Expression, bool> isLvalueOfLool = e =>
                        {
                            if (e == null)
                            {
                                return false;
                            }
                            else if (e is Ref)
                            {
                                var @ref = e.AssertCast<Ref>();
                                var local = @ref == null ? null : @ref.Sym as Local;
                                return local != null && loop.Locals.Contains(local);
                            }
                            else if (e is Slot)
                            {
                                var slot = e.AssertCast<Slot>();
                                var @ref = slot == null ? null : slot.This as Ref;
                                var local = @ref == null ? null : @ref.Sym as Local;
                                return local != null && loop.Locals.Contains(local);
                            }
                            else if (e is Apply)
                            {
                                var app = e.AssertCast<Apply>();
                                var callee = app == null ? null : app.Callee;
                                var prop = callee as Prop;
                                var @ref = prop == null ? null : prop.This as Ref;
                                var local = @ref == null ? null : @ref.Sym as Local;
                                return local != null && loop.Locals.Contains(local);
                            }
                            else
                            {
                                return false;
                            }
                        };

                        var lil = loop.Body.LastOrDefault() as Expression;
                        var lilIsIter = false;
                        if (lil is Assign)
                        {
                            var ass = lil.AssertCast<Assign>();
                            lilIsIter = isLvalueOfLool(ass.Lhs);
                        }
                        else if (lil is Operator)
                        {
                            var op = lil.AssertCast<Operator>();
                            if (op.OperatorType.IsAssign())
                            {
                                lilIsIter = isLvalueOfLool(op.Args.First());
                            }
                        }
                        else if (lil is Eval)
                        {
                            var m = lil.InvokedMethod();
                            if (m != null)
                            {
                                var args = lil.InvocationArgs();
                                if (m.IsInstance() || m.IsExtension())
                                {
                                    lilIsIter = isLvalueOfLool(args.FirstOrDefault());
                                }
                                else
                                {
                                    lilIsIter = args.Any(isLvalueOfLool);
                                }
                            }
                        }

                        if (lilIsIter)
                        {
                            loop.Iter = new Block();
                            loop.Iter.Add(lil);
                            loop.Body.Remove(lil);
                        }
                    }
                }
            }
        }
    }
}