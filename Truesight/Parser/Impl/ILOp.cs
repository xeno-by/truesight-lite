using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Reflection.Generics;
using Truesight.Parser.Api;
using Truesight.Parser.Api.Ops;
using XenoGears.Strings;
using Int8 = System.SByte;
using UInt8 = System.Byte;

namespace Truesight.Parser.Impl
{
    [DebuggerNonUserCode]
    public abstract class ILOp : IILOp, IEquatable<ILOp>
    {
        IMethodBody IILOp.Source { get { return Source; } }
        internal MethodBody Source { get; private set; }
        private Module Module { get { return Source.Module; } }
        // note. do not define properties of type Method here since this will break ILTrait.clone
        // that's an awful bug, but I really don't have time to fix it now

        // note. patches are filled in during the second pass so this has an external setter
        internal IPatch Patch { get; set; }
        IPatch IILOp.Patch { get { return Patch; } }

        public ReadOnlyCollection<ILOp> Prefixes { get; private set; }
        ReadOnlyCollection<IILOp> IILOp.Prefixes { get { return Prefixes.Cast<IILOp>().ToReadOnly(); } }

        public abstract IILOpType OpType { get; }
        public IILOpSpec OpSpec { get; private set; }
        public int Size { get { return SizeOfPrefixes + SizeOfOpCode + SizeOfOperand; } }
        public int SizeOfPrefixes { get { return Prefixes.Sum(meta => meta.Size); } }
        public int SizeOfOpCode { get { return OpSpec.OpCode.Size; } }
        // SizeOfOperand is rather complex and is defined elsewhere

        public int Offset { get; private set; }
        public int OffsetOfOpCode { get { return Offset + SizeOfPrefixes; } }
        public int OffsetOfOperand { get { return Offset + SizeOfPrefixes + SizeOfOpCode; } }
        public int OffsetOfNextOp { get { return Offset + Size; } }

        // I made RawBytes include bytes of prefixes
        // since it's natural to expect RawBytes.Length being equal to Size
        public ReadOnlyCollection<Byte> RawBytes { get { return Source.RawIL.Slice(Offset, OffsetOfNextOp).ToReadOnly(); } }
        public ReadOnlyCollection<Byte> BytesOfPrefixes { get { return Source.RawIL.Slice(Offset, OffsetOfOpCode).ToReadOnly(); } }
        public ReadOnlyCollection<Byte> BytesWithoutPrefixes { get { return Source.RawIL.Slice(OffsetOfOpCode, OffsetOfNextOp).ToReadOnly(); } }
        public ReadOnlyCollection<Byte> BytesOfOpCode { get { return Source.RawIL.Slice(OffsetOfOpCode, OffsetOfOperand).ToReadOnly(); } }
        public ReadOnlyCollection<Byte> BytesOfOperand { get { return Source.RawIL.Slice(OffsetOfOperand, OffsetOfNextOp).ToReadOnly(); } }

        public IILOp Prev { get; internal set; }
        public IILOp Next { get; internal set; }

        internal ILOp(MethodBody source, OpCode opcode, int offset)
            : this(source, opcode, offset, null)
        {
        }

        internal ILOp(MethodBody source, OpCode opcode, int offset, ReadOnlyCollection<ILOp> prefixes)
        {
            // method can be null in case we parse raw IL => then args and locals will be unavailable
            Source = source;
            OpSpec = new ILOpSpec(this, opcode);
            Offset = offset - opcode.Size;
            Prefixes = prefixes ?? Enumerable.Empty<ILOp>().ToReadOnly();
        }

        #region SizeOfOperand Implementation

