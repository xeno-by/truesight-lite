using System;
using System.Collections.ObjectModel;
using System.Reflection;
using Truesight.Parser.Api.DebugInfo;

namespace Truesight.Parser.Api
{
    public interface IMethodBody : IPatch, IEquatable<IMethodBody>
    {
        Module Module { get; }
        Type Type { get; }
        MethodBase Method { get; }
        IMethodDebugInfo DebugInfo { get; }

        ReadOnlyCollection<ParameterInfo> Args { get; }
        // todo. Ret might be null both for an unknown method and for ctor
        // fix this ambiguity (by possibly forging a PI for a ctor)
        // but I don't have time to do this now
        ParameterInfo Ret { get; }
        ReadOnlyCollection<ILocalVar> Locals { get; }
    }
}
