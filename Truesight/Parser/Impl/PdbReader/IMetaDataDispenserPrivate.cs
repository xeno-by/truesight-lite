using System;
using System.Runtime.InteropServices;

namespace Truesight.Parser.Impl.PdbReader
{
    [Guid("809c652e-7396-11d2-9771-00a0c9b4d50c"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    internal interface IMetaDataDispenserPrivate
    {
        void VTable_Stub();

        void OpenScope(
            [In, MarshalAs(UnmanagedType.LPWStr)] String szScope, 
            [In] Int32 dwOpenFlags, 
            [In] ref Guid riid, 
            [Out, MarshalAs(UnmanagedType.IUnknown)] out Object punk);
    }
}