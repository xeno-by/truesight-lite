using System;
using System.Runtime.CompilerServices;

namespace Truesight.Playground.InAction.Domain
{
    internal static class Hints
    {
        public static SharingHint Sharing { get { return new SharingHint(); } }

        public class SharingHint
        {
            internal SharingHint() { }

            [MethodImpl(MethodImplOptions.InternalCall)]
            extern public SharingHint Private(params Object[] vars);

            [MethodImpl(MethodImplOptions.InternalCall)]
            extern public SharingHint Local(params Object[] vars);

            [MethodImpl(MethodImplOptions.InternalCall)]
            extern public SharingHint Global(params Object[] vars);
        }
    }
}