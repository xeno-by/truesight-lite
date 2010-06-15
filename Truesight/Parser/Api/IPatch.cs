using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Truesight.Parser.Api
{
    public interface IPatch : IEnumerable<IILOp>, IEquatable<IPatch>
    {
        IMethodBody Source { get; }
        IPatch Parent { get; }

        ReadOnlyCollection<IILOp> Ops { get; }
        ReadOnlyCollection<IGuard> Guards { get; }

        ReadOnlyCollection<byte> RawIL { get; }
        ReadOnlyCollection<ExceptionHandlingClause> RawEHC { get; }
    }
}