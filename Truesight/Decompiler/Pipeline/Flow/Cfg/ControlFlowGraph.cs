using System;
using System.Collections.Generic;
using System.Linq;
using XenoGears.Assertions;
using XenoGears.Strings;
using XenoGears.Functional;

namespace Truesight.Decompiler.Pipeline.Flow.Cfg
{
    internal class ControlFlowGraph : BaseControlFlowGraph
    {
        internal int _allTimeVertexCounter = 0;
        private List<ControlFlowBlock> __vertices = new List<ControlFlowBlock>();
        private List<ControlFlowEdge> __edges = new List<ControlFlowEdge>();
        protected override IList<ControlFlowBlock> _vertices { get { return __vertices; } }
        protected override IList<ControlFlowEdge> _edges { get { return __edges; } }

        private readonly ControlFlowBlock _start;
        public override ControlFlowBlock Start { get { return _start; } }
        private readonly ControlFlowBlock _finish;
        public override ControlFlowBlock Finish { get { return _finish; } }

        public new ControlFlowGraph SetName(String name) { return (ControlFlowGraph)base.SetName(name); }
        public new ControlFlowGraph SetName(Func<String> name) { return (ControlFlowGraph)base.SetName(name); }

        public ControlFlowGraph()
        {
            HookUpVertexPostprocessors();
            AddVertex(_start = new ControlFlowBlock());
            AddVertex(_finish = new ControlFlowBlock());
        }

        public ControlFlowGraph(BaseControlFlowGraph proto, bool deep)
            : base(proto, deep)
        {
            _start = proto.Start;
            _finish = proto.Finish;

            _allTimeVertexCounter = __vertices.Count();
            if (_start == null) _allTimeVertexCounter++;
            if (_finish == null) _allTimeVertexCounter++;
            HookUpVertexPostprocessors();
        }

        private void HookUpVertexPostprocessors()
        {
            VertexAdded += v =>
            {
                if (v.Name.IsNullOrEmpty())
                {
                    var index = _allTimeVertexCounter++;
                    if (index == 0)
                    {
                        v.SetName("start");
                    }
                    else if (index == 1)
                    {
                        v.SetName("finish");
                    }
                    else
                    {
                        v.SetName((index - 2).SZtoAAA());
                    }
                }
            };

            VertexRemoved += e => (e != _start && e != _finish).AssertTrue();
        }

        public override String ToString()
        {
            return String.Format("genuine {0}", base.ToString());
        }
    }
}
