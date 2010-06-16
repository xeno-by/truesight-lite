using XenoGears.Assertions;

namespace Truesight.Playground.InAction.Kernels
{
    internal class SubMatrix<T>
    {
        private readonly Matrix<T> _adaptee;
        private readonly int _top;
        private readonly int _left;
        private readonly int _height;
        private readonly int _width;
        public int Height { get { return _height; } }
        public int Width { get { return _width; } }

        public SubMatrix(Matrix<T> adaptee, int top, int left, int height, int width)
        {
            (adaptee.Height >= top + height).AssertTrue();
            (adaptee.Width >= left + width).AssertTrue();

            _adaptee = adaptee;
            _top = top;
            _left = left;
            _height = height;
            _width = width;
        }

        protected int Dim(int dim)
        {
            if (dim == 0) return _height;
            if (dim == 1) return _width;
            throw AssertionHelper.Fail();
        }

        public T this[int row, int col]
        {
            get
            {
                (_height > row).AssertTrue();
                (_width > col).AssertTrue();
                return _adaptee[_top + row, _left + col];
            }

            set
            {
                (_height > row).AssertTrue();
                (_width > col).AssertTrue();
                _adaptee[_top + row, _left + col] = value;
            }
        }
    }
}