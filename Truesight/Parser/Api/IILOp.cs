using System;
using System.Collections.ObjectModel;

namespace Truesight.Parser.Api
{
    public interface IILOp : IEquatable<IILOp>
    {
        IMethodBody Source { get; }
        IPatch Patch { get; }

        ReadOnlyCollection<IILOp> Prefixes { get; }

        IILOpType OpType { get; }
        IILOpSpec OpSpec { get; }
        int Size { get; }
        int SizeOfPrefixes { get; }
        int SizeOfOpCode { get; }
        int SizeOfOperand { get; }

        int Offset { get; }
        int OffsetOfOpCode { get; }
        int OffsetOfOperand { get; }
        int OffsetOfNextOp { get; }

        ReadOnlyCollection<Byte> RawBytes { get; }
        ReadOnlyCollection<Byte> BytesOfPrefixes { get; }
        ReadOnlyCollection<Byte> BytesWithoutPrefixes { get; }
        ReadOnlyCollection<Byte> BytesOfOpCode { get; }
        ReadOnlyCollection<Byte> BytesOfOperand { get; }

        IILOp Prev { get; }
        IILOp Next { get; }
    }
}