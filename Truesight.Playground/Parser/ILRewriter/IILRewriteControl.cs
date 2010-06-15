using System;
using System.Reflection.Emit;

namespace Truesight.Playground.Parser.ILRewriter
{
    internal interface IILRewriteControl
    {
        IILRewriteControl Clone();
        IILRewriteControl Rewrite(Action<ILGenerator> rewriter);
        IILRewriteControl StripOff();

        OffsetsMode OffsetsMode { get; set; }
        void EnterOriginalOffsetsMode();
        void EnterRewrittenOffsetsMode();
    }
}