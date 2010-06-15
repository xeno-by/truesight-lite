using System;
using System.Diagnostics;
using Truesight.Parser.Api;

namespace Truesight.Parser.Impl
{
    [DebuggerNonUserCode]
    internal class Finally : Handler, IFinally, IEquatable<Finally>
    {
        internal Finally(Guard guard, Patch logic)
            : base(guard, logic)
        {
        }

        #region Equality members

        public bool Equals(IFinally other)
        {
            return Equals(other as Finally);
        }

        public bool Equals(Finally other)
        {
            return base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(Finally left, Finally right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Finally left, Finally right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}