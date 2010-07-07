using System;
using System.Diagnostics;
using XenoGears.Functional;
using XenoGears.Assertions;

namespace Truesight.Playground.InAction
{
    public partial class Tests
    {
        private float[,] RandMatrix(int height, int width)
        {
            var rng = new Random((int)DateTime.Now.Ticks);
            Func<int> rand = () => rng.Next() % 3;
//            Func<int> rand = () => 1;

            var matrix = new float[height, width];
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    matrix[i, j] = rand();
                }
            }

            return matrix;
        }

        private void PrintMatrix(String headline, float[,] m)
        {
            Trace.Write(headline + Environment.NewLine + m.StringJoin() + Environment.NewLine);
        }

        private void AssertAreTheSame(float[,] a, float[,] b)
        {
            var haveSameDims = a.Height() == b.Height() && a.Width() == b.Width();
            var areTheSame = haveSameDims ? ((Func<bool>)(() =>
            {
                for (var i = 0; i < a.Height(); i++)
                {
                    for (var j = 0; j < a.Width(); j++)
                    {
                        if (a[i, j] != b[i, j]) return false;
                    }
                }

                return true;
            }))() : false;
            if (!areTheSame)
            {
                Trace.WriteLine("*".Repeat(120));
                Trace.WriteLine("ERROR! Calculated matrix ain't equal to reference result.");
                Trace.WriteLine(String.Empty);

                PrintMatrix("Expected: ", a);
                Trace.WriteLine(String.Empty);
                PrintMatrix("Actual: ", b);
                throw AssertionHelper.Fail();
            }
        }

        private float[,] ReferenceMul(float[,] a, float[,] b)
        {
            var dim = a.Width().AssertThat(_ => a.Width() == b.Height());
            int a_height = a.Height(), b_width = b.Width();
            var c = new float[a_height, b_width];

            for (var i = 0; i < a_height; ++i)
            {
                for (var j = 0; j < b_width; ++j)
                {
                    var c_value = 0f;
                    for (var k = 0; k < dim; ++k)
                    {
                        c_value += a[i, k] * b[k, j];
                    }

                    c[i, j] = c_value;
                }
            }

            return c;
        }
    }
}
