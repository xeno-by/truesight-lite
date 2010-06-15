using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using XenoGears.Assertions;

namespace Truesight.Parser.Impl.Reader
{
    internal static partial class OpCodeReader
    {
        private static Object _staticCtorLock = new Object();
        private static bool _staticCtorAintRun = true;

        private static HashSet<Byte> _oneByteOpCodePrefixes = new HashSet<Byte>();
        private static Dictionary<Byte, OpCode> _firstByteToOpCode = new Dictionary<Byte, OpCode>();
        private static HashSet<Byte> _twoByteOpCodePrefixes = new HashSet<Byte>();
        private static Dictionary<Byte, OpCode> _secondByteToOpCode = new Dictionary<Byte, OpCode>();

        static OpCodeReader()
        {
            if (_staticCtorAintRun)
            {
                lock (_staticCtorLock)
                {
                    if (_staticCtorAintRun)
                    {
                        try
                        {
                            // nop
                            _oneByteOpCodePrefixes.Add(0x00);
                            _firstByteToOpCode.Add(0x00, OpCodes.Nop);

                            // break
                            _oneByteOpCodePrefixes.Add(0x01);
                            _firstByteToOpCode.Add(0x01, OpCodes.Break);

                            // ldarg.0
                            _oneByteOpCodePrefixes.Add(0x02);
                            _firstByteToOpCode.Add(0x02, OpCodes.Ldarg_0);

                            // ldarg.1
                            _oneByteOpCodePrefixes.Add(0x03);
                            _firstByteToOpCode.Add(0x03, OpCodes.Ldarg_1);

                            // ldarg.2
                            _oneByteOpCodePrefixes.Add(0x04);
                            _firstByteToOpCode.Add(0x04, OpCodes.Ldarg_2);

                            // ldarg.3
                            _oneByteOpCodePrefixes.Add(0x05);
                            _firstByteToOpCode.Add(0x05, OpCodes.Ldarg_3);

                            // ldloc.0
                            _oneByteOpCodePrefixes.Add(0x06);
                            _firstByteToOpCode.Add(0x06, OpCodes.Ldloc_0);

                            // ldloc.1
                            _oneByteOpCodePrefixes.Add(0x07);
                            _firstByteToOpCode.Add(0x07, OpCodes.Ldloc_1);

                            // ldloc.2
                            _oneByteOpCodePrefixes.Add(0x08);
                            _firstByteToOpCode.Add(0x08, OpCodes.Ldloc_2);

                            // ldloc.3
                            _oneByteOpCodePrefixes.Add(0x09);
                            _firstByteToOpCode.Add(0x09, OpCodes.Ldloc_3);

                            // stloc.0
                            _oneByteOpCodePrefixes.Add(0x0a);
                            _firstByteToOpCode.Add(0x0a, OpCodes.Stloc_0);

                            // stloc.1
                            _oneByteOpCodePrefixes.Add(0x0b);
                            _firstByteToOpCode.Add(0x0b, OpCodes.Stloc_1);

                            // stloc.2
                            _oneByteOpCodePrefixes.Add(0x0c);
                            _firstByteToOpCode.Add(0x0c, OpCodes.Stloc_2);

                            // stloc.3
                            _oneByteOpCodePrefixes.Add(0x0d);
                            _firstByteToOpCode.Add(0x0d, OpCodes.Stloc_3);

                            // ldarg.s
                            _oneByteOpCodePrefixes.Add(0x0e);
                            _firstByteToOpCode.Add(0x0e, OpCodes.Ldarg_S);

                            // ldarga.s
                            _oneByteOpCodePrefixes.Add(0x0f);
                            _firstByteToOpCode.Add(0x0f, OpCodes.Ldarga_S);

                            // starg.s
                            _oneByteOpCodePrefixes.Add(0x10);
                            _firstByteToOpCode.Add(0x10, OpCodes.Starg_S);

                            // ldloc.s
                            _oneByteOpCodePrefixes.Add(0x11);
                            _firstByteToOpCode.Add(0x11, OpCodes.Ldloc_S);

                            // ldloca.s
                            _oneByteOpCodePrefixes.Add(0x12);
                            _firstByteToOpCode.Add(0x12, OpCodes.Ldloca_S);

                            // stloc.s
                            _oneByteOpCodePrefixes.Add(0x13);
                            _firstByteToOpCode.Add(0x13, OpCodes.Stloc_S);

                            // ldnull
                            _oneByteOpCodePrefixes.Add(0x14);
                            _firstByteToOpCode.Add(0x14, OpCodes.Ldnull);

                            // ldc.i4.m1
                            _oneByteOpCodePrefixes.Add(0x15);
                            _firstByteToOpCode.Add(0x15, OpCodes.Ldc_I4_M1);

                            // ldc.i4.0
                            _oneByteOpCodePrefixes.Add(0x16);
                            _firstByteToOpCode.Add(0x16, OpCodes.Ldc_I4_0);

                            // ldc.i4.1
                            _oneByteOpCodePrefixes.Add(0x17);
                            _firstByteToOpCode.Add(0x17, OpCodes.Ldc_I4_1);

                            // ldc.i4.2
                            _oneByteOpCodePrefixes.Add(0x18);
                            _firstByteToOpCode.Add(0x18, OpCodes.Ldc_I4_2);

                            // ldc.i4.3
                            _oneByteOpCodePrefixes.Add(0x19);
                            _firstByteToOpCode.Add(0x19, OpCodes.Ldc_I4_3);

                            // ldc.i4.4
                            _oneByteOpCodePrefixes.Add(0x1a);
                            _firstByteToOpCode.Add(0x1a, OpCodes.Ldc_I4_4);

                            // ldc.i4.5
                            _oneByteOpCodePrefixes.Add(0x1b);
                            _firstByteToOpCode.Add(0x1b, OpCodes.Ldc_I4_5);

                            // ldc.i4.6
                            _oneByteOpCodePrefixes.Add(0x1c);
                            _firstByteToOpCode.Add(0x1c, OpCodes.Ldc_I4_6);

                            // ldc.i4.7
                            _oneByteOpCodePrefixes.Add(0x1d);
                            _firstByteToOpCode.Add(0x1d, OpCodes.Ldc_I4_7);

                            // ldc.i4.8
                            _oneByteOpCodePrefixes.Add(0x1e);
                            _firstByteToOpCode.Add(0x1e, OpCodes.Ldc_I4_8);

                            // ldc.i4.s
                            _oneByteOpCodePrefixes.Add(0x1f);
                            _firstByteToOpCode.Add(0x1f, OpCodes.Ldc_I4_S);

                            // ldc.i4
                            _oneByteOpCodePrefixes.Add(0x20);
                            _firstByteToOpCode.Add(0x20, OpCodes.Ldc_I4);

                            // ldc.i8
                            _oneByteOpCodePrefixes.Add(0x21);
                            _firstByteToOpCode.Add(0x21, OpCodes.Ldc_I8);

                            // ldc.r4
                            _oneByteOpCodePrefixes.Add(0x22);
                            _firstByteToOpCode.Add(0x22, OpCodes.Ldc_R4);

                            // ldc.r8
                            _oneByteOpCodePrefixes.Add(0x23);
                            _firstByteToOpCode.Add(0x23, OpCodes.Ldc_R8);

                            // dup
                            _oneByteOpCodePrefixes.Add(0x25);
                            _firstByteToOpCode.Add(0x25, OpCodes.Dup);

                            // pop
                            _oneByteOpCodePrefixes.Add(0x26);
                            _firstByteToOpCode.Add(0x26, OpCodes.Pop);

                            // jmp
                            _oneByteOpCodePrefixes.Add(0x27);
                            _firstByteToOpCode.Add(0x27, OpCodes.Jmp);

                            // call
                            _oneByteOpCodePrefixes.Add(0x28);
                            _firstByteToOpCode.Add(0x28, OpCodes.Call);

                            // calli
                            _oneByteOpCodePrefixes.Add(0x29);
                            _firstByteToOpCode.Add(0x29, OpCodes.Calli);

                            // ret
                            _oneByteOpCodePrefixes.Add(0x2a);
                            _firstByteToOpCode.Add(0x2a, OpCodes.Ret);

                            // br.s
                            _oneByteOpCodePrefixes.Add(0x2b);
                            _firstByteToOpCode.Add(0x2b, OpCodes.Br_S);

                            // brfalse.s
                            _oneByteOpCodePrefixes.Add(0x2c);
                            _firstByteToOpCode.Add(0x2c, OpCodes.Brfalse_S);

                            // brtrue.s
                            _oneByteOpCodePrefixes.Add(0x2d);
                            _firstByteToOpCode.Add(0x2d, OpCodes.Brtrue_S);

                            // beq.s
                            _oneByteOpCodePrefixes.Add(0x2e);
                            _firstByteToOpCode.Add(0x2e, OpCodes.Beq_S);

                            // bge.s
                            _oneByteOpCodePrefixes.Add(0x2f);
                            _firstByteToOpCode.Add(0x2f, OpCodes.Bge_S);

                            // bgt.s
                            _oneByteOpCodePrefixes.Add(0x30);
                            _firstByteToOpCode.Add(0x30, OpCodes.Bgt_S);

                            // ble.s
                            _oneByteOpCodePrefixes.Add(0x31);
                            _firstByteToOpCode.Add(0x31, OpCodes.Ble_S);

                            // blt.s
                            _oneByteOpCodePrefixes.Add(0x32);
                            _firstByteToOpCode.Add(0x32, OpCodes.Blt_S);

                            // bne.un.s
                            _oneByteOpCodePrefixes.Add(0x33);
                            _firstByteToOpCode.Add(0x33, OpCodes.Bne_Un_S);

                            // bge.un.s
                            _oneByteOpCodePrefixes.Add(0x34);
                            _firstByteToOpCode.Add(0x34, OpCodes.Bge_Un_S);

                            // bgt.un.s
                            _oneByteOpCodePrefixes.Add(0x35);
                            _firstByteToOpCode.Add(0x35, OpCodes.Bgt_Un_S);

                            // ble.un.s
                            _oneByteOpCodePrefixes.Add(0x36);
                            _firstByteToOpCode.Add(0x36, OpCodes.Ble_Un_S);

                            // blt.un.s
                            _oneByteOpCodePrefixes.Add(0x37);
                            _firstByteToOpCode.Add(0x37, OpCodes.Blt_Un_S);

                            // br
                            _oneByteOpCodePrefixes.Add(0x38);
                            _firstByteToOpCode.Add(0x38, OpCodes.Br);

                            // brfalse
                            _oneByteOpCodePrefixes.Add(0x39);
                            _firstByteToOpCode.Add(0x39, OpCodes.Brfalse);

                            // brtrue
                            _oneByteOpCodePrefixes.Add(0x3a);
                            _firstByteToOpCode.Add(0x3a, OpCodes.Brtrue);

                            // beq
                            _oneByteOpCodePrefixes.Add(0x3b);
                            _firstByteToOpCode.Add(0x3b, OpCodes.Beq);

                            // bge
                            _oneByteOpCodePrefixes.Add(0x3c);
                            _firstByteToOpCode.Add(0x3c, OpCodes.Bge);

                            // bgt
                            _oneByteOpCodePrefixes.Add(0x3d);
                            _firstByteToOpCode.Add(0x3d, OpCodes.Bgt);

                            // ble
                            _oneByteOpCodePrefixes.Add(0x3e);
                            _firstByteToOpCode.Add(0x3e, OpCodes.Ble);

                            // blt
                            _oneByteOpCodePrefixes.Add(0x3f);
                            _firstByteToOpCode.Add(0x3f, OpCodes.Blt);

                            // bne.un
                            _oneByteOpCodePrefixes.Add(0x40);
                            _firstByteToOpCode.Add(0x40, OpCodes.Bne_Un);

                            // bge.un
                            _oneByteOpCodePrefixes.Add(0x41);
                            _firstByteToOpCode.Add(0x41, OpCodes.Bge_Un);

                            // bgt.un
                            _oneByteOpCodePrefixes.Add(0x42);
                            _firstByteToOpCode.Add(0x42, OpCodes.Bgt_Un);

                            // ble.un
                            _oneByteOpCodePrefixes.Add(0x43);
                            _firstByteToOpCode.Add(0x43, OpCodes.Ble_Un);

                            // blt.un
                            _oneByteOpCodePrefixes.Add(0x44);
                            _firstByteToOpCode.Add(0x44, OpCodes.Blt_Un);

                            // switch
                            _oneByteOpCodePrefixes.Add(0x45);
                            _firstByteToOpCode.Add(0x45, OpCodes.Switch);

                            // ldind.i1
                            _oneByteOpCodePrefixes.Add(0x46);
                            _firstByteToOpCode.Add(0x46, OpCodes.Ldind_I1);

                            // ldind.u1
                            _oneByteOpCodePrefixes.Add(0x47);
                            _firstByteToOpCode.Add(0x47, OpCodes.Ldind_U1);

                            // ldind.i2
                            _oneByteOpCodePrefixes.Add(0x48);
                            _firstByteToOpCode.Add(0x48, OpCodes.Ldind_I2);

                            // ldind.u2
                            _oneByteOpCodePrefixes.Add(0x49);
                            _firstByteToOpCode.Add(0x49, OpCodes.Ldind_U2);

                            // ldind.i4
                            _oneByteOpCodePrefixes.Add(0x4a);
                            _firstByteToOpCode.Add(0x4a, OpCodes.Ldind_I4);

                            // ldind.u4
                            _oneByteOpCodePrefixes.Add(0x4b);
                            _firstByteToOpCode.Add(0x4b, OpCodes.Ldind_U4);

                            // ldind.i8
                            _oneByteOpCodePrefixes.Add(0x4c);
                            _firstByteToOpCode.Add(0x4c, OpCodes.Ldind_I8);

                            // ldind.i
                            _oneByteOpCodePrefixes.Add(0x4d);
                            _firstByteToOpCode.Add(0x4d, OpCodes.Ldind_I);

                            // ldind.r4
                            _oneByteOpCodePrefixes.Add(0x4e);
                            _firstByteToOpCode.Add(0x4e, OpCodes.Ldind_R4);

                            // ldind.r8
                            _oneByteOpCodePrefixes.Add(0x4f);
                            _firstByteToOpCode.Add(0x4f, OpCodes.Ldind_R8);

                            // ldind.ref
                            _oneByteOpCodePrefixes.Add(0x50);
                            _firstByteToOpCode.Add(0x50, OpCodes.Ldind_Ref);

                            // stind.ref
                            _oneByteOpCodePrefixes.Add(0x51);
                            _firstByteToOpCode.Add(0x51, OpCodes.Stind_Ref);

                            // stind.i1
                            _oneByteOpCodePrefixes.Add(0x52);
                            _firstByteToOpCode.Add(0x52, OpCodes.Stind_I1);

                            // stind.i2
                            _oneByteOpCodePrefixes.Add(0x53);
                            _firstByteToOpCode.Add(0x53, OpCodes.Stind_I2);

                            // stind.i4
                            _oneByteOpCodePrefixes.Add(0x54);
                            _firstByteToOpCode.Add(0x54, OpCodes.Stind_I4);

                            // stind.i8
                            _oneByteOpCodePrefixes.Add(0x55);
                            _firstByteToOpCode.Add(0x55, OpCodes.Stind_I8);

                            // stind.r4
                            _oneByteOpCodePrefixes.Add(0x56);
                            _firstByteToOpCode.Add(0x56, OpCodes.Stind_R4);

                            // stind.r8
                            _oneByteOpCodePrefixes.Add(0x57);
                            _firstByteToOpCode.Add(0x57, OpCodes.Stind_R8);

                            // add
                            _oneByteOpCodePrefixes.Add(0x58);
                            _firstByteToOpCode.Add(0x58, OpCodes.Add);

                            // sub
                            _oneByteOpCodePrefixes.Add(0x59);
                            _firstByteToOpCode.Add(0x59, OpCodes.Sub);

                            // mul
                            _oneByteOpCodePrefixes.Add(0x5a);
                            _firstByteToOpCode.Add(0x5a, OpCodes.Mul);

                            // div
                            _oneByteOpCodePrefixes.Add(0x5b);
                            _firstByteToOpCode.Add(0x5b, OpCodes.Div);

                            // div.un
                            _oneByteOpCodePrefixes.Add(0x5c);
                            _firstByteToOpCode.Add(0x5c, OpCodes.Div_Un);

                            // rem
                            _oneByteOpCodePrefixes.Add(0x5d);
                            _firstByteToOpCode.Add(0x5d, OpCodes.Rem);

                            // rem.un
                            _oneByteOpCodePrefixes.Add(0x5e);
                            _firstByteToOpCode.Add(0x5e, OpCodes.Rem_Un);

                            // and
                            _oneByteOpCodePrefixes.Add(0x5f);
                            _firstByteToOpCode.Add(0x5f, OpCodes.And);

                            // or
                            _oneByteOpCodePrefixes.Add(0x60);
                            _firstByteToOpCode.Add(0x60, OpCodes.Or);

                            // xor
                            _oneByteOpCodePrefixes.Add(0x61);
                            _firstByteToOpCode.Add(0x61, OpCodes.Xor);

                            // shl
                            _oneByteOpCodePrefixes.Add(0x62);
                            _firstByteToOpCode.Add(0x62, OpCodes.Shl);

                            // shr
                            _oneByteOpCodePrefixes.Add(0x63);
                            _firstByteToOpCode.Add(0x63, OpCodes.Shr);

                            // shr.un
                            _oneByteOpCodePrefixes.Add(0x64);
                            _firstByteToOpCode.Add(0x64, OpCodes.Shr_Un);

                            // neg
                            _oneByteOpCodePrefixes.Add(0x65);
                            _firstByteToOpCode.Add(0x65, OpCodes.Neg);

                            // not
                            _oneByteOpCodePrefixes.Add(0x66);
                            _firstByteToOpCode.Add(0x66, OpCodes.Not);

                            // conv.i1
                            _oneByteOpCodePrefixes.Add(0x67);
                            _firstByteToOpCode.Add(0x67, OpCodes.Conv_I1);

                            // conv.i2
                            _oneByteOpCodePrefixes.Add(0x68);
                            _firstByteToOpCode.Add(0x68, OpCodes.Conv_I2);

                            // conv.i4
                            _oneByteOpCodePrefixes.Add(0x69);
                            _firstByteToOpCode.Add(0x69, OpCodes.Conv_I4);

                            // conv.i8
                            _oneByteOpCodePrefixes.Add(0x6a);
                            _firstByteToOpCode.Add(0x6a, OpCodes.Conv_I8);

                            // conv.r4
                            _oneByteOpCodePrefixes.Add(0x6b);
                            _firstByteToOpCode.Add(0x6b, OpCodes.Conv_R4);

                            // conv.r8
                            _oneByteOpCodePrefixes.Add(0x6c);
                            _firstByteToOpCode.Add(0x6c, OpCodes.Conv_R8);

                            // conv.u4
                            _oneByteOpCodePrefixes.Add(0x6d);
                            _firstByteToOpCode.Add(0x6d, OpCodes.Conv_U4);

                            // conv.u8
                            _oneByteOpCodePrefixes.Add(0x6e);
                            _firstByteToOpCode.Add(0x6e, OpCodes.Conv_U8);

                            // callvirt
                            _oneByteOpCodePrefixes.Add(0x6f);
                            _firstByteToOpCode.Add(0x6f, OpCodes.Callvirt);

                            // cpobj
                            _oneByteOpCodePrefixes.Add(0x70);
                            _firstByteToOpCode.Add(0x70, OpCodes.Cpobj);

                            // ldobj
                            _oneByteOpCodePrefixes.Add(0x71);
                            _firstByteToOpCode.Add(0x71, OpCodes.Ldobj);

                            // ldstr
                            _oneByteOpCodePrefixes.Add(0x72);
                            _firstByteToOpCode.Add(0x72, OpCodes.Ldstr);

                            // newobj
                            _oneByteOpCodePrefixes.Add(0x73);
                            _firstByteToOpCode.Add(0x73, OpCodes.Newobj);

                            // castclass
                            _oneByteOpCodePrefixes.Add(0x74);
                            _firstByteToOpCode.Add(0x74, OpCodes.Castclass);

                            // isinst
                            _oneByteOpCodePrefixes.Add(0x75);
                            _firstByteToOpCode.Add(0x75, OpCodes.Isinst);

                            // conv.r.un
                            _oneByteOpCodePrefixes.Add(0x76);
                            _firstByteToOpCode.Add(0x76, OpCodes.Conv_R_Un);

                            // unbox
                            _oneByteOpCodePrefixes.Add(0x79);
                            _firstByteToOpCode.Add(0x79, OpCodes.Unbox);

                            // throw
                            _oneByteOpCodePrefixes.Add(0x7a);
                            _firstByteToOpCode.Add(0x7a, OpCodes.Throw);

                            // ldfld
                            _oneByteOpCodePrefixes.Add(0x7b);
                            _firstByteToOpCode.Add(0x7b, OpCodes.Ldfld);

                            // ldflda
                            _oneByteOpCodePrefixes.Add(0x7c);
                            _firstByteToOpCode.Add(0x7c, OpCodes.Ldflda);

                            // stfld
                            _oneByteOpCodePrefixes.Add(0x7d);
                            _firstByteToOpCode.Add(0x7d, OpCodes.Stfld);

                            // ldsfld
                            _oneByteOpCodePrefixes.Add(0x7e);
                            _firstByteToOpCode.Add(0x7e, OpCodes.Ldsfld);

                            // ldsflda
                            _oneByteOpCodePrefixes.Add(0x7f);
                            _firstByteToOpCode.Add(0x7f, OpCodes.Ldsflda);

                            // stsfld
                            _oneByteOpCodePrefixes.Add(0x80);
                            _firstByteToOpCode.Add(0x80, OpCodes.Stsfld);

                            // stobj
                            _oneByteOpCodePrefixes.Add(0x81);
                            _firstByteToOpCode.Add(0x81, OpCodes.Stobj);

                            // conv.ovf.i1.un
                            _oneByteOpCodePrefixes.Add(0x82);
                            _firstByteToOpCode.Add(0x82, OpCodes.Conv_Ovf_I1_Un);

                            // conv.ovf.i2.un
                            _oneByteOpCodePrefixes.Add(0x83);
                            _firstByteToOpCode.Add(0x83, OpCodes.Conv_Ovf_I2_Un);

                            // conv.ovf.i4.un
                            _oneByteOpCodePrefixes.Add(0x84);
                            _firstByteToOpCode.Add(0x84, OpCodes.Conv_Ovf_I4_Un);

                            // conv.ovf.i8.un
                            _oneByteOpCodePrefixes.Add(0x85);
                            _firstByteToOpCode.Add(0x85, OpCodes.Conv_Ovf_I8_Un);

                            // conv.ovf.u1.un
                            _oneByteOpCodePrefixes.Add(0x86);
                            _firstByteToOpCode.Add(0x86, OpCodes.Conv_Ovf_U1_Un);

                            // conv.ovf.u2.un
                            _oneByteOpCodePrefixes.Add(0x87);
                            _firstByteToOpCode.Add(0x87, OpCodes.Conv_Ovf_U2_Un);

                            // conv.ovf.u4.un
                            _oneByteOpCodePrefixes.Add(0x88);
                            _firstByteToOpCode.Add(0x88, OpCodes.Conv_Ovf_U4_Un);

                            // conv.ovf.u8.un
                            _oneByteOpCodePrefixes.Add(0x89);
                            _firstByteToOpCode.Add(0x89, OpCodes.Conv_Ovf_U8_Un);

                            // conv.ovf.i.un
                            _oneByteOpCodePrefixes.Add(0x8a);
                            _firstByteToOpCode.Add(0x8a, OpCodes.Conv_Ovf_I_Un);

                            // conv.ovf.u.un
                            _oneByteOpCodePrefixes.Add(0x8b);
                            _firstByteToOpCode.Add(0x8b, OpCodes.Conv_Ovf_U_Un);

                            // box
                            _oneByteOpCodePrefixes.Add(0x8c);
                            _firstByteToOpCode.Add(0x8c, OpCodes.Box);

                            // newarr
                            _oneByteOpCodePrefixes.Add(0x8d);
                            _firstByteToOpCode.Add(0x8d, OpCodes.Newarr);

                            // ldlen
                            _oneByteOpCodePrefixes.Add(0x8e);
                            _firstByteToOpCode.Add(0x8e, OpCodes.Ldlen);

                            // ldelema
                            _oneByteOpCodePrefixes.Add(0x8f);
                            _firstByteToOpCode.Add(0x8f, OpCodes.Ldelema);

                            // ldelem.i1
                            _oneByteOpCodePrefixes.Add(0x90);
                            _firstByteToOpCode.Add(0x90, OpCodes.Ldelem_I1);

                            // ldelem.u1
                            _oneByteOpCodePrefixes.Add(0x91);
                            _firstByteToOpCode.Add(0x91, OpCodes.Ldelem_U1);

                            // ldelem.i2
                            _oneByteOpCodePrefixes.Add(0x92);
                            _firstByteToOpCode.Add(0x92, OpCodes.Ldelem_I2);

                            // ldelem.u2
                            _oneByteOpCodePrefixes.Add(0x93);
                            _firstByteToOpCode.Add(0x93, OpCodes.Ldelem_U2);

                            // ldelem.i4
                            _oneByteOpCodePrefixes.Add(0x94);
                            _firstByteToOpCode.Add(0x94, OpCodes.Ldelem_I4);

                            // ldelem.u4
                            _oneByteOpCodePrefixes.Add(0x95);
                            _firstByteToOpCode.Add(0x95, OpCodes.Ldelem_U4);

                            // ldelem.i8
                            _oneByteOpCodePrefixes.Add(0x96);
                            _firstByteToOpCode.Add(0x96, OpCodes.Ldelem_I8);

                            // ldelem.i
                            _oneByteOpCodePrefixes.Add(0x97);
                            _firstByteToOpCode.Add(0x97, OpCodes.Ldelem_I);

                            // ldelem.r4
                            _oneByteOpCodePrefixes.Add(0x98);
                            _firstByteToOpCode.Add(0x98, OpCodes.Ldelem_R4);

                            // ldelem.r8
                            _oneByteOpCodePrefixes.Add(0x99);
                            _firstByteToOpCode.Add(0x99, OpCodes.Ldelem_R8);

                            // ldelem.ref
                            _oneByteOpCodePrefixes.Add(0x9a);
                            _firstByteToOpCode.Add(0x9a, OpCodes.Ldelem_Ref);

                            // stelem.i
                            _oneByteOpCodePrefixes.Add(0x9b);
                            _firstByteToOpCode.Add(0x9b, OpCodes.Stelem_I);

                            // stelem.i1
                            _oneByteOpCodePrefixes.Add(0x9c);
                            _firstByteToOpCode.Add(0x9c, OpCodes.Stelem_I1);

                            // stelem.i2
                            _oneByteOpCodePrefixes.Add(0x9d);
                            _firstByteToOpCode.Add(0x9d, OpCodes.Stelem_I2);

                            // stelem.i4
                            _oneByteOpCodePrefixes.Add(0x9e);
                            _firstByteToOpCode.Add(0x9e, OpCodes.Stelem_I4);

                            // stelem.i8
                            _oneByteOpCodePrefixes.Add(0x9f);
                            _firstByteToOpCode.Add(0x9f, OpCodes.Stelem_I8);

                            // stelem.r4
                            _oneByteOpCodePrefixes.Add(0xa0);
                            _firstByteToOpCode.Add(0xa0, OpCodes.Stelem_R4);

                            // stelem.r8
                            _oneByteOpCodePrefixes.Add(0xa1);
                            _firstByteToOpCode.Add(0xa1, OpCodes.Stelem_R8);

                            // stelem.ref
                            _oneByteOpCodePrefixes.Add(0xa2);
                            _firstByteToOpCode.Add(0xa2, OpCodes.Stelem_Ref);

                            // ldelem
                            _oneByteOpCodePrefixes.Add(0xa3);
                            _firstByteToOpCode.Add(0xa3, OpCodes.Ldelem);

                            // stelem
                            _oneByteOpCodePrefixes.Add(0xa4);
                            _firstByteToOpCode.Add(0xa4, OpCodes.Stelem);

                            // unbox.any
                            _oneByteOpCodePrefixes.Add(0xa5);
                            _firstByteToOpCode.Add(0xa5, OpCodes.Unbox_Any);

                            // conv.ovf.i1
                            _oneByteOpCodePrefixes.Add(0xb3);
                            _firstByteToOpCode.Add(0xb3, OpCodes.Conv_Ovf_I1);

                            // conv.ovf.u1
                            _oneByteOpCodePrefixes.Add(0xb4);
                            _firstByteToOpCode.Add(0xb4, OpCodes.Conv_Ovf_U1);

                            // conv.ovf.i2
                            _oneByteOpCodePrefixes.Add(0xb5);
                            _firstByteToOpCode.Add(0xb5, OpCodes.Conv_Ovf_I2);

                            // conv.ovf.u2
                            _oneByteOpCodePrefixes.Add(0xb6);
                            _firstByteToOpCode.Add(0xb6, OpCodes.Conv_Ovf_U2);

                            // conv.ovf.i4
                            _oneByteOpCodePrefixes.Add(0xb7);
                            _firstByteToOpCode.Add(0xb7, OpCodes.Conv_Ovf_I4);

                            // conv.ovf.u4
                            _oneByteOpCodePrefixes.Add(0xb8);
                            _firstByteToOpCode.Add(0xb8, OpCodes.Conv_Ovf_U4);

                            // conv.ovf.i8
                            _oneByteOpCodePrefixes.Add(0xb9);
                            _firstByteToOpCode.Add(0xb9, OpCodes.Conv_Ovf_I8);

                            // conv.ovf.u8
                            _oneByteOpCodePrefixes.Add(0xba);
                            _firstByteToOpCode.Add(0xba, OpCodes.Conv_Ovf_U8);

                            // refanyval
                            _oneByteOpCodePrefixes.Add(0xc2);
                            _firstByteToOpCode.Add(0xc2, OpCodes.Refanyval);

                            // ckfinite
                            _oneByteOpCodePrefixes.Add(0xc3);
                            _firstByteToOpCode.Add(0xc3, OpCodes.Ckfinite);

                            // mkrefany
                            _oneByteOpCodePrefixes.Add(0xc6);
                            _firstByteToOpCode.Add(0xc6, OpCodes.Mkrefany);

                            // ldtoken
                            _oneByteOpCodePrefixes.Add(0xd0);
                            _firstByteToOpCode.Add(0xd0, OpCodes.Ldtoken);

                            // conv.u2
                            _oneByteOpCodePrefixes.Add(0xd1);
                            _firstByteToOpCode.Add(0xd1, OpCodes.Conv_U2);

                            // conv.u1
                            _oneByteOpCodePrefixes.Add(0xd2);
                            _firstByteToOpCode.Add(0xd2, OpCodes.Conv_U1);

                            // conv.i
                            _oneByteOpCodePrefixes.Add(0xd3);
                            _firstByteToOpCode.Add(0xd3, OpCodes.Conv_I);

                            // conv.ovf.i
                            _oneByteOpCodePrefixes.Add(0xd4);
                            _firstByteToOpCode.Add(0xd4, OpCodes.Conv_Ovf_I);

                            // conv.ovf.u
                            _oneByteOpCodePrefixes.Add(0xd5);
                            _firstByteToOpCode.Add(0xd5, OpCodes.Conv_Ovf_U);

                            // add.ovf
                            _oneByteOpCodePrefixes.Add(0xd6);
                            _firstByteToOpCode.Add(0xd6, OpCodes.Add_Ovf);

                            // add.ovf.un
                            _oneByteOpCodePrefixes.Add(0xd7);
                            _firstByteToOpCode.Add(0xd7, OpCodes.Add_Ovf_Un);

                            // mul.ovf
                            _oneByteOpCodePrefixes.Add(0xd8);
                            _firstByteToOpCode.Add(0xd8, OpCodes.Mul_Ovf);

                            // mul.ovf.un
                            _oneByteOpCodePrefixes.Add(0xd9);
                            _firstByteToOpCode.Add(0xd9, OpCodes.Mul_Ovf_Un);

                            // sub.ovf
                            _oneByteOpCodePrefixes.Add(0xda);
                            _firstByteToOpCode.Add(0xda, OpCodes.Sub_Ovf);

                            // sub.ovf.un
                            _oneByteOpCodePrefixes.Add(0xdb);
                            _firstByteToOpCode.Add(0xdb, OpCodes.Sub_Ovf_Un);

                            // endfinally
                            _oneByteOpCodePrefixes.Add(0xdc);
                            _firstByteToOpCode.Add(0xdc, OpCodes.Endfinally);

                            // leave
                            _oneByteOpCodePrefixes.Add(0xdd);
                            _firstByteToOpCode.Add(0xdd, OpCodes.Leave);

                            // leave.s
                            _oneByteOpCodePrefixes.Add(0xde);
                            _firstByteToOpCode.Add(0xde, OpCodes.Leave_S);

                            // stind.i
                            _oneByteOpCodePrefixes.Add(0xdf);
                            _firstByteToOpCode.Add(0xdf, OpCodes.Stind_I);

                            // conv.u
                            _oneByteOpCodePrefixes.Add(0xe0);
                            _firstByteToOpCode.Add(0xe0, OpCodes.Conv_U);

                            // arglist
                            _twoByteOpCodePrefixes.Add(0xfe);
                            _secondByteToOpCode.Add(0x00, OpCodes.Arglist);

                            // ceq
                            _twoByteOpCodePrefixes.Add(0xfe);
                            _secondByteToOpCode.Add(0x01, OpCodes.Ceq);

                            // cgt
                            _twoByteOpCodePrefixes.Add(0xfe);
                            _secondByteToOpCode.Add(0x02, OpCodes.Cgt);

                            // cgt.un
                            _twoByteOpCodePrefixes.Add(0xfe);
                            _secondByteToOpCode.Add(0x03, OpCodes.Cgt_Un);

                            // clt
                            _twoByteOpCodePrefixes.Add(0xfe);
                            _secondByteToOpCode.Add(0x04, OpCodes.Clt);

                            // clt.un
                            _twoByteOpCodePrefixes.Add(0xfe);
                            _secondByteToOpCode.Add(0x05, OpCodes.Clt_Un);

                            // ldftn
                            _twoByteOpCodePrefixes.Add(0xfe);
                            _secondByteToOpCode.Add(0x06, OpCodes.Ldftn);

                            // ldvirtftn
                            _twoByteOpCodePrefixes.Add(0xfe);
                            _secondByteToOpCode.Add(0x07, OpCodes.Ldvirtftn);

                            // ldarg
                            _twoByteOpCodePrefixes.Add(0xfe);
                            _secondByteToOpCode.Add(0x09, OpCodes.Ldarg);

                            // ldarga
                            _twoByteOpCodePrefixes.Add(0xfe);
                            _secondByteToOpCode.Add(0x0a, OpCodes.Ldarga);

                            // starg
                            _twoByteOpCodePrefixes.Add(0xfe);
                            _secondByteToOpCode.Add(0x0b, OpCodes.Starg);

                            // ldloc
                            _twoByteOpCodePrefixes.Add(0xfe);
                            _secondByteToOpCode.Add(0x0c, OpCodes.Ldloc);

                            // ldloca
                            _twoByteOpCodePrefixes.Add(0xfe);
                            _secondByteToOpCode.Add(0x0d, OpCodes.Ldloca);

                            // stloc
                            _twoByteOpCodePrefixes.Add(0xfe);
                            _secondByteToOpCode.Add(0x0e, OpCodes.Stloc);

                            // localloc
                            _twoByteOpCodePrefixes.Add(0xfe);
                            _secondByteToOpCode.Add(0x0f, OpCodes.Localloc);

                            // endfilter
                            _twoByteOpCodePrefixes.Add(0xfe);
                            _secondByteToOpCode.Add(0x11, OpCodes.Endfilter);

                            // unaligned.
                            _twoByteOpCodePrefixes.Add(0xfe);
                            _secondByteToOpCode.Add(0x12, OpCodes.Unaligned);

                            // volatile.
                            _twoByteOpCodePrefixes.Add(0xfe);
                            _secondByteToOpCode.Add(0x13, OpCodes.Volatile);

                            // tail.
                            _twoByteOpCodePrefixes.Add(0xfe);
                            _secondByteToOpCode.Add(0x14, OpCodes.Tailcall);

                            // initobj
                            _twoByteOpCodePrefixes.Add(0xfe);
                            _secondByteToOpCode.Add(0x15, OpCodes.Initobj);

                            // constrained.
                            _twoByteOpCodePrefixes.Add(0xfe);
                            _secondByteToOpCode.Add(0x16, OpCodes.Constrained);

                            // cpblk
                            _twoByteOpCodePrefixes.Add(0xfe);
                            _secondByteToOpCode.Add(0x17, OpCodes.Cpblk);

                            // initblk
                            _twoByteOpCodePrefixes.Add(0xfe);
                            _secondByteToOpCode.Add(0x18, OpCodes.Initblk);

                            // rethrow
                            _twoByteOpCodePrefixes.Add(0xfe);
                            _secondByteToOpCode.Add(0x1a, OpCodes.Rethrow);

                            // sizeof
                            _twoByteOpCodePrefixes.Add(0xfe);
                            _secondByteToOpCode.Add(0x1c, OpCodes.Sizeof);

                            // refanytype
                            _twoByteOpCodePrefixes.Add(0xfe);
                            _secondByteToOpCode.Add(0x1d, OpCodes.Refanytype);

                            // readonly.
                            _twoByteOpCodePrefixes.Add(0xfe);
                            _secondByteToOpCode.Add(0x1e, OpCodes.Readonly);
                        }
                        finally
                        {
                            _staticCtorAintRun = false;
                        }
                    }
                }
            }
        }
    }
}
