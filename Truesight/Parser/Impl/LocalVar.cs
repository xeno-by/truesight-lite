using System;
using System.Diagnostics;
using System.Reflection;
using XenoGears.Functional;
using XenoGears.Assertions;
using Truesight.Parser.Api;
using XenoGears.Strings;

namespace Truesight.Parser.Impl
{
    [DebuggerNonUserCode]
    internal class LocalVar : ILocalVar, IEquatable<LocalVar>
    {
        public MethodBody Source { get; private set; }
        IMethodBody ILocalVar.Source { get { return Source; } }

        public int Index { get; private set; }
        public Type Type { get; private set; }

        internal LocalVar(MethodBody source, LocalVariableInfo lvi)
        {
            Source = source.AssertNotNull();
            Index = lvi.AssertNotNull().LocalIndex;
            Type = lvi.AssertNotNull().LocalType;
        }

        public override String ToString()
        {
            var name = "loc" + Index;
            if (Source.DebugInfo != null) name = Source.DebugInfo.LocalNames.GetOrDefault(Index);
            return String.Format("{0} {1}", Type.GetCSharpRef(ToCSharpOptions.Informative), name);
        }

        #region Equality members

        public bool Equals(ILocalVar other)
        {
            return Equals(other as LocalVar);
        }

        public bool Equals(LocalVar other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Source, Source) && other.Index == Index;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(LocalVar)) return false;
            return Equals((LocalVar)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Source != null ? Source.GetHashCode() : 0) * 397) ^ Index;
            }
        }

        public static bool operator ==(LocalVar left, LocalVar right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(LocalVar left, LocalVar right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}