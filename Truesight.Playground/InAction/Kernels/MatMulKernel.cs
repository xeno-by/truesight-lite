using System;
using Truesight.Playground.InAction.Domain;
using XenoGears.Functional;

namespace Truesight.Playground.InAction.Kernels
{
    internal abstract class MatMulKernel : Kernel<float[,], float[,], float[,]>
    {
        protected override void RunKernel(float[,] a, float[,] b, float[,] c)
        {
            var row = BlockIdx.Y * BlockDim.Y + ThreadIdx.Y;
            var col = BlockIdx.X * BlockDim.X + ThreadIdx.X;
            // this is necessary in case when matrix dims ain't multiples of block dims
            if (a.Height() <= row || b.Width() <= col) return;

            var c_value = 0f;
            for (var dim = 0; dim < a.Width(); ++dim)
            {
                c_value += a[row, dim] * b[dim, col];
            }

            c[row, col] = c_value;
        }
    }
}