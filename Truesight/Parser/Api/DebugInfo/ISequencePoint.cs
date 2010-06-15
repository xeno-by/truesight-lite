using System;

namespace Truesight.Parser.Api.DebugInfo
{
    public interface ISequencePoint : IEquatable<ISequencePoint>
    {
        int ILOffset { get; }
        ITextRun TextRun { get; }

        String SourceFile { get; }
        int StartLine { get; }
        int StartColumn { get; }
        int EndLine { get; }
        int EndColumn { get; }
    }
}