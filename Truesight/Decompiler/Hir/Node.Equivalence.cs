using System;
using System.Collections.Generic;
using System.Diagnostics;
using XenoGears.Functional;
using XenoGears.Traits.Equivatable;

namespace Truesight.Decompiler.Hir
{
    public abstract partial class Node : IEquivatable<Node>
    {
        protected virtual bool EigenEquiv(Node node) { return node != null && GetType() == node.GetType(); }
        bool IEquivatable<Node>.Equiv(Node other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (!EigenEquiv(other)) return false;
            return Children.AllMatch(other.Children, (ct, co) => ct.Equiv(co));
        }

        protected virtual int EigenHashCode() { return 0; }
        int IEquivatable<Node>.EquivHashCode()
        {
            return Children.Fold(EigenHashCode(), (h, n) => h ^ (n == null ? 0 : n.EquivHashCode()));
        }

        public static IEqualityComparer<Node> DefaultComparer() { return DefaultComparer<Node>(); }
        public static IEqualityComparer<T> DefaultComparer<T>() 
            where T : Node
        {
            return new EqualityComparer<T>(Equals, n => n.GetHashCode());
        }

        public static IEqualityComparer<Node> EquivComparer() { return EquivComparer<Node>(); }
        public static IEqualityComparer<T> EquivComparer<T>() 
            where T : Node
        {
            return new EqualityComparer<T>((n1, n2) => n1.Equiv(n1), n => n.EquivHashCode());
        }

        [DebuggerNonUserCode]
        private class EqualityComparer<T> : IEqualityComparer<T>
        {
            private readonly Func<T, T, bool> _equalityComparer;
            private readonly Func<T, int> _hasher;

            public EqualityComparer(Func<T, T, bool> equalityComparer, Func<T, int> hasher)
            {
                _equalityComparer = equalityComparer;
                _hasher = hasher;
            }

            public static IEqualityComparer<T> Default
            {
                get { return System.Collections.Generic.EqualityComparer<T>.Default; }
            }

            public bool Equals(T x, T y)
            {
                return _equalityComparer(x, y);
            }

            public int GetHashCode(T obj)
            {
                return _hasher(obj);
            }
        }
    }
}
