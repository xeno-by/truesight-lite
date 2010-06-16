using System;
using Truesight.Playground.InAction.Domain;
using XenoGears.Assertions;
using XenoGears.Functional;

namespace Truesight.Playground.InAction.Kernels
{
    internal abstract class MatMulKernel_Fast : Kernel<float[,], float[,], float[,]>
    {
        protected override void RunKernel(float[,] a, float[,] b, float[,] c)
        {
            (BlockDim.X == BlockDim.Y).AssertTrue();
            var c_value = 0f;

            for (var i = 0; i < (int)Math.Ceiling(1f * a.Width() / BlockDim.X); ++i) 
            {
                var asub = Submatrix(a, BlockIdx.Y, i);
                var asub_shared = new float[BlockDim.X, BlockDim.X];
                var idle_a = asub.Height <= ThreadIdx.Y || asub.Width <= ThreadIdx.X;
                if (idle_a) asub_shared[ThreadIdx.Y, ThreadIdx.X] = 0;
                else asub_shared[ThreadIdx.Y, ThreadIdx.X] = asub[ThreadIdx.Y, ThreadIdx.X];
                Hints.Sharing.Local(asub_shared);

                var bsub = Submatrix(b, i, BlockIdx.X);
                var bsub_shared = new float[BlockDim.X, BlockDim.X];
                var idle_b = bsub.Height <= ThreadIdx.Y || bsub.Width <= ThreadIdx.X;
                if (idle_b) bsub_shared[ThreadIdx.Y, ThreadIdx.X] = 0;
                else bsub_shared[ThreadIdx.Y, ThreadIdx.X] = bsub[ThreadIdx.Y, ThreadIdx.X];
                Hints.Sharing.Local(bsub_shared);

                SyncThreads();

                var stripLen = Math.Min(a.Width() - i * BlockDim.X, BlockDim.X);
                for (var j = 0; j < stripLen; ++j)
                {
                    c_value += asub_shared[ThreadIdx.Y, j] * bsub_shared[j, ThreadIdx.X];
                }

                SyncThreads();
            }

            var csub = Submatrix(c, BlockIdx.Y, BlockIdx.X);
            if (csub.Height > ThreadIdx.Y && csub.Width > ThreadIdx.X) 
                csub[ThreadIdx.Y, ThreadIdx.X] = c_value;
        }

        protected SubMatrix<float> Submatrix(Matrix<float> m, int blockRow, int blockCol)
        {
            var top = blockRow * BlockDim.Y;
            var left = blockCol * BlockDim.X;
            var height = Math.Min(BlockDim.Y, m.Height - top);
            var width = Math.Min(BlockDim.X, m.Width - left);
            return new SubMatrix<float>(m, top, left, height, width);
        }
    }
}