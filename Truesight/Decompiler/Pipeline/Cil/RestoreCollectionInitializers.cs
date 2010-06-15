using System;
using System.Collections.Generic;
using System.Linq;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Core.Functional;
using Truesight.Decompiler.Hir.Traversal;
using Truesight.Decompiler.Pipeline.Attrs;
using Truesight.Decompiler.Pipeline.Cil.Common;
using Truesight.Decompiler.Pipeline.Flow.Cfg;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Traits.Equivatable;

namespace Truesight.Decompiler.Pipeline.Cil
{
    [Decompiler(Weight = (int)Stages.PostprocessEarlyHir)]
    internal static class RestoreCollectionInitializers
    {
        // todo. c#3.0-style collection initializers are to be implemented
        [DecompilationStep(Weight = 2)]
        public static void DoRestoreCollectionInitializers(ControlFlowGraph cfg)
        {
            // here we transform "new T[0]" expressions into collection initializers
            var allStmts = cfg.Vertices.SelectMany(cfb => cfb.BalancedCode).ToReadOnly();
            var allNodes = allStmts.SelectMany(s => s.Family()).ToReadOnly();
            var emptyArrayCtors = allNodes.OfType<Eval>().Where(eval =>
            {
                var ctor = eval.InvokedCtor();
                if (ctor == null || !ctor.DeclaringType.IsArray) return false;

                var app = eval == null ? null : eval.Callee;
                var arrlen = app == null ? null : app.Args.SingleOrDefault2() as Const;
                if (arrlen == null || (!(arrlen.Value is int) && !(arrlen.Value is long))) return false;
                var i_arrlen = arrlen.Value.AssertCoerce<long>();
                return i_arrlen == 0;
            }).ToReadOnly();
            emptyArrayCtors.ForEach(eac => eac.Parent.ReplaceRecursive(eac, new CollectionInit(eac)));

            // now we proceed to decompile stuff like "var a = new T[n]; a[0] = foo0; ... a[n-1] = foo;"
            var arrayCtors = allStmts.OfType<Assign>().Where(ass =>
            {
                if (!(ass.Rhs is Eval)) return false;
                var ctor = ass.Rhs.InvokedCtor();
                return ctor == null ? false : ctor.DeclaringType.IsArray;
            }).ToReadOnly();

            // todo #1. this needs to be like RestoreOpAssignOperators
            // to be capable of restoring recursive collection initializers
            // todo #2. what about multidimensional initializers for jagges and/or rects?
            // for the latter t0do n0te that for dimensions higher than 3 arrays of objects define
            // GetValue(params) and SetValue(params) methods => this won't work correctly
            foreach (var ctor in arrayCtors)
            {
                var cfb = cfg.Vertices.Single(cfb1 => cfb1.BalancedCode.Contains(ctor));

                var map = new Dictionary<long, Expression>();
                var inits = cfb.BalancedCode.SkipWhile(n => n != ctor).Skip(1).TakeWhile(n =>
                {
                    var m = n.InvokedMethod();
                    if (m != null && ((m.DeclaringType == typeof(Array) && m.Name == "SetValue") ||
                        (m.DeclaringType.IsArray && m.Name == "Set")))
                    {
                        var args = n.InvocationArgs().AssertNotNull();
                        if (args.Count() != 3) return false;
                        var arrayRef = args.First();
                        if (!arrayRef.Equiv(ctor.Lhs)) return false;

                        var value = args.Third() as Expression;
                        if (value == null) return false;
                        var index = args.Second() as Const;
                        if (index == null || (!(index.Value is int) && !(index.Value is long))) return false;

                        var i_index = index.Value.AssertCoerce<long>();
                        map.Add(i_index, value);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }).ToReadOnly();

                var arrlen = ctor.Rhs.InvocationArgs().SingleOrDefault2() as Const;
                if (arrlen == null || (!(arrlen.Value is int) && !(arrlen.Value is long))) continue;
                var i_arrlen = arrlen.Value.AssertCoerce<long>();
                if (map.Count() != i_arrlen) continue;
                if (!Set.Equal(0L.Unfold(i => i + 1, i => i < i_arrlen), map.Keys)) continue;

                var elements = map.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToReadOnly();
                var arrayInit = new CollectionInit(ctor.Rhs.AssertCast<Eval>(), elements);
                inits.ForEach(init => cfg.Remove(init));
                ctor.Rhs = arrayInit;
            }
        }
    }
}