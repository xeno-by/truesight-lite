using System;
using System.Diagnostics;
using Truesight.Parser.Api.DebugInfo;

namespace Truesight.Parser.Impl.DebugInfo
{
    [DebuggerNonUserCode]
    internal class TextRun : ITextRun, IEquatable<TextRun>
    {
        public String SourceFile { get; private set; }
        public int StartLine { get; private set; }
        public int StartColumn { get; private set; }
        public int EndLine { get; private set; }
        public int EndColumn { get; private set; }

        public TextRun(String sourceFile, int startLine, int startColumn, int endLine, int endColumn)
        {
            SourceFile = sourceFile;
            StartLine = startLine;
            StartColumn = startColumn;
            EndLine = endLine;
            EndColumn = endColumn;
        }

        public bool Equals(TextRun other) { return Equals((ITextRun)other); }
        public bool Equals(ITextRun other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.SourceFile, SourceFile) && other.StartLine == StartLine && other.StartColumn == StartColumn && other.EndLine == EndLine && other.EndColumn == EndColumn;
        }

        public override bool Equals(Object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (!(obj is ITextRun)) return false;
            return Equals((ITextRun)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (SourceFile != null ? SourceFile.GetHashCode() : 0);
                result = (result * 397) ^ StartLine;
                result = (result * 397) ^ StartColumn;
                result = (result * 397) ^ EndLine;
                result = (result * 397) ^ EndColumn;
                return result;
            }
        }

        public static bool operator ==(TextRun left, TextRun right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TextRun left, TextRun right)
        {
            return !Equals(left, right);
        }

        public override String ToString()
        {
            return String.Format(String.Format("{1}:{2} .. {3}:{4} at {0}", 
               SourceFile, StartLine, StartColumn, EndLine, EndColumn));
        }
    }
}