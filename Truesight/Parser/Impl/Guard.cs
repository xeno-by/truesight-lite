using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Truesight.Parser.Api;
using XenoGears.Functional;

namespace Truesight.Parser.Impl
{
    [DebuggerNonUserCode]
    internal class Guard : IGuard, IEquatable<Guard>
    {
        public IMethodBody Source { get; private set; }
        IMethodBody IGuard.Source { get { return Source; } }

        public IPatch Guarded { get; private set; }
        IPatch IGuard.Guarded { get { return Guarded; } }

        public ReadOnlyCollection<Handler> OnException { get; private set; }
        ReadOnlyCollection<IHandler> IGuard.OnException { get { return OnException.Cast<IHandler>().ToReadOnly(); } }

        public ReadOnlyCollection<Finally> OnFinally { get; private set; }
        ReadOnlyCollection<IFinally> IGuard.OnFinally { get { return OnException.Cast<IFinally>().ToReadOnly(); } }

        public Guard(IMethodBody source, IPatch guarded, ReadOnlyCollection<Handler> onException, ReadOnlyCollection<Finally> onFinally)
        {
            Source = source;
            Guarded = guarded;
            OnException = onException;
            OnFinally = onFinally;
        }

        #region Equality members

        public bool Equals(IGuard other)
        {
            return Equals(other as Guard);
        }

        public bool Equals(Guard other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Source, Source) && Equals(other.Guarded, Guarded) && Equals(other.OnException, OnException) && Equals(other.OnFinally, OnFinally);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Guard)) return false;
            return Equals((Guard)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Source != null ? Source.GetHashCode() : 0);
                result = (result * 397) ^ (Guarded != null ? Guarded.GetHashCode() : 0);
                result = (result * 397) ^ (OnException != null ? OnException.GetHashCode() : 0);
                result = (result * 397) ^ (OnFinally != null ? OnFinally.GetHashCode() : 0);
                return result;
            }
        }

        public static bool operator ==(Guard left, Guard right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Guard left, Guard right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}