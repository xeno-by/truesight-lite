using System;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Runtime.InteropServices;

namespace Truesight.Parser.Impl.PdbReader
{
    [DebuggerNonUserCode]
    internal static class LowLevelApi
    {
        public static ISymbolReader GetSymReader(this Assembly asm)
        {
            return asm.GetModules().Single().GetSymReader();
        }

        public static ISymbolReader GetSymReader(this Module module)
        {
            object objDispenser;
            var dispenserClsid = new Guid(0xe5cb7a31, 0x7512, 0x11d2, 0x89, 0xce, 0x00, 0x80, 0xc7, 0x92, 0xe5, 0xd8);    // CLSID_CorMetaDataDispenser
            var dispenserIid = new Guid(0x809c652e, 0x7396, 0x11d2, 0x97, 0x71, 0x00, 0xa0, 0xc9, 0xb4, 0xd5, 0x0c);        // IID_IMetaDataDispenser
            NativeMethods.CoCreateInstance(ref dispenserClsid, null, 1, ref dispenserIid, out objDispenser);
            var dispenser = (IMetaDataDispenserPrivate)objDispenser;

            try
            {
                object objImporter;
                var importerIid = new Guid(0x7dac8207, 0xd3ae, 0x4c75, 0x9b, 0x67, 0x92, 0x80, 0x1a, 0x49, 0x7d, 0x44);         // IID_IMetaDataImport
                dispenser.OpenScope(module.Name, 0, ref importerIid, out objImporter);

                var importerPtr = IntPtr.Zero;
                ISymbolReader reader;
                try
                {
                    importerPtr = Marshal.GetComInterfaceForObject(objImporter, typeof(IMetadataImportPrivateComVisible));
                    reader = new SymBinder().GetReader(importerPtr, module.Name, null);
                }
                finally
                {
                    if (importerPtr != IntPtr.Zero)
                    {
                        Marshal.Release(importerPtr);
                    }
                }

                return reader;
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        public static ISymbolMethod GetSymReader(this MethodBase method)
        {
            var reader = method.Module.GetSymReader();
            if (reader == null)
            {
                return null;
            }
            else
            {
                var token = new SymbolToken(method.MetadataToken);
                return reader.GetMethod(token);
            }
        }
    }
}