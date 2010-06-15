using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Truesight.Decompiler.Pipeline.Flow.Cfg;
using XenoGears.Functional;
using XenoGears.Assertions;

namespace Truesight.Decompiler.Pipeline.Flow.Scopes
{
    internal class Offspring : ViewOfControlFlowGraph
    {
        public IScope Scope { get; private set; }
        public ControlFlowBlock Root { get; private set; }
        public ControlFlowBlock Head { get; private set; }
        public ReadOnlyCollection<ControlFlowBlock> Body { get; private set; }
        public ReadOnlyCollection<ControlFlowBlock> Pivots { get; private set; }

        public Offspring(IScope neighborScope, ControlFlowBlock root, ControlFlowBlock head)
        {
            Scope = neighborScope;
            Root = root;
            Head = head;

            CalculateBodyAndPivots();
            InitializeControlFlowGraph();
        }

        private void CalculateBodyAndPivots()
        {
            var parentCfg = Scope.Parent.LocalCfg;
            var cflow = new List<ControlFlowBlock>();
            var pivots = new List<ControlFlowBlock>();

            var todo = new HashSet<ControlFlowBlock>{Head};
            while (todo.IsNotEmpty())
            {
                var v = todo.First();
                cflow.Add(v);
                todo.Remove(v);

                var h_pivots = Scope.Hierarchy().SelectMany(s => s.Pivots);
                if (h_pivots.Contains(v))
                {
                    pivots.Add(v);
                }
                else
                {
                    var inEdges = parentCfg.TreeVedges(null, v);
                    var innerEdges = parentCfg.Edges(cflow, cflow);
                    var rootEdge = parentCfg.Vedge(Root, v);
                    inEdges.Except(innerEdges).Except(rootEdge).AssertEmpty();

                    var outEdges = parentCfg.Vedges(v, null);
                    var pending = outEdges.Select(e => e.Target).Where(v1 => !cflow.Contains(v1));
                    pending.ForEach(v1 => todo.Add(v1));
                }
            }

            Body = cflow.Except(pivots).ToReadOnly();
            Pivots = pivots.ToReadOnly();
        }

        private void InitializeControlFlowGraph()
        {
            var vertices = Body.ToHashSet();
            if (vertices.IsEmpty())
            {
                Initialize(Scope.LocalCfg, vertices);
                InheritStartAndFinish(Root, Pivots.AssertSingle());
                AddEigenEdge(new ControlFlowEdge(Start, Finish));
            }
            else
            {
                var finish = Pivots.First();
                Initialize(Scope.LocalCfg, vertices, (e, vcfg) =>
                {
                    var viewVertex = vertices.Contains(e.Source) ? e.Source : e.Target;
                    var alienVertex = vertices.Contains(e.Source) ? e.Target : e.Source;

                    if (alienVertex == e.Source)
                    {
                        (alienVertex == Root).AssertTrue();
                        (viewVertex == Head).AssertTrue();
                        vcfg.InheritStart(alienVertex);
                    }
                    else
                    {
                        Pivots.Contains(alienVertex).AssertTrue();
                        if (alienVertex == finish) vcfg.InheritFinish(alienVertex);
                    }

                    vcfg.AddEigenVertex(alienVertex);
                    vcfg.AddEigenEdge(e.ShallowClone());
                });
            }
        }

        public override String ToString()
        {
            return String.Format("{0} => {1} => {2}, {3}", 
                Root.Name, Head.Name, Pivots.Select(v => v.Name).StringJoin(), base.ToString());
        }
    }
}
