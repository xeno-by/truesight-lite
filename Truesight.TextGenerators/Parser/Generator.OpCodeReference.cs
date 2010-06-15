using System;
using System.IO;
using System.Reflection.Emit;
using System.Text;
using NUnit.Framework;
using Truesight.TextGenerators.Core;
using XenoGears.Reflection.Shortcuts;
using XenoGears.Strings;
using XenoGears.Assertions;

namespace Truesight.TextGenerators.Parser
{
    public partial class Generator
    {
        [Test]
        public void GenerateOpCodeReference()
        {
            var buf = new StringBuilder();
            var template = typeof(Generator).Assembly.ReadAllText("Truesight.TextGenerators.Parser.Generator.OpCodeReference.template");

            var lines = template.SplitLines();
            foreach (var line in lines)
            {
                var iof = line.IndexOf("%FILL_THE_HASHSET%");
                if (iof != -1)
                {
                    (line.Trim() == "%FILL_THE_HASHSET%").AssertTrue();

                    foreach (var f_opcode in typeof(OpCodes).GetFields(BF.PublicStatic))
                    {
                        var opcode = f_opcode.GetValue(null).AssertCast<OpCode>();
                        if (opcode.OpCodeType == OpCodeType.Nternal) continue;

                        buf.Append(line.Substring(0, iof));
                        buf.AppendLine(String.Format("_allOpCodes.Add(OpCodes.{0});", f_opcode.Name));
                    }
                }
                else
                {
                    buf.AppendLine(line);
                }
            }

            File.WriteAllText(@"..\..\..\Truesight\Parser\Impl\Ops\OpCodeReference.cs", buf.ToString());
        }
    }
}
