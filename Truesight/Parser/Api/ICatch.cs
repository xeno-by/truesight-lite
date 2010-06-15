using System;

namespace Truesight.Parser.Api
{
    public interface ICatch : IHandler, IEquatable<ICatch>
    {
        Type Exception { get; }
        IPatch Filter { get; }
    }
}