using System;
using Truesight.Playground.InAction.Domain;

namespace Truesight.Playground.InAction.Runtime
{
    internal partial class Runtime<T1, T2, T3> : IKernel<T1, T2, T3>
    {
        private readonly IGrid _grid;
        private readonly IKernel<T1, T2, T3> _kernel;

        public Runtime(IGrid grid, Type t_kernel)
        {
            _grid = grid;
            _kernel = Jit(t_kernel);
        }

        dim3 IGridApi.GridDim { get { return _grid.GridDim; } }
        dim3 IGridApi.BlockDim { get { return _grid.BlockDim; } }
        int3 IGridApi.BlockIdx { get { throw new NotSupportedException("Calls to IGridApi.BlockIdx should have been crosscompiled by the runtime."); } }
        int3 IGridApi.ThreadIdx { get { throw new NotSupportedException("Calls to IGridApi.ThreadIdx should have been crosscompiled by the runtime."); } }
        void ISyncApi.SyncThreads(params Object[] keys) { throw new NotSupportedException("Calls to ISyncApi.SyncThreads should have been crosscompiled by the runtime."); }

        public void RunKernel(T1 arg1, T2 arg2, T3 arg3)
        {
            _kernel.RunKernel(arg1, arg2, arg3);
        }
    }
}