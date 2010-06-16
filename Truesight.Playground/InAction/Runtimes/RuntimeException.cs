using System;
using Truesight.Playground.InAction.Domain;
using XenoGears.Assertions;

namespace Truesight.Playground.InAction.Runtimes
{
    internal class RuntimeException<T1, T2, T3> : Exception
    {
        public IKernel<T1, T2, T3> Kernel { get; private set; }
        public dim3? GridDim { get; private set; }
        public int3? BlockIdx { get; private set; }
        public dim3? BlockDim { get; private set; }
        public int3? ThreadIdx { get; private set; }
        public String WorkerThread { get; private set; }

        public RuntimeException(IKernel<T1, T2, T3> kernel, IGridApi gridApi, String workerThread, Exception innerException)
            : this(kernel, gridApi.GridDim, gridApi.BlockIdx, gridApi.BlockDim, gridApi.BlockIdx, workerThread, innerException)
        {
        }

        public RuntimeException(IKernel<T1, T2, T3> kernel, dim3? gridDim, int3? blockIdx, dim3? blockDim, int3? threadIdx, String workerThread, Exception innerException)
            : base(null, innerException)
        {
            Kernel = kernel.AssertNotNull();
            GridDim = gridDim;
            BlockIdx = blockIdx;
            BlockDim = blockDim;
            ThreadIdx = threadIdx;
            WorkerThread = workerThread.AssertNotNull();
        }

        public override String Message
        {
            get 
            {
                return String.Format(
                    "A fatal error has occurred when running kernel thread{0}" +
                    "Kernel: {1}{0}"+
                    "Logical thread: BlockIdx = {2} of {3}, ThreadIdx = {4} of {5}{0}" +
                    "Physical thread: {6}{0}" +
                    "A short description of error: " + InnerException.GetType().Name + ": " + InnerException.Message,
                    Environment.NewLine, 
                    Kernel.GetType().FullName, 
                    BlockIdx == null ? "N/A" : BlockIdx.ToString(),
                    GridDim == null ? "N/A" : GridDim.ToString(),
                    ThreadIdx == null ? "N/A" : ThreadIdx.ToString(),
                    BlockDim == null ? "N/A" : BlockDim.ToString(),
                    WorkerThread);
            }
        }
    }
}