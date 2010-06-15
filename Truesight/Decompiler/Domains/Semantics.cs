using System;
using System.Diagnostics;

namespace Truesight.Decompiler.Domains
{
    [DebuggerNonUserCode]
    public class Semantics : IEquatable<Semantics>
    {
        public Language Language { get; private set; }
        public bool LoadDebugInfo { get; private set; }

        public Semantics(Language language)
            : this(language, false)
        {
        }

        public Semantics(Language language, bool loadDebugInfo)
        {
            Language = language;
            LoadDebugInfo = loadDebugInfo;
        }

        public static Semantics CSharp35_WithoutDebugInfo
        {
            get { return new Semantics(Language.CSharp35, false); }
        }

        public static Semantics CSharp35_WithDebugInfo
        {
            get { return new Semantics(Language.CSharp35, true); }
        }

        public bool Equals(Semantics other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Language, Language) && other.LoadDebugInfo.Equals(LoadDebugInfo);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Semantics)) return false;
            return Equals((Semantics)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Language.GetHashCode() * 397) ^ LoadDebugInfo.GetHashCode();
            }
        }

        public static bool operator ==(Semantics left, Semantics right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Semantics left, Semantics right)
        {
            return !Equals(left, right);
        }

        public override String ToString()
        {
            return String.Format("{0}, {1}", Language, LoadDebugInfo ? "debug" : "no debug");
        }
    }
}