using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Truesight.Parser.Impl.PdbReader
{
    [DebuggerNonUserCode]
    internal static class NativeMethods
    {
        [DllImport("ole32.dll")]
        internal static extern int CoCreateInstance(
            [In] ref Guid rclsid,
            [In, MarshalAs(UnmanagedType.IUnknown)] Object pUnkOuter,
            [In] uint dwClsContext,
            [In] ref Guid riid,
            [Out, MarshalAs(UnmanagedType.Interface)] out Object ppv);
    }
}