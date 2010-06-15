using System;
using System.Collections.ObjectModel;

namespace Truesight.Parser.Api
{
    public interface IGuard : IEquatable<IGuard>
    {
        IMethodBody Source { get; }
        IPatch Guarded { get; }

        ReadOnlyCollection<IHandler> OnException { get; }
        ReadOnlyCollection<IFinally> OnFinally { get; }
    }
}