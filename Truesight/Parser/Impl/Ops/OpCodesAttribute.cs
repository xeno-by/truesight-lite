using System;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Linq;
using XenoGears.Assertions;
using Truesight.Parser.Impl.Reader;

namespace Truesight.Parser.Impl.Ops
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    [DebuggerNonUserCode]
    internal class OpCodesAttribute : Attribute
    {
        public OpCode[] OpCodes { get; private set; }

        public OpCodesAttribute(params UInt16[] opCodeValues)
        {
            OpCodes = opCodeValues.Select(ocv => 
                OpCodeReader.ReadOpCode(ocv).AssertNotNull().Value).ToArray();
        }
    }
}
