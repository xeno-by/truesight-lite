using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using XenoGears.Functional;
using XenoGears.Assertions;
using Truesight.Parser.Api;

namespace Truesight.Parser.Impl
{
    [DebuggerNonUserCode]
    internal class Patch : IPatch, IEquatable<Patch>
    {
        public MethodBody Source { get { return Parent.Source.AssertCast2<MethodBody>(); } }
        IMethodBody IPatch.Source { get { return Source; } }

        public IPatch Parent { get; private set; }
        IPatch IPatch.Parent { get { return Parent; } }

        public ReadOnlyCollection<ILOp> Ops { get; private set; }
        ReadOnlyCollection<IILOp> IPatch.Ops { get { return Ops.Cast<IILOp>().ToReadOnly(); } }
        IEnumerator IEnumerable.GetEnumerator() { return ((IEnumerable<IILOp>)this).GetEnumerator(); }
        IEnumerator<IILOp> IEnumerable<IILOp>.GetEnumerator() { return ((IPatch)this).GetEnumerator(); }

        public ReadOnlyCollection<Guard> Guards { get; private set; }
        ReadOnlyCollection<IGuard> IPatch.Guards { get { return Guards.Cast<IGuard>().ToReadOnly(); } }

        internal int Offset { get { return Ops.First().Offset; } }
        internal int Length { get { return Ops.Sum(op => op.Size); } }

        public ReadOnlyCollection<byte> RawIL { get { return Source.RawIL.Slice(Offset, Offset + Length).ToReadOnly(); } }
        public ReadOnlyCollection<ExceptionHandlingClause> RawEHC { get { throw new NotImplementedException(); } }

        public Patch(IPatch parent, ReadOnlyCollection<ILOp> ops, ReadOnlyCollection<Guard> guards)
        {
            Parent = parent.AssertNotNull();
            Ops = ops.AssertNotNull();
            Guards = guards.AssertNotNull();
        }

        #region Equality members

        public bool Equals(IPatch other)
        {
            return Equals(other as Patch);
        }

        public bool Equals(Patch other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Source, Source) && other.Offset == Offset && other.Length == Length;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Patch)) return false;
            return Equals((Patch)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Source != null ? Source.GetHashCode() : 0);
                result = (result * 397) ^ Offset;
                result = (result * 397) ^ Length;
                return result;
            }
        }

        public static bool operator ==(Patch left, Patch right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Patch left, Patch right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}