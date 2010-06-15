using System.Collections.ObjectModel;
using Truesight.Decompiler.Pipeline.Attrs;
using Truesight.Decompiler.Pipeline.Flow.Cfg;
using XenoGears.Collections;
using Truesight.Parser.Api;

namespace Truesight.Decompiler.Pipeline.Cil
{
    [Decompiler(Weight = (int)Stages.BuildEarlyHirAndCfg)]
    internal static class BuildControlFlowGraph
    {
        [DecompilationStep(Weight = 1)]
        public static ControlFlowGraph DoBuildControlFlowGraph(IMethodBody cil, Symbols symbols)
        {
            ReadOnlyDictionary<ControlFlowBlock, ReadOnlyCollection<IILOp>> blocks2parts;
            var cfg = CreateCarcass.DoCreateCarcass(cil, out blocks2parts);
            foreach (var cfb in blocks2parts.Keys)
            {
                InitialDecompilation.DoPrimaryDecompilation(cfb, blocks2parts[cfb], symbols);
            }

            return cfg;
        }
    }
}