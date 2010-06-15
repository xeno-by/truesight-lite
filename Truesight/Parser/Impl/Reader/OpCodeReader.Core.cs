using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using XenoGears.Functional;
using XenoGears.Assertions;

namespace Truesight.Parser.Impl.Reader
{
    [DebuggerNonUserCode]
    internal static partial class OpCodeReader
    {
        public static OpCode? ReadOpCode(Int16 opCodeValue)
        {
            return ReadOpCode((UInt16)opCodeValue);
        }

        public static OpCode? ReadOpCode(UInt16 opCodeValue)
        {
            var firstByte = (Byte)(opCodeValue >> 8);
            var secondByte = (Byte)(opCodeValue % (1 << 8));

            var stream = new MemoryStream(firstByte.MkArray().Concat(secondByte).ToArray());
            if (firstByte == 0) stream.ReadByte();

            var opcode = ReadOpCode(stream);
            if (opcode == null)
            {
                return null;
            }
            else
            {
                ((UInt16)opcode.Value.Value == (UInt16)opCodeValue).AssertTrue();
                return opcode;
            }
        }

        public static OpCode? ReadOpCode(Stream stream)
        {
            stream.CanRead.AssertTrue();
            stream.CanSeek.AssertTrue();

            return ReadOpCode(new BinaryReader(stream));
        }

        public static OpCode? ReadOpCode(BinaryReader reader)
        {
            var stream = reader.BaseStream;
            stream.CanRead.AssertTrue();
            stream.CanSeek.AssertTrue();

            if (stream.Position >= stream.Length)
            {
                return null;
            }
            else
            {
                var firstByte = reader.ReadByte();
                if (_oneByteOpCodePrefixes.Contains(firstByte))
                {
                    return _firstByteToOpCode[firstByte];
                }
                else if (_twoByteOpCodePrefixes.Contains(firstByte))
                {
                    if (stream.Position >= stream.Length)
                    {
                        stream.Seek(-1, SeekOrigin.Current);
                        return null;
                    }
                    else
                    {
                        var secondByte = reader.ReadByte();
                        return _secondByteToOpCode[secondByte];
                    }
                }
                else
                {
                    // leave stream intact if we can't read a valid opcode
                    stream.Seek(-1, SeekOrigin.Current);
                    return null;
                }
            }
        }
    }
}
