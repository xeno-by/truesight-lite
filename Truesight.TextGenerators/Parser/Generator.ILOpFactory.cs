using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Truesight.Parser.Impl;
using Truesight.Parser.Impl.Ops;
using Truesight.TextGenerators.Core;
using XenoGears.Assertions;
using XenoGears.Reflection.Attributes;
using XenoGears.Strings;

namespace Truesight.TextGenerators.Parser
{
    public partial class Generator
    {
        [Test]
        public void GenerateILOpFactory()
        {
            var buf = new StringBuilder();
            var template = typeof(Generator).Assembly.ReadAllText("Truesight.TextGenerators.Parser.Generator.ILOpFactory.template");

            var lines = template.SplitLines();
            foreach (var line in lines)
            {
                var iof = line.IndexOf("%FILL_THE_SWITCH%");
                if (iof != -1)
                {
                    (line.Trim() == "%FILL_THE_SWITCH%").AssertTrue();
                    var indent = line.Substring(0, iof);
                    buf.AppendLine(indent + "switch ((ushort)opcode.Value)");
                    buf.AppendLine(indent + "{");
                    indent += "    ";

                    foreach (var t_ilop in typeof(ILOp).Assembly.GetTypes()
                        .Where(t => typeof(ILOp) != t && typeof(ILOp).IsAssignableFrom(t)))
                    {
                        foreach (var opcode in t_ilop.Attr<OpCodesAttribute>().OpCodes)
                        {
                            if (opcode.Value >= 0) buf.AppendLine(indent + "case 0x" + opcode.Value.ToString("x2") + ": // " + opcode.Name);
                            else buf.AppendLine(indent + "case 0x" + opcode.Value.ToString("x4") + ": // " + opcode.Name);
                            buf.AppendLine(indent + String.Format(
                                "    return new {0}(body, reader, prefixes);", t_ilop.Name));
                        }
                    }

                    buf.AppendLine(indent + "default: ");
                    buf.AppendLine(indent + "    throw new NotSupportedException(String.Format("+
                        "\"Opcode \\\"0x{0:x4}\\\" is not supported\", opcode.Value));");
                    buf.AppendLine(indent.Substring(4) + "}");
                }
                else
                {
                    buf.AppendLine(line);
                }
            }

            File.WriteAllText(@"..\..\..\Truesight\Parser\Impl\Reader\ILOpFactory.cs", buf.ToString());
        }
    }
}
