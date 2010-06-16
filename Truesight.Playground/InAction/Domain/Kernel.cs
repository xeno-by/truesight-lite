using System;

namespace Truesight.Playground.InAction.Domain
{
    internal abstract class Kernel<T1, T2, T3> : IKernel<T1, T2, T3>
    {
        dim3 IGridApi.GridDim { get { return GridDim; } }
        protected virtual dim3 GridDim { get { throw new NotSupportedException(); } }

        int3 IGridApi.BlockIdx { get { return BlockIdx; } }
        protected virtual int3 BlockIdx { get { throw new NotSupportedException(); } }

        dim3 IGridApi.BlockDim { get { return BlockDim; } }
        protected virtual dim3 BlockDim { get { throw new NotSupportedException(); } }

        int3 IGridApi.ThreadIdx { get { return ThreadIdx; } }
        protected virtual int3 ThreadIdx { get { throw new NotSupportedException(); } }

        void ISyncApi.SyncThreads(params Object[] keys) { SyncThreads(keys); }
        protected virtual void SyncThreads(params Object[] keys) { throw new NotSupportedException(); }

        public abstract void RunKernel(T1 arg1, T2 arg2, T3 arg3);
    }
}