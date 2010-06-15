using System;

namespace Truesight.Parser.Api.DebugInfo
{
    public interface ITextRun : IEquatable<ITextRun>
    {
        String SourceFile { get; }
        int StartLine { get; }
        int StartColumn { get; }
        int EndLine { get; }
        int EndColumn { get; }
    }
}