using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using Truesight.Decompiler.Pipeline.Flow.Cfg;
using XenoGears.Functional;
using XenoGears.Collections;
using Truesight.Parser.Api;
using XenoGears.Assertions;
using Truesight.Parser.Api.Ops;
using CilPredicateType = Truesight.Parser.Api.Ops.PredicateType;
using HirPredicateType = Truesight.Decompiler.Pipeline.Flow.Cfg.PredicateType;

namespace Truesight.Decompiler.Pipeline.Cil
{
    internal static class CreateCarcass
    {
        public static ControlFlowGraph DoCreateCarcass(IMethodBody cil, out ReadOnlyDictionary<ControlFlowBlock, ReadOnlyCollection<IILOp>> blocks2parts)
        {
            // create the control flow graph
            var cfg = new ControlFlowGraph();

            // partition the code into blocks with continuous control flow
            // todo. support switches and protected regions
            var targets = new HashSet<IILOp>(cil.OfType<Branch>().Select(br => br.Target));
            var l_partitions = new List<ReadOnlyCollection<IILOp>>();
            var l_partition = new List<IILOp>();
            Action qualifyPartition = () => { if (l_partition.IsNotEmpty()) { l_partitions.Add(l_partition.ToReadOnly()); l_partition = new List<IILOp>(); } };
            foreach (var op in cil)
            {
                if (op is Branch || op is Ret) qualifyPartition();
                else 
                {
                    if (targets.Contains(op)) qualifyPartition();
                    l_partition.Add(op);
                    if (op is Throw) qualifyPartition();
                }
            }
            qualifyPartition();
            var partitions = l_partitions.ToReadOnly();

            // create blocks and map those to ops and partitions
            blocks2parts = partitions.ToDictionary(p => new ControlFlowBlock(), p => p).ToReadOnly();
            blocks2parts.ForEach(kvp => cfg.AddVertex(kvp.Key));
            var op2blocks = new Dictionary<IILOp, ControlFlowBlock>();
            blocks2parts.ForEach(kvp => kvp.Value.ForEach(op => op2blocks.Add(op, kvp.Key)));
            cil.ForEach(op => { if (!op2blocks.ContainsKey(op)) op2blocks.Add(op, null); });

            // prepare to link the blocks
            Action<IILOp, IILOp, CilPredicateType?> link = (op1, op2, cil_pred) =>
            {
                var source = op1 == null ? cfg.Start : op2blocks[op1];
                var target = op2 == null ? cfg.Finish : op2blocks[op2];
                var hir_pred = cil_pred == null ? (HirPredicateType?)null :
                    (HirPredicateType)Enum.Parse(typeof(HirPredicateType), cil_pred.Value.ToString());
                cfg.AddEdge(new ControlFlowEdge(source, target, hir_pred));
            };

            // link the blocks (down from 300+ LOC to this simple loop =))
            if (cil.IsEmpty()) link(null, null, null);
            foreach (var op in cil)
            {
                // todo. support switches here
                if (op is Switch) throw AssertionHelper.Fail();

                // todo. support general case of control flow
                // n0te. throw needs something on stack, so br > throw is impossible
                Func<IILOp, bool> isJmp = op1 => op1 is Ret || op1 is Branch;
                if (isJmp(op) && isJmp(op.Prev)) continue;

                if (isJmp(op))
                {
                    Func<IILOp, CilPredicateType?> pred = op1 => op1 is Ret ? null : op1 is Branch ? ((Branch)op1).PredicateType : ((Func<CilPredicateType?>)(() => { throw AssertionHelper.Fail(); }))();
                    Func<IILOp, bool> uncond = op1 => isJmp(op1) && pred(op1) == null;
                    Func<IILOp, bool> cond = op1 => isJmp(op1) && pred(op1) != null;
                    Func<IILOp, IILOp> target = null; target = op1 => 
                        op1 is Ret ? null : op1 is Branch ? target(((Branch)op1).Target) : op1;

                    (target(op) is Branch).AssertFalse();
                    if (target(op) is Ret) link(op.Prev, null, pred(op));
                    else link(op.Prev, target(op), pred(op));

                    isJmp(op.Next).AssertImplies(uncond(op.Next));
                    if (cond(op)) link(op.Prev, target(op.Next), pred(op).Negate());
                }
                else if (op is Throw)
                {
                    // do nothing - throw doesn't create links
                }
                else
                {
                    if (op.Prev == null) link(null, op, null);
                    if (isJmp(op.Next)) continue;

                    var blk = op2blocks.GetOrDefault(op);
                    var blk_next = op2blocks.GetOrDefault(op.Next);
                    if (blk != blk_next) link(op, op.Next, null);
                }
            }

            // yield control to the next step of the pipeline
            return cfg;
        }
    }
}
