using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Truesight.Playground.InAction.Domain
{
    internal struct dim3 : IEnumerable<int>, IEquatable<dim3>
    {
        public int X;
        public int Y;
        public int Z;

        public dim3(int x) : this(x, 1, 1) { }
        public dim3(int x, int y) : this(x, y, 1) { }
        public dim3(int x, int y, int z) { X = x; Y = y; Z = z; }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        public IEnumerator<int> GetEnumerator() { return new[] { X, Y, Z }.Cast<int>().GetEnumerator(); }
        public override String ToString() { return String.Format("{0}{1}", "dim3", String.Format("({0}, {1}, {2})", X, Y, Z)); }

        public bool Equals(dim3 other)
        {
            return Equals(other.X, X) && Equals(other.Y, Y) && Equals(other.Z, Z);
        }

        public override bool Equals(Object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(dim3)) return false;
            return Equals((dim3)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = X.GetHashCode();
                result = (result * 397) ^ Y.GetHashCode();
                result = (result * 397) ^ Z.GetHashCode();
                return result;
            }
        }

        public static bool operator ==(dim3 left, dim3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(dim3 left, dim3 right)
        {
            return !left.Equals(right);
        }
    }
}
