using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using NUnit.Framework;
using XenoGears;
using Truesight.TextGenerators.Parser.KnowledgeBase;

namespace Truesight.TextGenerators.Parser
{
    [TestFixture]
    public class Visualizer
    {
        [Test]
        public void PrintOutKnowledgeBase()
        {
            Func<OpCodeFamilyKb, String> typeKeyFromFamily = fkb => fkb.OpCodes
                .Select(oc => oc.OpCodeType)
                .Where(t => t != OpCodeType.Macro)
                .Distinct()
                .Order()
                .StringJoin("/");

            Func<OpCodeFamilyKb, String> flowKeyFromFamily = fkb => fkb.OpCodes
                .Select(oc => oc.FlowControl)
                .Distinct()
                .Order()
                .StringJoin("/");

            var kb = ILOpsKb.Content;
            Trace.WriteLine(kb.Count + " values total:");
            Trace.WriteLine(String.Empty);

            var grouped = kb.GroupBy(fkb => new { TypeKey = typeKeyFromFamily(fkb), FlowKey = flowKeyFromFamily(fkb) });
            foreach (var grouping in grouped)
            {
                var key = grouping.Key;
                Trace.WriteLine(String.Format("[Type = {0}, Flow = {1}] ({2})",
                    key.TypeKey, key.FlowKey, grouping.Count()));

                foreach (var fkb in grouping.OrderBy(fkb => fkb.Name))
                {
                    Trace.Write(fkb.Name + ": ");
                    Trace.WriteLine(fkb.OpCodes.Select(opcode => opcode.Name + (
                        fkb.SubSpecs[opcode].Tags.Contains("Ignore") ? " (ignored)" : "")).StringJoin());
                }

                Trace.WriteLine(String.Empty);
            }
        }
    }
}
