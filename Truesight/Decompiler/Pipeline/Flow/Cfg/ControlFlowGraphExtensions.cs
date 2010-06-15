using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using XenoGears.Functional;

namespace Truesight.Decompiler.Pipeline.Flow.Cfg
{
    internal static class ControlFlowGraphExtensions
    {
        public static ReadOnlyCollection<ControlFlowEdge> AlienEdges(this BaseControlFlowGraph cfg, Func<ControlFlowBlock, bool> vertices)
        {
            return cfg.AlienEdges(cfg.Vertices.Where(vertices));
        }

        public static ReadOnlyCollection<ControlFlowEdge> AlienEdges(this BaseControlFlowGraph cfg, IEnumerable<ControlFlowBlock> vertices)
        {
            var allEdges = cfg.Edges(vertices, null).Concat(cfg.Edges(null, vertices));
            var innerEdges = cfg.Edges(vertices, vertices);
            return allEdges.Except(innerEdges).ToReadOnly();
        }

        public static ReadOnlyCollection<ControlFlowEdge> AlienTreeEdges(this BaseControlFlowGraph cfg, Func<ControlFlowBlock, bool> vertices)
        {
            return cfg.AlienTreeEdges(cfg.Vertices.Where(vertices));
        }

        public static ReadOnlyCollection<ControlFlowEdge> AlienTreeEdges(this BaseControlFlowGraph cfg, IEnumerable<ControlFlowBlock> vertices)
        {
            var allEdges = cfg.TreeEdges(vertices, null).Concat(cfg.TreeEdges(null, vertices));
            var innerEdges = cfg.TreeEdges(vertices, vertices);
            return allEdges.Except(innerEdges).ToReadOnly();
        }

        public static ReadOnlyCollection<ControlFlowEdge> AlienBackEdges(this BaseControlFlowGraph cfg, Func<ControlFlowBlock, bool> vertices)
        {
            return cfg.AlienBackEdges(cfg.Vertices.Where(vertices));
        }

        public static ReadOnlyCollection<ControlFlowEdge> AlienBackEdges(this BaseControlFlowGraph cfg, IEnumerable<ControlFlowBlock> vertices)
        {
            var allEdges = cfg.BackEdges(vertices, null).Concat(cfg.BackEdges(null, vertices));
            var innerEdges = cfg.BackEdges(vertices, vertices);
            return allEdges.Except(innerEdges).ToReadOnly();
        }
    }
}