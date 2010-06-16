using System;

namespace Truesight.Playground.InAction.Domain
{
    internal abstract class Kernel<T1, T2, T3> : IKernel<T1, T2, T3>
    {
        public virtual dim3 GridDim { get { throw new NotSupportedException(); } }
        public virtual int3 BlockIdx { get { throw new NotSupportedException(); } }
        public virtual dim3 BlockDim { get { throw new NotSupportedException(); } }
        public virtual int3 ThreadIdx { get { throw new NotSupportedException(); } }
        public virtual void SyncThreads(params Object[] keys) { throw new NotSupportedException(); }

        public abstract void RunKernel(T1 arg1, T2 arg2, T3 arg3);
    }
}