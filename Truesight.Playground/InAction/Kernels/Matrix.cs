using System;

namespace Truesight.Playground.InAction.Kernels
{
    internal class Matrix<T>
    {
        private readonly T[,] _arr;
        public int Height { get { return _arr.GetLength(0); } }
        public int Width { get { return _arr.GetLength(1); } }

        public Matrix(int height, int width)
        {
            _arr = new T[height, width];
        }

        public Matrix (int height, int width, Func<int, int, T> initializer)
            : this(height, width)
        {
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    _arr[i, j] = initializer(i, j);
                }
            }
        }

        public Matrix(T[,] arr)
        {
            _arr = arr;
        }

        public T this[int row, int col]
        {
            get { return _arr[row, col]; }
            set { _arr[row, col] = value; }
        }

        public static implicit operator Matrix<T>(T[,] arr)
        {
            return new Matrix<T>(arr);
        }

        public static implicit operator T[,](Matrix<T> matrix)
        {
            return matrix._arr;
        }
    }
}