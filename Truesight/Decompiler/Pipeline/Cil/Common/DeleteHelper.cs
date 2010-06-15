using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Truesight.Decompiler.Hir;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Pipeline.Flow.Cfg;
using XenoGears.Functional;

namespace Truesight.Decompiler.Pipeline.Cil.Common
{
    [DebuggerNonUserCode]
    internal static class DeleteHelper
    {
        public static void Remove(this ControlFlowGraph cfg, params Node[] toDelete)
        {
            cfg.Remove((IEnumerable<Node>)toDelete);
        }

        public static void Remove(this ControlFlowGraph cfg, IEnumerable<Node> toDelete)
        {
            toDelete.ForEach(n => cfg.Remove(n));
        }

        public static void Remove(this ControlFlowGraph cfg, Node toDelete)
        {
            var parent = cfg.Vertices.SingleOrDefault(cfb =>
                cfb.BalancedCode.Contains(toDelete) || cfb.Residue.Contains(toDelete as Expression));
            if (parent != null)
            {
                parent.BalancedCode.Remove(toDelete);
                parent.Residue.Remove(toDelete as Expression);
            }
        }

        public static void Replace(this ControlFlowGraph cfg, Node replacee, Node replacer)
        {
            var parent = cfg.Vertices.SingleOrDefault(cfb =>
                cfb.BalancedCode.Contains(replacee) || cfb.Residue.Contains(replacee as Expression));
            if (parent != null)
            {
                parent.BalancedCode.ReplaceElements(replacee, replacer);
                parent.Residue.ReplaceElements(replacee as Expression, replacer as Expression);
            }
        }
    }
}