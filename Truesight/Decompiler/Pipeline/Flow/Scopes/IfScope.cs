using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Truesight.Decompiler.Hir;
using Truesight.Decompiler.Hir.Core.ControlFlow;
using Truesight.Decompiler.Pipeline.Flow.Cfg;
using XenoGears.Functional;
using XenoGears.Assertions;

namespace Truesight.Decompiler.Pipeline.Flow.Scopes
{
    internal class IfScope : IScope<If>
    {
        private readonly BlockScope _parent;
        public IScope Parent { get { return _parent; } }

        Node IScope.Hir { get { return Hir; } }
        public If Hir { get { return _if; } }

        public BaseControlFlowGraph LocalCfg { get { return _parent.LocalCfg; } }
        public BaseControlFlowGraph IfTrue { get { return _ifTrue; } }
        public BaseControlFlowGraph IfFalse { get { return _ifFalse; } }
        public ReadOnlyCollection<Offspring> Offsprings { get { return _trueOffsprings.Concat(_falseOffsprings).ToReadOnly(); } }

        public ControlFlowBlock Test { get { return _head; } }
        public ControlFlowBlock Conv { get { return _conv; } }
        public ReadOnlyCollection<ControlFlowBlock> Pivots { get { return Seq.Empty<ControlFlowBlock>().ToReadOnly(); } }

        private readonly If _if = new If();
        private readonly ControlFlowBlock _head;
        private ControlFlowBlock _conv { get { return LocalCfg.ConvNearest(_head).AssertNotNull(); } }
        private readonly BaseControlFlowGraph _ifTrue;
        private readonly ReadOnlyCollection<Offspring> _trueOffsprings;
        private readonly BaseControlFlowGraph _ifFalse;
        private readonly ReadOnlyCollection<Offspring> _falseOffsprings;

        public static IfScope Decompile(BlockScope parent, ControlFlowBlock head) { return new IfScope(parent, head); }
        private IfScope(BlockScope parent, ControlFlowBlock head)
        {
            _parent = parent.AssertNotNull();
            _head = head.AssertNotNull();

            parent.Hir.AddElements(head.BalancedCode);
            _if.Test = head.Residue.AssertSingle();

            var v_true = LocalCfg.TreeVedges(head, null).AssertSingle(e => e.Tag == PredicateType.IsTrue).Target;
            _ifTrue = this.InferBranch(v_true, out _trueOffsprings);
            var v_false = LocalCfg.TreeVedges(head, null).AssertSingle(e => e.Tag == PredicateType.IsFalse).Target;
            _ifFalse = this.InferBranch(v_false, out _falseOffsprings);

            _if.IfTrue = BlockScope.Decompile(this, _ifTrue).Hir;
            _if.IfFalse = BlockScope.Decompile(this, _ifFalse).Hir;
        }

        public ViewOfControlFlowGraph InferBranch(ControlFlowBlock headOfBranch, out ReadOnlyCollection<Offspring> offsprings_out)
        {
            var cfg = LocalCfg;

            var cflow = cfg.Cflow(headOfBranch, _conv).Except(_conv);
            var wannabes = cfg.Cflow(headOfBranch).Except(cfg.Cflow(_conv));
            var closure = cflow.Closure(wannabes, (vin, vout) => vout != _head && cfg.Vedge(vout, vin) != null);
            var vertices = closure.Except(_conv).OrderBy(v => cfg.Cflow().IndexOf(v)).ToReadOnly();

            if (vertices.IsEmpty())
            {
                var branch = cfg.CreateView(vertices).CreateEigenStartAndFinish();
                branch.AddEigenEdge(new ControlFlowEdge(branch.Start, branch.Finish));

                offsprings_out = Seq.Empty<Offspring>().ToReadOnly();
                return branch;
            }
            else
            {
                var offsprings = new List<Offspring>();
                var branch = cfg.CreateView(vertices, (e, vcfg) =>
                {
                    if (vertices.Contains(e.Target))
                    {
                        (e.Source == _head && e.Target == headOfBranch && e.IsConditional).AssertTrue();
                        vcfg.AddEigenEdge(new ControlFlowEdge(vcfg.Start, e.Target));
                    }
                    else
                    {
                        if (e.Target == _conv)
                        {
                            vcfg.AddEigenEdge(new ControlFlowEdge(e.Source, vcfg.Finish, e.Tag));
                        }
                        else
                        {
                            (cfg.Vedges(e.Source, null).Count() == 2).AssertTrue();
                            offsprings.Add(new Offspring(this, e.Source, e.Target));
                        }
                    }
                });
                (branch.Start != null && branch.Finish != null).AssertTrue();

                offsprings_out = offsprings.ToReadOnly();
                return branch;
            }
        }

        public String Uri
        {
            get
            {
                var self = String.Format("if ({0} => {1})", _head.Name, _conv.Name);
                return Parent.Uri + " :: " + self;
            }
        }

        public override String ToString()
        {
            var lite = Uri.Replace("complex :: ", "");
            lite = lite.Replace("body :: ", "");
            return "{" + lite + "}";
        }
    }
}