        // note. will crash for Switch since exact number of operands is unknown in advance
        // thus it's made virtual and is a must for being overriden in switch
        public virtual int SizeOfOperand
        {
            get
            {
                switch (OpSpec.OpCode.OperandType)
                {
                    case OperandType.InlineBrTarget:
                        return sizeof(Int32);
                    case OperandType.InlineField:
                        return sizeof(Int32);
                    case OperandType.InlineI:
                        return sizeof(Int32);
                    case OperandType.InlineI8:
                        return sizeof(Int64);
                    case OperandType.InlineMethod:
                        return sizeof(Int32);
                    case OperandType.InlineNone:
                        return 0;
#pragma warning disable 612, 618
                    case OperandType.InlinePhi:
#pragma warning restore 612, 618
                        throw AssertionHelper.Fail();
                    case OperandType.InlineR:
                        return sizeof(Double);
                    case OperandType.InlineSig:
                        return sizeof(Int32);
                    case OperandType.InlineString:
                        return sizeof(Int32);
                    case OperandType.InlineSwitch:
                        throw AssertionHelper.Fail();
                    case OperandType.InlineTok:
                        return sizeof(Int32);
                    case OperandType.InlineType:
                        return sizeof(Int32);
                    case OperandType.InlineVar:
                        return sizeof(Int32);
                    case OperandType.ShortInlineBrTarget:
                        return sizeof(SByte);
                    case OperandType.ShortInlineI:
                        return sizeof(SByte);
                    case OperandType.ShortInlineR:
                        return sizeof(Single);
                    case OperandType.ShortInlineVar:
                        return sizeof(SByte);
                    default:
                        throw AssertionHelper.Fail();
                }
            }
        }

        #endregion

        #region Operand reading routines

        internal Int8 ReadI1(BinaryReader reader)
        {
            return reader.ReadSByte();
        }

        internal Int16 ReadI2(BinaryReader reader)
        {
            return reader.ReadInt16();
        }

        internal Int32 ReadI4(BinaryReader reader)
        {
            return reader.ReadInt32();
        }

        internal Int64 ReadI8(BinaryReader reader)
        {
            return reader.ReadInt64();
        }

        internal UInt8 ReadU1(BinaryReader reader)
        {
            return reader.ReadByte();
        }

        internal UInt16 ReadU2(BinaryReader reader)
        {
            return reader.ReadUInt16();
        }

        internal UInt32 ReadU4(BinaryReader reader)
        {
            return reader.ReadUInt32();
        }

        internal UInt64 ReadU8(BinaryReader reader)
        {
            return reader.ReadUInt64();
        }

        internal Single ReadR4(BinaryReader reader)
        {
            return reader.ReadSingle();
        }

        internal Double ReadR8(BinaryReader reader)
        {
            return reader.ReadDouble();
        }

        internal Int32 ReadMetadataToken(BinaryReader reader)
        {
            return reader.ReadInt32();
        }

        // this is a fake method required for Ldc to compile successfully
        internal String ReadStr(BinaryReader reader)
        {
            throw AssertionHelper.Fail();
        }

        // this is a fake method required for Ldc to compile successfully
        internal String ReadToken(BinaryReader reader)
        {
            throw AssertionHelper.Fail();
        }

        #endregion

        #region Resolution routines

        internal ILOp ResolveReference(Int32 targetOffset)
        {
            return Source.AssertCast<ILOp>().SingleOrDefault(op => op.Offset == targetOffset);
        }

        internal MemberInfo MemberFromToken(Int32 metadataToken)
        {
            if (Module == null) return null;

            // note. before changing anything below, please, get an understanding of what's going on
            // here we're not interested in type arguments of the member being resolved
            // what we do care about though is type arguments of the CONTEXT
            // check out the example here: http://msdn.microsoft.com/en-us/library/ms145421(VS.90).aspx
            var ctx_targs = Source.Type.XGetGenericArguments();
            var ctx_margs = Source.Method.XGetGenericArguments();
            return Module.ResolveMember(metadataToken, ctx_targs, ctx_margs);
        }

        internal FieldInfo FieldFromToken(Int32 metadataToken)
        {
            return MemberFromToken(metadataToken).AssertCast<FieldInfo>();
        }

        internal MethodInfo MethodFromToken(Int32 metadataToken)
        {
            return MemberFromToken(metadataToken).AssertCast<MethodInfo>();
        }

