using System;
using System.Diagnostics;
using XenoGears.Assertions;
using Truesight.Parser.Api;

namespace Truesight.Parser.Impl
{
    [DebuggerNonUserCode]
    internal abstract class Handler : IHandler, IEquatable<Handler>
    {
        public IMethodBody Source { get { return Guard.Source; } }
        IGuard IHandler.Guard { get { return Guard; } }
        public Guard Guard { get; private set; }

        IPatch IHandler.Logic { get { return Logic; } }
        public Patch Logic { get; private set; }

        internal Handler(Guard guard, Patch logic)
        {
            Guard = guard.AssertNotNull();
            Logic = logic.AssertNotNull();
        }

        #region Equality members

        public bool Equals(IHandler other)
        {
            return Equals(other as Finally);
        }

        public bool Equals(Handler other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Guard, Guard) && Equals(other.Logic, Logic);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (this.GetType() != obj.GetType()) return false;
            return Equals((Handler)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Guard != null ? Guard.GetHashCode() : 0) * 397) ^ (Logic != null ? Logic.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Handler left, Handler right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Handler left, Handler right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}