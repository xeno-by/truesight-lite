using System;

namespace Truesight.Parser.Api
{
    public interface IHandler : IEquatable<IHandler>
    {
        IMethodBody Source { get; }
        IGuard Guard { get; }

        IPatch Logic { get; }
    }
}