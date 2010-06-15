using System.Collections.Generic;
using System.Collections.ObjectModel;
using Truesight.Parser.Api;

namespace Truesight.Playground.Parser.ILRewriter
{
    internal interface IILRewriterContext
    {
        HashSet<IILOp> ClonedOps { get; }
        Dictionary<IILOp, ReadOnlyCollection<IILOp>> RewrittenOps { get; }

        int OrigToRewritten(int origOffset);
        int RewrittenToOrig(int rewrittenOffset);
        int RewrittenToNearestOrig(int rewrittenOffset);
    }
}