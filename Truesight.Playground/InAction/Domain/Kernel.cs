using System;
using System.Runtime.CompilerServices;

namespace Truesight.Playground.InAction.Domain
{
    internal abstract class Kernel<T1, T2, T3> : IKernel<T1, T2, T3>
    {
        dim3 IGridApi.GridDim { get { return GridDim; } }
        extern protected virtual dim3 GridDim { [MethodImpl(MethodImplOptions.InternalCall)] get; }

        int3 IGridApi.BlockIdx { get { return BlockIdx; } }
        extern protected virtual int3 BlockIdx { [MethodImpl(MethodImplOptions.InternalCall)] get; }

        dim3 IGridApi.BlockDim { get { return BlockDim; } }
        extern protected virtual dim3 BlockDim { [MethodImpl(MethodImplOptions.InternalCall)] get; }

        int3 IGridApi.ThreadIdx { get { return ThreadIdx; } }
        extern protected virtual int3 ThreadIdx { [MethodImpl(MethodImplOptions.InternalCall)] get; }

        void ISyncApi.SyncThreads(params Object[] keys) { SyncThreads(keys); }
        [MethodImpl(MethodImplOptions.InternalCall)]
        extern protected virtual void SyncThreads(params Object[] keys);

        public abstract void RunKernel(T1 arg1, T2 arg2, T3 arg3);
    }
}