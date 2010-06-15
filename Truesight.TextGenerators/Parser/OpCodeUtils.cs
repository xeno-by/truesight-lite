using System;
using System.Linq;
using System.Reflection.Emit;
using XenoGears.Assertions;
using XenoGears.Reflection.Shortcuts;
using XenoGears.Strings;

namespace Truesight.TextGenerators.Parser
{
    public static class OpCodeUtils
    {
        public static String GetCSharpDeclaration(this OpCode opcode)
        {
            var fieldName = typeof(OpCodes)
                .GetFields(BF.PublicStatic)
                .ToDictionary(f => f.GetValue(null).AssertCast<OpCode>(), f => f)
                .Single(kvp => kvp.Key.Value == opcode.Value)
                .Value.Name;
            return typeof(OpCodes).GetCSharpRef(ToCSharpOptions.ForCodegen) + "." + fieldName;
        }

        public static String GetCSharpByteSequence(this OpCode opcode)
        {
            if (opcode.Size == 1)
            {
                return "0x" + (((UInt16)opcode.Value) % 0x100).ToString("x2");
            }
            else if (opcode.Size == 2)
            {
                return "0x" + ((UInt16)opcode.Value).ToString("x4");
            }
            else
            {
                throw AssertionHelper.Fail();
            }
        }
    }
}
