using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using System.Text;
using NUnit.Framework;
using Truesight.Parser.Impl.Ops;
using Truesight.TextGenerators.Core;
using XenoGears.Functional;
using XenoGears.Reflection.Shortcuts;
using XenoGears.Strings;
using XenoGears.Assertions;
using AssertionHelper=XenoGears.Assertions.AssertionHelper;

namespace Truesight.TextGenerators.Parser
{
    public partial class Generator
    {
        [Test]
        public void GenerateOpCodeReader()
        {
            var buf = new StringBuilder();
            var template = typeof(Generator).Assembly.ReadAllText("Truesight.TextGenerators.Parser.Generator.OpCodeReader.template");

            var lines = template.SplitLines();
            foreach (var line in lines)
            {
                var iof = line.IndexOf("%FILL_MAPS%");
                if (iof != -1)
                {
                    (line.Trim() == "%FILL_MAPS%").AssertTrue();
                    var indent = line.Substring(0, iof);

                    var oneByteOpCodes = new Dictionary<Byte, OpCode>();
                    var twoByteOpCodes = new Dictionary<UInt16, OpCode>();
                    foreach (var opcode in OpCodeReference.AllOpCodes)
                    {
                        if (opcode.Size == 1)
                        {
                            oneByteOpCodes.Add((Byte)(UInt16)opcode.Value, opcode);
                        }
                        else if (opcode.Size == 2)
                        {
                            twoByteOpCodes.Add((UInt16)opcode.Value, opcode);
                        }
                        else
                        {
                            throw AssertionHelper.Fail();
                        }
                    }

                    var opcodeNames = typeof(OpCodes).GetFields(BF.PublicStatic)
                        .ToDictionary(f => (OpCode)f.GetValue(null), f => f.Name);

                    var oneByteOpCodePrefixes = new HashSet<Byte>();
                    var firstByteToOpCode = new Dictionary<Byte, OpCode>();
                    oneByteOpCodes.Keys.ForEach(oneByte =>
                    {
                        buf.AppendLine(indent + "// " + oneByteOpCodes[oneByte].Name);
                        var byte_tos = "0x" + oneByte.ToString("x2");

                        oneByteOpCodePrefixes.Add(oneByte);
                        buf.AppendLine(indent + String.Format("_oneByteOpCodePrefixes.Add("+
                            "{0});", byte_tos));

                        firstByteToOpCode.Add(oneByte, oneByteOpCodes[oneByte]);
                        buf.AppendLine(indent + String.Format("_firstByteToOpCode.Add("+
                            "{0}, OpCodes.{1});", byte_tos, opcodeNames[firstByteToOpCode[oneByte]]));

                        buf.AppendLine();
                    });

                    var twoByteOpCodePrefixes = new HashSet<Byte>();
                    var secondByteToOpCode = new Dictionary<Byte, OpCode>();
                    twoByteOpCodes.Keys.ForEach((twoBytes, i) =>
                    {
                        var firstByte = (Byte)(twoBytes >> 8);
                        var secondByte = (Byte)(twoBytes % (1 << 8));
                        buf.AppendLine(indent + "// " + twoByteOpCodes[twoBytes].Name);
                        var firstByte_tos = "0x" + firstByte.ToString("x2");
                        var secondByte_tos = "0x" + secondByte.ToString("x2");
                        var twoBytes_tos = "0x" + twoBytes.ToString("x4");

                        twoByteOpCodePrefixes.Add(firstByte);
                        buf.AppendLine(indent + String.Format("_twoByteOpCodePrefixes.Add("+
                            "{0});", firstByte_tos));

                        secondByteToOpCode.Add(secondByte, twoByteOpCodes[twoBytes]);
                        buf.AppendLine(indent + String.Format("_secondByteToOpCode.Add("+
                            "{0}, OpCodes.{1});", secondByte_tos, opcodeNames[twoByteOpCodes[twoBytes]]));

                        if (i != twoByteOpCodes.Keys.Count() - 1) buf.AppendLine();
                    });

                    // essential for unambiguous reading!
                    oneByteOpCodePrefixes.Intersect(twoByteOpCodePrefixes).AssertEmpty();

                    // verify consistence
                    (oneByteOpCodePrefixes.Count() == OpCodeReference.AllOpCodes.Where(oc => oc.Size == 1).Count()).AssertTrue();
                    (twoByteOpCodePrefixes.Count() == 1).AssertTrue();
                    (firstByteToOpCode.Count() == OpCodeReference.AllOpCodes.Where(oc => oc.Size == 1).Count()).AssertTrue();
                    (secondByteToOpCode.Count() == OpCodeReference.AllOpCodes.Where(oc => oc.Size == 2).Count()).AssertTrue();
                }
                else
                {
                    buf.AppendLine(line);
                }
            }

            File.WriteAllText(@"..\..\..\Truesight\Parser\Impl\Reader\OpCodeReader.Reference.cs", buf.ToString());
        }
    }
}
