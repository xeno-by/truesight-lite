using System;
using System.Threading;
using Truesight.Playground.InAction.Domain;
using XenoGears.Functional;
using XenoGears.Assertions;
using System.Linq;

namespace Truesight.Playground.InAction.Runtimes
{
    internal partial class Runtime<T1, T2, T3> : IGridRunner<T1, T2, T3>
    {
        void IGridRunner<T1, T2, T3>.RunGrid(T1 arg1, T2 arg2, T3 arg3)
        {
            var crashCount = 0;

            var gridDims = new []{_grid.GridDim.Z, _grid.GridDim.Y, _grid.GridDim.X};
            var numBlocks = gridDims.Product();
            var workers = 0.UpTo(Environment.ProcessorCount - 1).Select(i => new Thread(() =>
            {
                var chunkSize = (int)Math.Ceiling(numBlocks * 1.0 / Environment.ProcessorCount - 1);
                var start = i * chunkSize;
                var end = Math.Min((i + 1) * chunkSize, numBlocks) - 1;

                start.UpTo(end).ForEach(j =>
                {
                    var dimSizes = gridDims.Scanrae(1, (curr, dim, _) => curr * dim).ToReadOnly();
                    var indices = dimSizes.SkipLast(1).Scanrbi(j, (curr, dimSize, _) => curr % dimSize, (curr, dimSize, _) => curr / dimSize, (curr, _) => curr).ToReadOnly();
                    var blid = new int3(indices[2], indices[1], indices[0]);

                    try
                    {
                        var blockRunner = _kernel.AssertCast<IBlockRunner<T1, T2, T3>>();
                        blockRunner.RunBlock(blid, arg1, arg2, arg3);
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref crashCount);
                        throw new RuntimeException<T1, T2, T3>(_kernel, _grid.GridDim, blid, _grid.BlockDim, null, Thread.CurrentThread.Name, ex);
                    }
                });
            }){Name = "Runtime worker thread #" + i}).ToReadOnly();

            workers.ForEach(w => w.Start());
            workers.ForEach(w => w.Join());
            (crashCount == 0).AssertTrue();
        }
    }
}
