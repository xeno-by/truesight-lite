using System;
using Truesight.Decompiler.Hir;
using Truesight.Decompiler.Pipeline.Attrs;
using Truesight.Decompiler.Pipeline.Flow.Cfg;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Core.Functional;
using Truesight.Decompiler.Hir.TypeInference;
using XenoGears.Assertions;
using XenoGears.Functional;
using Truesight.Decompiler.Hir.Traversal.Traversers;
using System.Linq;

namespace Truesight.Decompiler.Pipeline.Cil
{
    // todo. later think about other cases when we need to preserve Addr/Deref nodes for byrefs
    // as far as I can imagine, all of those involve pointers:
    // 1) assignment to a pointer, 2) assignment from a pointer, 3) passing as a pointer parameter

    [Decompiler(Weight = (int)Stages.PostprocessEarlyHir)]
    internal static class StripOffRedundancies
    {
        [DecompilationStep(Weight = 6)]
        public static void DoStripOffRedundancies(ControlFlowGraph cfg)
        {
            var allNodes = cfg.Vertices.SelectMany(cfb => Seq.Concat(cfb.BalancedCode, cfb.Residue.Cast<Node>()));
            allNodes.ForEach(StripOffRedundanciesInPlace);
        }

        // note. unlike most transformations in Truesight, this one works in-place
        private static void StripOffRedundanciesInPlace(Node root)
        {
            Action<Node> defaultTraverse = node => node.Children.ForEach(c =>
            {
                var deref = c as Deref;
                if (deref != null)
                {
                    var t = deref.Target.InferType();
                    if (t != null && t.IsByRef) c.ReplaceWith(deref.Target);
                }

                c.Traverse();
            });

            root.Traverse(defaultTraverse,
                (Assign ass) =>
                {
                    defaultTraverse(ass);

                    var ass_prop = ass.Lhs as Prop;
                    var ass_app = ass.Lhs as Apply;
                    if (ass_app != null) ass_prop = ass_app.Callee as Prop;
                    if (ass_prop != null)
                    {
                        var addr = ass_prop.This as Addr;
                        if (addr != null) ass_prop.This = addr.Target;
                    }

                    var ass_deref = ass.Lhs as Deref;
                    if (ass_deref != null)
                    {
                        var t = ass_deref.Target.InferType();
                        if (t != null && t.IsByRef) ass.Lhs = ass_deref.Target;
                    }
                },
                (Slot slot) =>
                {
                    var addr = slot.This as Addr;
                    if (addr != null) slot.This = addr.Target;
                    defaultTraverse(slot);
                },
                (Deref deref) =>
                {
                    var t = deref.Target.InferType();
                    if (t != null && t.IsByRef) throw AssertionHelper.Fail();
                    defaultTraverse(deref);
                },
                (Apply app) =>
                {
                    var callee_addr = app.Callee as Addr;
                    if (callee_addr != null) app.Callee = callee_addr.Target;
                    defaultTraverse(app);

                    app.ArgsInfo.Zip((arg, pi, i) =>
                    {
                        var addr = arg as Addr;
                        var arg_byref = addr != null;
                        var p_byref = pi != null && pi.Type.IsByRef;
                        if (arg_byref && p_byref) app.Args[i] = addr.Target;

                        var p_is_this = i == 0 && pi != null && pi.Name == "this";
                        if (arg_byref && p_is_this) app.Args[i] = addr.Target;
                    });
                }
            );
        }
    }
}