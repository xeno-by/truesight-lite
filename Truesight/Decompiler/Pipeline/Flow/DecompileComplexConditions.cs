using System;
using System.Linq;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Pipeline.Attrs;
using Truesight.Decompiler.Pipeline.Flow.Cfg;
using XenoGears.Functional;
using XenoGears.Assertions;

namespace Truesight.Decompiler.Pipeline.Flow
{
    [Decompiler(Weight = (int)Stages.PostprocessCfg)]
    internal static class DecompileComplexConditions
    {
        [DecompilationStep(Weight = 3)]
        public static void DoDecompileComplexConditions(ControlFlowGraph cfg)
        {
            var flow = cfg.Cflow(cfg.Start);
            while (true)
            {
                var vs = flow.FirstOrDefault(v =>
                    v.Residue.Count() == 1 &&
                    cfg.Vedges(v, null).Count() == 2 &&
                    cfg.Vedges(v, null).All(e => e.Target.BalancedCode.IsEmpty() && e.Target.Residue.Count() == 1));
                if (vs == null) break;

                var conv = cfg.ConvStrict(vs);
                var ass = conv.BalancedCode.AssertFirst().AssertCast<Assign>();
                (ass.Lhs is Ref && ass.Rhs is Loophole).AssertTrue();

                var parts = cfg.Cflow(vs, conv);
                var innerEdges = cfg.Edges(parts, parts).ToList();
                cfg.Edges(null, parts).Except(innerEdges).AssertEach(e => e.Target == vs);
                cfg.Edges(parts, null).Except(innerEdges).AssertEach(e => e.Source == vs || e.Source == conv);

                while (true)
                {
                    var somethingWasChanged = false;
                    foreach (var pivot in parts)
                    {
                        var pivot_inEdges = cfg.Vedges(null, pivot);
                        if (pivot_inEdges.Count() != 1) continue;

                        var e_pred2pivot = pivot_inEdges.AssertSingle();
                        var pred = e_pred2pivot.Source;
                        var pred_outEdges = cfg.Vedges(pred, null);
                        if (pred_outEdges.Count() != 2) continue;

                        var e_pred2target1 = pred_outEdges.AssertSingle(e => e.Target != pivot);
                        var target1 = e_pred2target1.Target;
                        var e_pivot2target1 = cfg.Vedge(pivot, target1);
                        if (e_pivot2target1 == null) continue;

                        var pivot_outEdges = cfg.Vedges(pivot, null);
                        if (pivot_outEdges.Count() != 2) continue;
                        var e_pivot2target2 = pivot_outEdges.AssertSingle(e => e.Target != target1);
                        var target2 = e_pivot2target2.Target;

                        var @operator = e_pred2target1.Condition == PredicateType.IsTrue ? OperatorType.OrElse :
                            e_pred2target1.Condition == PredicateType.IsFalse ? OperatorType.AndAlso :
                            ((Func<OperatorType>)(() => { throw AssertionHelper.Fail(); }))();
                        var clause_left = pred.Residue.AssertSingle();
                        var clause_right = pivot.Residue.AssertSingle();
                        var negate_rhs = e_pred2target1.Condition != e_pivot2target1.Condition;
                        if (negate_rhs) clause_right = Operator.Not(clause_right);
                        var junction = Operator.Create(@operator, clause_left, clause_right);

                        cfg.RemoveVertex(pivot);
                        cfg.AddEdge(new ControlFlowEdge(pred, target2, e_pred2target1.Condition.Negate()));
                        pred.Residue.SetElements(junction);
                        somethingWasChanged |= true;
                    }

                    if (!somethingWasChanged) break;
                }

                parts = cfg.Cflow(vs, conv);
                (parts.Count() == 4).AssertTrue();
                var @const = parts.Except(vs, conv).AssertSingle(v => v.Residue.AssertSingle() is Const);
                var vnext = parts.Except(vs, conv, @const).AssertSingle();
                (cfg.Vedge(@const, vnext) == null && cfg.Vedge(vnext, @const) == null).AssertTrue();
                cfg.Vedge(vs, vnext).IsConditional.AssertTrue();
                cfg.Vedge(vs, @const).IsConditional.AssertTrue();
                cfg.Vedge(vnext, conv).IsUnconditional.AssertTrue();
                cfg.Vedge(@const, conv).IsUnconditional.AssertTrue();

                var estart = vs.Residue.AssertSingle();
                var enext = vnext.Residue.AssertSingle();
                var cond_const = @const.Residue.AssertSingle().AssertCast<Const>().Value.AssertCast<int>();
                var cond_edge = cfg.Vedge(vs, @const).Condition;
                var val_const = cond_const == 1 ? true :
                    cond_const == 0 ? false :
                    ((Func<bool>)(() => { throw AssertionHelper.Fail(); }))();
                var val_edge = cond_edge == PredicateType.IsTrue ? true :
                    cond_edge == PredicateType.IsFalse ? false :
                    ((Func<bool>)(() => { throw AssertionHelper.Fail(); }))();

                var operator1 = val_const ? OperatorType.OrElse : OperatorType.AndAlso;
                var clause_left1 = val_edge && val_const ? estart : Operator.Not(estart);
                var clause_right1 = !val_edge && !val_const ? Operator.Not(enext) : enext;
                var junction1 = Operator.Create(operator1, clause_left1, clause_right1);

                var conv_outEdges = cfg.Vedges(conv, null);
                var conv_inEdges = cfg.Vedges(null, conv).Except(cfg.Vedges(parts, conv));
                cfg.RemoveVertices(@const, vnext, conv);
                conv_outEdges.ForEach(e => cfg.AddEdge(new ControlFlowEdge(vs, e.Target, e.Tag)));
                conv_inEdges.ForEach(e => cfg.AddEdge(new ControlFlowEdge(e.Source, vs, e.Tag)));

                vs.BalancedCode.Add(new Assign(ass.Lhs, junction1));
                vs.BalancedCode.AddElements(conv.BalancedCode.Skip(1));
                vs.Residue.SetElements(conv.Residue);
            }

            cfg.Edges().AssertEach(e => e.Tag.Arity() <= 1);
            cfg.Vertices.Where(v => v.Residue.IsNotEmpty()).AssertEach(v => v.Residue.Count() == 1);
            cfg.Vertices.AssertNone(v => v.Residue.IsNotEmpty() && 
                cfg.Vedges(v, null).Any(e => e.IsUnconditional && e.Target != cfg.Finish));
        }
    }
}