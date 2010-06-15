using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Truesight.Decompiler.Hir;
using Truesight.Decompiler.Hir.Core.Expressions;
using XenoGears.Functional;
using XenoGears.Collections.Observable;
using XenoGears.Traits.Hierarchy;
using XenoGears.Traits.Dumpable;

namespace Truesight.Decompiler.Pipeline.Flow.Cfg
{
    [DumpFormat(NullObjectFormat = "", DefaultExtension = "cfb")]
    [DebuggerDisplay("{ToString(), nq}")]
    internal class ControlFlowBlock : BaseNamedEntity, IDumpableAsText
    {
        private readonly ObservableList<Node> _balancedCode = new ObservableList<Node>();
        public IObservableList<Node> BalancedCode { get { return _balancedCode; } }

        private readonly ObservableList<Expression> _residue = new ObservableList<Expression>();
        public IObservableList<Expression> Residue { get { return _residue; } }

        public new ControlFlowBlock SetName(String name) { return (ControlFlowBlock)base.SetName(name); }
        public new ControlFlowBlock SetName(Func<String> name) { return (ControlFlowBlock)base.SetName(name); }

        public int Incoming
        {
            get
            {
                var all = BalancedCode.Cast<Node>().Concat(Residue.Cast<Node>());
                var loopholes = all.SelectMany(s => s.Family()).OfType<Loophole>();
                return loopholes.Distinct(l => l.ProtoId).Count();
            }
        }

        public int Outgoing
        {
            get
            {
                return Residue.Count();
            }
        }

        public override String ToString() { return this.DumpAsText(); }
        void IDumpableAsText.DumpAsText(TextWriter writer)
        {
            var balancedCode = BalancedCode.IsEmpty() ? null : BalancedCode.Select(n => n.DumpAsText()).StringJoin(";" + Environment.NewLine);
            if (balancedCode != null && balancedCode.EndsWith(Environment.NewLine))
                balancedCode = balancedCode.Substring(0, balancedCode.Length - Environment.NewLine.Length);
            var residue = Residue.IsEmpty() ? null : Residue.Select(e => "r: " + e.DumpAsText()).StringJoin(Environment.NewLine);
            var dump = new[] { Name, balancedCode, residue }.Where(s => s.IsNeitherNullNorEmpty()).StringJoin(Environment.NewLine);
            writer.Write(dump);
        }
    }
}