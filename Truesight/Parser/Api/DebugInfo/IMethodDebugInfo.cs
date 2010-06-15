using System;
using System.Collections.ObjectModel;
using System.Reflection;
using XenoGears.Collections;

namespace Truesight.Parser.Api.DebugInfo
{
    public interface IMethodDebugInfo
    {
        MethodBase Method { get; }

        ReadOnlyCollection<ISequencePoint> SequencePoints { get; }
        ITextRun this[int ilOffset] { get; }

        ReadOnlyDictionary<int, String> LocalNames { get; }
        String this[ILocalVar local] { get; }
    }
}