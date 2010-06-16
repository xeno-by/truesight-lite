using System;

namespace Truesight.Playground.InAction.Domain
{
    internal static class Hints
    {
        public static SharingHint Sharing { get { return new SharingHint(); } }

        public class SharingHint
        {
            internal SharingHint() {}

            public SharingHint Private(params Object[] vars)
            {
                throw new NotSupportedException();
            }

            public SharingHint Local(params Object[] vars)
            {
                throw new NotSupportedException();
            }

            public SharingHint Global(params Object[] vars)
            {
                throw new NotSupportedException();
            }
        }
    }
}