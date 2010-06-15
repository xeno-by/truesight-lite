using System;
using System.Reflection;
using System.Reflection.Emit;
using Truesight.Parser.Api;

namespace Truesight.Playground.Parser.ILRewriter
{
    internal static class ILTrait
    {
        // that's ugly, but it's the best I can come with without writing full-fledged CCI analogue
        // todo #1. This doesn't take account of all intricacies of ILGenerator
        // e.g. ain't update maxstack, tokenfixups and God knows what else
        // todo #2. Honestly the offsetsmode and fixup stuff is so complex
        // that I'd better just use mutable IILOps model, but don't have time to implement it right now

        [Obsolete("Before using this think twice and proceed only if you understand: "+
            "1) why IILRewriteControl has OffsetsMode, "+
            "2) how do you specify an original offset for a newly generated branch "+
            "(note: calling EnterOriginalOffsetsMode is not enough).")]
        public static IILRewriterContext RewriteInto(this MethodBase src, MethodBuilder dest,
            Func<IILOp, IILRewriteControl, IILRewriteControl> logic)
        {
            var rewriter = new ILRewriter(src, dest, logic);
            rewriter.DoRewrite();
            return rewriter;
        }
    }
}
