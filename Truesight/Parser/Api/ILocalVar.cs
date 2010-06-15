using System;

namespace Truesight.Parser.Api
{
    public interface ILocalVar : IEquatable<ILocalVar>
    {
        IMethodBody Source { get; }

        int Index { get; }
        Type Type { get; }
    }
}