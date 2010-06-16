using NUnit.Framework;
using Truesight.Playground.InAction.Domain;
using Truesight.Playground.InAction.Kernels;
using Truesight.Playground.InAction.Runtimes;

namespace Truesight.Playground.InAction
{
    [TestFixture]
    public partial class Tests
    {
        [Test]
        public void MatMulKernel()
        {
            var a = RandMatrix(16, 20);
            var b = RandMatrix(20, 16);
            var c = ReferenceMul(a, b);

            var c_kernel = new float[16, 16];
            var grid = new Grid{BlockDim = new dim3(4, 4, 1), GridDim = new dim3(4, 4, 1)};
            var runtime = new Runtime<float[,], float[,], float[,]>(grid, typeof(MatMulKernel));
            runtime.RunKernel(a, b, c_kernel);
            AssertAreTheSame(c, c_kernel);
        }

        [Test]
        public void MatMulKernel_Fast()
        {
            var a = RandMatrix(16, 20);
            var b = RandMatrix(20, 16);
            var c = ReferenceMul(a, b);

            var c_kernel = new float[16, 16];
            var grid = new Grid { BlockDim = new dim3(4, 4, 1), GridDim = new dim3(4, 4, 1) };
            var runtime = new Runtime<float[,], float[,], float[,]>(grid, typeof(MatMulKernel_Fast));
            runtime.RunKernel(a, b, c_kernel);
            AssertAreTheSame(c, c_kernel);
        }
    }
}
