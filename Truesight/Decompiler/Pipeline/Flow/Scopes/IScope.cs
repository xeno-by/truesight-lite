using System;
using System.Collections.ObjectModel;
using Truesight.Decompiler.Hir;
using Truesight.Decompiler.Pipeline.Flow.Cfg;

namespace Truesight.Decompiler.Pipeline.Flow.Scopes
{
    internal interface IScope
    {
        String Uri { get; }
        IScope Parent { get; }

        Node Hir { get; }

        BaseControlFlowGraph LocalCfg { get; }
        ReadOnlyCollection<ControlFlowBlock> Pivots { get; }
        ReadOnlyCollection<Offspring> Offsprings { get; }
    }

    internal interface IScope<T> : IScope
        where T : Node
    {
        new T Hir { get; }
    }
}