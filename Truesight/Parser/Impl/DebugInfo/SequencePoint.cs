using System;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using Truesight.Parser.Api.DebugInfo;

namespace Truesight.Parser.Impl.DebugInfo
{
    // todo. support 0xfeefee semantics here
    // see http://blogs.msdn.com/jmstall/archive/2005/06/19/FeeFee_SequencePoints.aspx

    [DebuggerNonUserCode]
    internal class SequencePoint : ISequencePoint, IEquatable<SequencePoint>
    {
        public int ILOffset { get; private set; }
        public TextRun TextRun { get; private set; }
        ITextRun ISequencePoint.TextRun { get { return TextRun; } }

        public String SourceFile { get { return TextRun.SourceFile; } }
        public int StartLine { get { return TextRun.StartLine; } }
        public int StartColumn { get { return TextRun.StartColumn; } }
        public int EndLine { get { return TextRun.EndLine; } }
        public int EndColumn { get { return TextRun.EndColumn; } }

        public SequencePoint(int ilOffset, ISymbolDocument document, int startLine, int startColumn, int endLine, int endColumn)
        {
            ILOffset = ilOffset;
            TextRun = new TextRun(document.URL, startLine, startColumn, endLine, endColumn);
        }

        public bool Equals(SequencePoint other) { return Equals((ISequencePoint)this); }
        public bool Equals(ISequencePoint other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.ILOffset == ILOffset && Equals(other.TextRun, TextRun);
        }

        public override bool Equals(Object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (!(obj is ISequencePoint)) return false;
            return Equals((ISequencePoint)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ILOffset * 397) ^ (TextRun != null ? TextRun.GetHashCode() : 0);
            }
        }

        public static bool operator ==(SequencePoint left, SequencePoint right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SequencePoint left, SequencePoint right)
        {
            return !Equals(left, right);
        }

        public override String ToString()
        {
            return String.Format("{0} -> [{1}]", ILOffset, TextRun);
        }
    }
}