using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection.Emit;
using Truesight.Parser.Api.Ops;

namespace Truesight.Parser.Impl.Reader
{
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    internal static class ILOpFactory
    {
        public static ILOp Create(OpCode opcode, MethodBody body, BinaryReader reader, ReadOnlyCollection<ILOp> prefixes)
        {
            // the stuff below is important since the constructor will need to read the opcode once again
            reader.BaseStream.Seek(-opcode.Size, SeekOrigin.Current);

            switch ((ushort)opcode.Value)
            {
                case 0x02: // ldarg.0
                    return new Ldarg(body, reader, prefixes);
                case 0x03: // ldarg.1
                    return new Ldarg(body, reader, prefixes);
                case 0x04: // ldarg.2
                    return new Ldarg(body, reader, prefixes);
                case 0x05: // ldarg.3
                    return new Ldarg(body, reader, prefixes);
                case 0x0e: // ldarg.s
                    return new Ldarg(body, reader, prefixes);
                case 0xfe09: // ldarg
                    return new Ldarg(body, reader, prefixes);
                case 0xfe00: // arglist
                    return new Arglist(body, reader, prefixes);
                case 0x27: // jmp
                    return new Jmp(body, reader, prefixes);
                case 0x45: // switch
                    return new Switch(body, reader, prefixes);
                case 0x10: // starg.s
                    return new Starg(body, reader, prefixes);
                case 0xfe0b: // starg
                    return new Starg(body, reader, prefixes);
                case 0xfe14: // tail.
                    return new Tail(body, reader, prefixes);
                case 0x9b: // stelem.i
                    return new Stelem(body, reader, prefixes);
                case 0x9c: // stelem.i1
                    return new Stelem(body, reader, prefixes);
                case 0x9d: // stelem.i2
                    return new Stelem(body, reader, prefixes);
                case 0x9e: // stelem.i4
                    return new Stelem(body, reader, prefixes);
                case 0x9f: // stelem.i8
                    return new Stelem(body, reader, prefixes);
                case 0xa0: // stelem.r4
                    return new Stelem(body, reader, prefixes);
                case 0xa1: // stelem.r8
                    return new Stelem(body, reader, prefixes);
                case 0xa2: // stelem.ref
                    return new Stelem(body, reader, prefixes);
                case 0xa4: // stelem
                    return new Stelem(body, reader, prefixes);
                case 0xfe1a: // rethrow
                    return new Rethrow(body, reader, prefixes);
                case 0x8e: // ldlen
                    return new Ldlen(body, reader, prefixes);
                case 0x14: // ldnull
                    return new Ldc(body, reader, prefixes);
                case 0x15: // ldc.i4.m1
                    return new Ldc(body, reader, prefixes);
                case 0x16: // ldc.i4.0
                    return new Ldc(body, reader, prefixes);
                case 0x17: // ldc.i4.1
                    return new Ldc(body, reader, prefixes);
                case 0x18: // ldc.i4.2
                    return new Ldc(body, reader, prefixes);
                case 0x19: // ldc.i4.3
                    return new Ldc(body, reader, prefixes);
                case 0x1a: // ldc.i4.4
                    return new Ldc(body, reader, prefixes);
                case 0x1b: // ldc.i4.5
                    return new Ldc(body, reader, prefixes);
                case 0x1c: // ldc.i4.6
                    return new Ldc(body, reader, prefixes);
                case 0x1d: // ldc.i4.7
                    return new Ldc(body, reader, prefixes);
                case 0x1e: // ldc.i4.8
                    return new Ldc(body, reader, prefixes);
                case 0x1f: // ldc.i4.s
                    return new Ldc(body, reader, prefixes);
                case 0x20: // ldc.i4
                    return new Ldc(body, reader, prefixes);
                case 0x21: // ldc.i8
                    return new Ldc(body, reader, prefixes);
                case 0x22: // ldc.r4
                    return new Ldc(body, reader, prefixes);
                case 0x23: // ldc.r8
                    return new Ldc(body, reader, prefixes);
                case 0x72: // ldstr
                    return new Ldc(body, reader, prefixes);
                case 0xd0: // ldtoken
                    return new Ldc(body, reader, prefixes);
                case 0x0f: // ldarga.s
                    return new Ldarga(body, reader, prefixes);
                case 0xfe0a: // ldarga
                    return new Ldarga(body, reader, prefixes);
                case 0xc3: // ckfinite
                    return new Ckfinite(body, reader, prefixes);
                case 0x75: // isinst
                    return new Isinst(body, reader, prefixes);
                case 0x70: // cpobj
                    return new Cpobj(body, reader, prefixes);
                case 0x8f: // ldelema
                    return new Ldelema(body, reader, prefixes);
                case 0xfe16: // constrained.
                    return new Constrained(body, reader, prefixes);
                case 0x06: // ldloc.0
                    return new Ldloc(body, reader, prefixes);
                case 0x07: // ldloc.1
                    return new Ldloc(body, reader, prefixes);
                case 0x08: // ldloc.2
                    return new Ldloc(body, reader, prefixes);
                case 0x09: // ldloc.3
                    return new Ldloc(body, reader, prefixes);
                case 0x11: // ldloc.s
                    return new Ldloc(body, reader, prefixes);
                case 0xfe0c: // ldloc
                    return new Ldloc(body, reader, prefixes);
                case 0xfe06: // ldftn
                    return new Ldftn(body, reader, prefixes);
                case 0xfe07: // ldvirtftn
                    return new Ldftn(body, reader, prefixes);
                case 0xfe11: // endfilter
                    return new Endfilter(body, reader, prefixes);
                case 0x46: // ldind.i1
                    return new Ldind(body, reader, prefixes);
                case 0x47: // ldind.u1
                    return new Ldind(body, reader, prefixes);
                case 0x48: // ldind.i2
                    return new Ldind(body, reader, prefixes);
                case 0x49: // ldind.u2
                    return new Ldind(body, reader, prefixes);
                case 0x4a: // ldind.i4
                    return new Ldind(body, reader, prefixes);
                case 0x4b: // ldind.u4
                    return new Ldind(body, reader, prefixes);
                case 0x4c: // ldind.i8
                    return new Ldind(body, reader, prefixes);
                case 0x4d: // ldind.i
                    return new Ldind(body, reader, prefixes);
                case 0x4e: // ldind.r4
                    return new Ldind(body, reader, prefixes);
                case 0x4f: // ldind.r8
                    return new Ldind(body, reader, prefixes);
                case 0x50: // ldind.ref
                    return new Ldind(body, reader, prefixes);
                case 0x71: // ldobj
                    return new Ldind(body, reader, prefixes);
                case 0xfe15: // initobj
                    return new Initobj(body, reader, prefixes);
                case 0x0a: // stloc.0
                    return new Stloc(body, reader, prefixes);
                case 0x0b: // stloc.1
                    return new Stloc(body, reader, prefixes);
                case 0x0c: // stloc.2
                    return new Stloc(body, reader, prefixes);
                case 0x0d: // stloc.3
                    return new Stloc(body, reader, prefixes);
                case 0x13: // stloc.s
                    return new Stloc(body, reader, prefixes);
                case 0xfe0e: // stloc
                    return new Stloc(body, reader, prefixes);
                case 0xfe1c: // sizeof
                    return new Sizeof(body, reader, prefixes);
                case 0x7c: // ldflda
                    return new Ldflda(body, reader, prefixes);
                case 0x7f: // ldsflda
                    return new Ldflda(body, reader, prefixes);
                case 0x7b: // ldfld
                    return new Ldfld(body, reader, prefixes);
                case 0x7e: // ldsfld
                    return new Ldfld(body, reader, prefixes);
                case 0x01: // break
                    return new Break(body, reader, prefixes);
                case 0x51: // stind.ref
                    return new Stind(body, reader, prefixes);
                case 0x52: // stind.i1
                    return new Stind(body, reader, prefixes);
                case 0x53: // stind.i2
                    return new Stind(body, reader, prefixes);
                case 0x54: // stind.i4
                    return new Stind(body, reader, prefixes);
                case 0x55: // stind.i8
                    return new Stind(body, reader, prefixes);
                case 0x56: // stind.r4
                    return new Stind(body, reader, prefixes);
                case 0x57: // stind.r8
                    return new Stind(body, reader, prefixes);
                case 0x81: // stobj
                    return new Stind(body, reader, prefixes);
                case 0xdf: // stind.i
                    return new Stind(body, reader, prefixes);
                case 0x12: // ldloca.s
                    return new Ldloca(body, reader, prefixes);
                case 0xfe0d: // ldloca
                    return new Ldloca(body, reader, prefixes);
                case 0xfe17: // cpblk
                    return new Cpblk(body, reader, prefixes);
                case 0x7d: // stfld
                    return new Stfld(body, reader, prefixes);
                case 0x80: // stsfld
                    return new Stfld(body, reader, prefixes);
                case 0x67: // conv.i1
                    return new Cast(body, reader, prefixes);
                case 0x68: // conv.i2
                    return new Cast(body, reader, prefixes);
                case 0x69: // conv.i4
                    return new Cast(body, reader, prefixes);
                case 0x6a: // conv.i8
                    return new Cast(body, reader, prefixes);
                case 0x6b: // conv.r4
                    return new Cast(body, reader, prefixes);
                case 0x6c: // conv.r8
                    return new Cast(body, reader, prefixes);
                case 0x6d: // conv.u4
                    return new Cast(body, reader, prefixes);
                case 0x6e: // conv.u8
                    return new Cast(body, reader, prefixes);
                case 0x74: // castclass
                    return new Cast(body, reader, prefixes);
                case 0x76: // conv.r.un
                    return new Cast(body, reader, prefixes);
                case 0x79: // unbox
                    return new Cast(body, reader, prefixes);
                case 0x82: // conv.ovf.i1.un
                    return new Cast(body, reader, prefixes);
                case 0x83: // conv.ovf.i2.un
                    return new Cast(body, reader, prefixes);
                case 0x84: // conv.ovf.i4.un
                    return new Cast(body, reader, prefixes);
                case 0x85: // conv.ovf.i8.un
                    return new Cast(body, reader, prefixes);
                case 0x86: // conv.ovf.u1.un
                    return new Cast(body, reader, prefixes);
                case 0x87: // conv.ovf.u2.un
                    return new Cast(body, reader, prefixes);
                case 0x88: // conv.ovf.u4.un
                    return new Cast(body, reader, prefixes);
                case 0x89: // conv.ovf.u8.un
                    return new Cast(body, reader, prefixes);
                case 0x8a: // conv.ovf.i.un
                    return new Cast(body, reader, prefixes);
                case 0x8b: // conv.ovf.u.un
                    return new Cast(body, reader, prefixes);
                case 0x8c: // box
                    return new Cast(body, reader, prefixes);
                case 0xa5: // unbox.any
                    return new Cast(body, reader, prefixes);
                case 0xb3: // conv.ovf.i1
                    return new Cast(body, reader, prefixes);
                case 0xb4: // conv.ovf.u1
                    return new Cast(body, reader, prefixes);
                case 0xb5: // conv.ovf.i2
                    return new Cast(body, reader, prefixes);
                case 0xb6: // conv.ovf.u2
                    return new Cast(body, reader, prefixes);
                case 0xb7: // conv.ovf.i4
                    return new Cast(body, reader, prefixes);
                case 0xb8: // conv.ovf.u4
                    return new Cast(body, reader, prefixes);
                case 0xb9: // conv.ovf.i8
                    return new Cast(body, reader, prefixes);
                case 0xba: // conv.ovf.u8
                    return new Cast(body, reader, prefixes);
                case 0xd1: // conv.u2
                    return new Cast(body, reader, prefixes);
                case 0xd2: // conv.u1
                    return new Cast(body, reader, prefixes);
                case 0xd3: // conv.i
                    return new Cast(body, reader, prefixes);
                case 0xd4: // conv.ovf.i
                    return new Cast(body, reader, prefixes);
                case 0xd5: // conv.ovf.u
                    return new Cast(body, reader, prefixes);
                case 0xe0: // conv.u
                    return new Cast(body, reader, prefixes);
                case 0xc6: // mkrefany
                    return new Mkrefany(body, reader, prefixes);
                case 0xdc: // endfinally
                    return new Endfinally(body, reader, prefixes);
                case 0x28: // call
                    return new Call(body, reader, prefixes);
                case 0x29: // calli
                    return new Call(body, reader, prefixes);
                case 0x6f: // callvirt
                    return new Call(body, reader, prefixes);
                case 0xfe1d: // refanytype
                    return new Refanytype(body, reader, prefixes);
                case 0xfe0f: // localloc
                    return new Localloc(body, reader, prefixes);
                case 0xfe18: // initblk
                    return new Initblk(body, reader, prefixes);
                case 0xfe12: // unaligned.
                    return new Unaligned(body, reader, prefixes);
                case 0x26: // pop
                    return new Pop(body, reader, prefixes);
                case 0x2b: // br.s
                    return new Branch(body, reader, prefixes);
                case 0x2c: // brfalse.s
                    return new Branch(body, reader, prefixes);
                case 0x2d: // brtrue.s
                    return new Branch(body, reader, prefixes);
                case 0x2e: // beq.s
                    return new Branch(body, reader, prefixes);
                case 0x2f: // bge.s
                    return new Branch(body, reader, prefixes);
                case 0x30: // bgt.s
                    return new Branch(body, reader, prefixes);
                case 0x31: // ble.s
                    return new Branch(body, reader, prefixes);
                case 0x32: // blt.s
                    return new Branch(body, reader, prefixes);
                case 0x33: // bne.un.s
                    return new Branch(body, reader, prefixes);
                case 0x34: // bge.un.s
                    return new Branch(body, reader, prefixes);
                case 0x35: // bgt.un.s
                    return new Branch(body, reader, prefixes);
                case 0x36: // ble.un.s
                    return new Branch(body, reader, prefixes);
                case 0x37: // blt.un.s
                    return new Branch(body, reader, prefixes);
                case 0x38: // br
                    return new Branch(body, reader, prefixes);
                case 0x39: // brfalse
                    return new Branch(body, reader, prefixes);
                case 0x3a: // brtrue
                    return new Branch(body, reader, prefixes);
                case 0x3b: // beq
                    return new Branch(body, reader, prefixes);
                case 0x3c: // bge
                    return new Branch(body, reader, prefixes);
                case 0x3d: // bgt
                    return new Branch(body, reader, prefixes);
                case 0x3e: // ble
                    return new Branch(body, reader, prefixes);
                case 0x3f: // blt
                    return new Branch(body, reader, prefixes);
                case 0x40: // bne.un
                    return new Branch(body, reader, prefixes);
                case 0x41: // bge.un
                    return new Branch(body, reader, prefixes);
                case 0x42: // bgt.un
                    return new Branch(body, reader, prefixes);
                case 0x43: // ble.un
                    return new Branch(body, reader, prefixes);
                case 0x44: // blt.un
                    return new Branch(body, reader, prefixes);
                case 0xdd: // leave
                    return new Branch(body, reader, prefixes);
                case 0xde: // leave.s
                    return new Branch(body, reader, prefixes);
                case 0xfe13: // volatile.
                    return new Volatile(body, reader, prefixes);
                case 0x7a: // throw
                    return new Throw(body, reader, prefixes);
                case 0x2a: // ret
                    return new Ret(body, reader, prefixes);
                case 0xfe1e: // readonly.
                    return new Readonly(body, reader, prefixes);
                case 0x90: // ldelem.i1
                    return new Ldelem(body, reader, prefixes);
                case 0x91: // ldelem.u1
                    return new Ldelem(body, reader, prefixes);
                case 0x92: // ldelem.i2
                    return new Ldelem(body, reader, prefixes);
                case 0x93: // ldelem.u2
                    return new Ldelem(body, reader, prefixes);
                case 0x94: // ldelem.i4
                    return new Ldelem(body, reader, prefixes);
                case 0x95: // ldelem.u4
                    return new Ldelem(body, reader, prefixes);
                case 0x96: // ldelem.i8
                    return new Ldelem(body, reader, prefixes);
                case 0x97: // ldelem.i
                    return new Ldelem(body, reader, prefixes);
                case 0x98: // ldelem.r4
                    return new Ldelem(body, reader, prefixes);
                case 0x99: // ldelem.r8
                    return new Ldelem(body, reader, prefixes);
                case 0x9a: // ldelem.ref
                    return new Ldelem(body, reader, prefixes);
                case 0xa3: // ldelem
                    return new Ldelem(body, reader, prefixes);
                case 0x25: // dup
                    return new Dup(body, reader, prefixes);
                case 0xc2: // refanyval
                    return new Refanyval(body, reader, prefixes);
                case 0x58: // add
                    return new Operator(body, reader, prefixes);
                case 0x59: // sub
                    return new Operator(body, reader, prefixes);
                case 0x5a: // mul
                    return new Operator(body, reader, prefixes);
                case 0x5b: // div
                    return new Operator(body, reader, prefixes);
                case 0x5c: // div.un
                    return new Operator(body, reader, prefixes);
                case 0x5d: // rem
                    return new Operator(body, reader, prefixes);
                case 0x5e: // rem.un
                    return new Operator(body, reader, prefixes);
                case 0x5f: // and
                    return new Operator(body, reader, prefixes);
                case 0x60: // or
                    return new Operator(body, reader, prefixes);
                case 0x61: // xor
                    return new Operator(body, reader, prefixes);
                case 0x62: // shl
                    return new Operator(body, reader, prefixes);
                case 0x63: // shr
                    return new Operator(body, reader, prefixes);
                case 0x64: // shr.un
                    return new Operator(body, reader, prefixes);
                case 0x65: // neg
                    return new Operator(body, reader, prefixes);
                case 0x66: // not
                    return new Operator(body, reader, prefixes);
                case 0xd6: // add.ovf
                    return new Operator(body, reader, prefixes);
                case 0xd7: // add.ovf.un
                    return new Operator(body, reader, prefixes);
                case 0xd8: // mul.ovf
                    return new Operator(body, reader, prefixes);
                case 0xd9: // mul.ovf.un
                    return new Operator(body, reader, prefixes);
                case 0xda: // sub.ovf
                    return new Operator(body, reader, prefixes);
                case 0xdb: // sub.ovf.un
                    return new Operator(body, reader, prefixes);
                case 0xfe01: // ceq
                    return new Operator(body, reader, prefixes);
                case 0xfe02: // cgt
                    return new Operator(body, reader, prefixes);
                case 0xfe03: // cgt.un
                    return new Operator(body, reader, prefixes);
                case 0xfe04: // clt
                    return new Operator(body, reader, prefixes);
                case 0xfe05: // clt.un
                    return new Operator(body, reader, prefixes);
                case 0x00: // nop
                    return new Nop(body, reader, prefixes);
                case 0x73: // newobj
                    return new New(body, reader, prefixes);
                case 0x8d: // newarr
                    return new New(body, reader, prefixes);
                default: 
                    throw new NotSupportedException(String.Format("Opcode \"0x{0:x4}\" is not supported", opcode.Value));
            }
        }
    }
}

