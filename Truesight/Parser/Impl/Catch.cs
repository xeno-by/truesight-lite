using System;
using System.Diagnostics;
using XenoGears.Assertions;
using Truesight.Parser.Api;

namespace Truesight.Parser.Impl
{
    [DebuggerNonUserCode]
    internal class Catch : Handler, ICatch, IEquatable<Catch>
    {
        public Type Exception { get; set; }
        IPatch ICatch.Filter { get { return Filter; } }
        public Patch Filter { get; set; }

        internal Catch(Guard guard, Patch logic)
            : this(guard, logic, null, null)
        {
        }

        internal Catch(Guard guard, Patch logic, Type exception)
            : this(guard, logic, exception, null)
        {
        }

        internal Catch(Guard guard, Patch logic, Type exception, Patch filter)
            : base(guard, logic)
        {
            Exception = exception;
            Filter = filter;
        }

        #region Equality members

        public bool Equals(ICatch other)
        {
            return Equals(other as Catch);
        }

        public bool Equals(Catch other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return ((Handler)this).Equals((Handler)other) &&
                Equals(other.Exception, Exception) && Equals(other.Filter, Filter);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Catch)) return false;
            return Equals((Catch)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return base.GetHashCode() * 397 ^
                    ((Exception != null ? Exception.GetHashCode() : 0) * 397) ^
                    (Filter != null ? Filter.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Catch left, Catch right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Catch left, Catch right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}