        internal ConstructorInfo CtorFromToken(Int32 metadataToken)
        {
            return MemberFromToken(metadataToken).AssertCast<ConstructorInfo>();
        }

        internal MethodBase MethodBaseFromToken(Int32 metadataToken)
        {
            return MemberFromToken(metadataToken).AssertCast<MethodBase>();
        }

        internal Type TypeFromToken(Int32 metadataToken)
        {
            return MemberFromToken(metadataToken).AssertCast<Type>();
        }

        internal String StringFromToken(Int32 metadataToken)
        {
            return Module == null ? null : Module.ResolveString(metadataToken);
        }

        internal Byte[] SignatureFromToken(Int32 metadataToken)
        {
            return Module == null ? null : Module.ResolveSignature(metadataToken);
        }

        internal MethodBase MethodBaseFromSignature(Byte[] signature)
        {
            return Module == null ? null : ((Func<MethodBase>)(() => { throw new NotImplementedException(); }))();
        }

        #endregion

        #region Stringifying helpers

        internal String ObjectToCSharpLiteral(Object o)
        {
            if (o == null)
            {
                return "null";
            }
            else if (o is String)
            {
                return o.AssertCast<String>().ToCSharpString();
            }
            else if (o is Type)
            {
                var t = (Type)o;
                var s_t = t.GetCSharpRef(ToCSharpOptions.Informative);
                return String.Format("typeof({0})", s_t);
            }
            else if (o is RuntimeTypeHandle)
            {
                var th = (RuntimeTypeHandle)o;
                try
                {
                    var t = Type.GetTypeFromHandle(th);
                    return ObjectToCSharpLiteral(t) + ".TypeHandle";
                }
                catch
                {
                    var fmt = "x" + Marshal.SizeOf(typeof(IntPtr));
                    return "runtime type handle 0x" + th.Value.ToString(fmt);
                }
            }
            else if (o is FieldInfo)
            {
                var f = (FieldInfo)o;
                var s_f = f.GetCSharpRef(ToCSharpOptions.Informative);
                return String.Format("fieldof({0})", s_f);
            }
            else if (o is RuntimeFieldHandle)
            {
                var fh = (RuntimeFieldHandle)o;
                try
                {
                    var f = FieldInfo.GetFieldFromHandle(fh);
                    return ObjectToCSharpLiteral(f) + ".FieldHandle";
                }
                catch
                {
                    var fmt = "x" + Marshal.SizeOf(typeof(IntPtr));
                    return "runtime field handle 0x" + fh.Value.ToString(fmt);
                }
            }
            else if (o is MethodBase)
            {
                var m = (MethodBase)o;
                var s_m = m.GetCSharpRef(ToCSharpOptions.Informative);
                return String.Format("methodof({0})", s_m);
            }
            else if (o is RuntimeMethodHandle)
            {
                var mh = (RuntimeMethodHandle)o;
                try
                {
                    var m = MethodBase.GetMethodFromHandle(mh);
                    return ObjectToCSharpLiteral(m) + ".MethodHandle";
                }
                catch
                {
                    var fmt = "x" + Marshal.SizeOf(typeof(IntPtr));
                    return "runtime method handle 0x" + mh.Value.ToString(fmt);
                }
            }
            else
            {
                return o.ToInvariantString();
            }
        }

        internal String MemberInfoToString(MemberInfo m)
        {
            if (m is Type)
            {
                return TypeToString((Type)m);
            }
            else if (m is FieldInfo)
            {
                return FieldInfoToString((FieldInfo)m);
            }
            else if (m is MethodBase)
            {
                return MethodBaseToString((MethodBase)m);
            }
            else if (m is MethodInfo)
            {
                return MethodInfoToString((MethodInfo)m);
            }
            else if (m is ConstructorInfo)
            {
                return ConstructorInfoToString((ConstructorInfo)m);
            }
            else
            {
                throw AssertionHelper.Fail();
            }
        }

        internal String FieldInfoToString(FieldInfo f)
        {
            return f.GetCSharpRef(ToCSharpOptions.Informative);
        }

