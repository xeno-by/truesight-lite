using System;
using System.Linq;
using Truesight.Decompiler.Hir.Core.ControlFlow;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Core.Scopes;
using Truesight.Decompiler.Hir.Core.Symbols;
using Truesight.Decompiler.Hir.Traversal;
using Truesight.Decompiler.Pipeline.Attrs;
using Truesight.Decompiler.Pipeline.Flow.Cfg;
using Truesight.Decompiler.Pipeline.Flow.Scopes;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Traits.Hierarchy;

namespace Truesight.Decompiler.Pipeline.Flow
{
    [Decompiler(Weight = (int)Stages.DecompileScopes)]
    internal static class DecompileScopes
    {
        [DecompilationStep(Weight = 1)]
        public static Block DoDecompileScopes(ControlFlowGraph cfg)
        {
            // todo. do not crash on irregular control flow but rather emit labels/gotos
            return cfg.DecompileScopes();
        }

        [DecompilationStep(Weight = 2)]
        public static void DoInferScopesForLocals(Context ctx)
        {
            var symbols = ctx.Symbols;
            ctx.Body.UsagesOfLocals().ForEach(kvp =>
            {
                var local = kvp.Key;
                if (ctx.Method.IsConstructor && local == symbols.ResolveParam(0))
                {
                    ctx.Body.Locals.Add(local);
                    return;
                }

                var usages = kvp.Value.ToList();
                var scopes = usages.Select(n => n.Scope()).Distinct().ToList();
                foreach (var s in scopes.AsEnumerable().Reverse())
                {
                    Func<Ref, bool> isDefAss = e =>
                    {
                        if (e == null) return false;
                        var ass = e.Parent as Assign;
                        var lhs_ref = ass == null ? null : ass.Lhs as Ref;
                        var lhs_sym = lhs_ref == null ? null : lhs_ref.Sym;
                        return local == lhs_sym;
                    };

                    var indices = usages.IndicesOf(@ref => @ref.Scope().Hierarchy2().Contains(s)).ToReadOnly();
                    if (indices.IsEmpty()) continue;
                    (indices.Count() == indices.Last() - indices.First() + 1).AssertTrue();
                    var i_first = indices.First();
                    var i_last = indices.Last();

                    var first = usages.Nth(i_first);
                    var afterLast = usages.NthOrDefault(i_last + 1);
                    var isCluster = isDefAss(first) && (afterLast == null || isDefAss(afterLast));
                    if (isCluster)
                    {
                        var cl_local = symbols.IntroduceLocal(local.Name, local.Type);
                        s.Locals.Add(cl_local);

                        var u_refs = i_first.UpTo(i_last).Select(i => usages.Nth(i));
                        u_refs.ForEach(u => u.Sym = cl_local);
                        int start = i_first, finish = i_last, cnt = finish - start + 1;
                        usages.RemoveRange(start, cnt);
                    }
                    else
                    {
                        var beforeFirst = indices.First() == 0 ? null : usages.Nth(indices.First() - 1);
                        var stmt = beforeFirst == null ? null : beforeFirst.Stmt();
                        var loop = stmt == null ? null : stmt.Next as Loop;
                        if (loop == s || loop == s.Parent)
                        {
                            isCluster = isDefAss(beforeFirst) && (afterLast == null || isDefAss(afterLast));
                            if (isCluster)
                            {
                                var cl_local = symbols.IntroduceLocal(local.Name, local.Type);
                                loop.Locals.Add(cl_local);

                                var u_refs = (i_first - 1).UpTo(i_last).Select(i => usages.Nth(i));
                                u_refs.ForEach(u => u.Sym = cl_local);
                                int start = i_first - 1, finish = i_last, cnt = finish - start + 1;
                                usages.RemoveRange(start, cnt);

                                loop.Init = new Block();
                                loop.Init.Add(stmt);
                                loop.Parent.Children.Remove(stmt);
                            }
                        }
                    }
                }
            });
        }

    }
}