        internal String MethodBaseToString(MethodBase m)
        {
            return m.GetCSharpRef(ToCSharpOptions.Informative);
        }

        internal String MethodInfoToString(MethodInfo m)
        {
            return m.GetCSharpRef(ToCSharpOptions.Informative);
        }

        internal String ConstructorInfoToString(ConstructorInfo c)
        {
            return c.GetCSharpRef(ToCSharpOptions.Informative);
        }

        internal String TypeToString(Type t)
        {
            return t.GetCSharpRef(ToCSharpOptions.Informative);
        }

        internal String OperatorTypeToString(OperatorType? t)
        {
            if (t == null) return null;

            switch (t.Value)
            {
                case OperatorType.Add:
                    return "add";
                case OperatorType.And:
                    return "and";
                case OperatorType.Divide:
                    return "div";
                case OperatorType.Equal:
                    return "eq";
                case OperatorType.GreaterThan:
                    return "gt";
                case OperatorType.GreaterThanOrEqual:
                    return "ge";
                case OperatorType.LeftShift:
                    return "shl";
                case OperatorType.LessThan:
                    return "lt";
                case OperatorType.LessThanOrEqual:
                    return "le";
                case OperatorType.NotEqual:
                    return "ne";
                case OperatorType.Modulo:
                    return "rem";
                case OperatorType.Multiply:
                    return "mul";
                case OperatorType.Negate:
                    return "neg";
                case OperatorType.Not:
                    return "not";
                case OperatorType.Or:
                    return "or";
                case OperatorType.RightShift:
                    return "shr";
                case OperatorType.Subtract:
                    return "sub";
                case OperatorType.Xor:
                    return "xor";
                default:
                    throw AssertionHelper.Fail();
            }
        }

        internal String PredicateTypeToString(PredicateType? t)
        {
            if (t == null) return null;

            switch (t.Value)
            {
                case PredicateType.Equal:
                    return "eq";
                case PredicateType.GreaterThan:
                    return "gt";
                case PredicateType.GreaterThanOrEqual:
                    return "ge";
                case PredicateType.LessThan:
                    return "lt";
                case PredicateType.LessThanOrEqual:
                    return "le";
                case PredicateType.NotEqual:
                    return "ne";
                case PredicateType.IsTrue:
                    return "true";
                case PredicateType.IsFalse:
                    return "false";
                default:
                    throw AssertionHelper.Fail();
            }
        }

        internal String OffsetToString(Int32 offset)
        {
            return "0x" + offset.ToString("x4");
        }

        internal String OffsetsToString(IEnumerable<Int32> offsets)
        {
            return offsets.Select(offset => OffsetToString(offset)).StringJoin();
        }

        internal String ByteToString(Byte @byte)
        {
            return @byte.ToString("x2");
        }

        internal String ByteArrayToString(IEnumerable<Byte> bytes)
        {
            return bytes.Aggregate("0x", (run, curr) => run + ByteToString(curr));
        }

        internal String ParameterInfoToString(ParameterInfo pi)
        {
            if (pi == null) return "0 (this)"; // process special case of ldarg.0 for instance methods
            return String.Format("{0} ({1})", pi.Position + 
                (pi.Member.AssertCast<MethodBase>().IsStatic ? 0 : 1), pi.Name);
        }

        internal String ILocalVarToString(ILocalVar lv)
        {
            var name = lv.Source.DebugInfo == null ? null :
                lv.Source.DebugInfo.LocalNames.GetOrDefault(lv.Index);
            return name == null ? lv.Index.ToString() : String.Format("{0} ({1})",
                lv.Index.ToString(), name);
        }

        #endregion

        #region Equality members

        public bool Equals(IILOp other)
        {
            return Equals(other as ILOp);
        }

        public bool Equals(ILOp other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Source, Source) && other.Offset == Offset;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(ILOp)) return false;
            return Equals((ILOp)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Source != null ? Source.GetHashCode() : 0) * 397) ^ Offset;
            }
        }

        public static bool operator ==(ILOp left, ILOp right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ILOp left, ILOp right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}