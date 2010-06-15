using Truesight.Decompiler.Hir.Core.Functional;
using Truesight.Decompiler.Pipeline.Attrs;
using XenoGears.Reflection.Generics;
using Truesight.Parser;
using Truesight.Parser.Api;
using XenoGears.Assertions;

namespace Truesight.Decompiler.Pipeline.Cil
{
    [Decompiler(Weight = (int)Stages.ParseCIL)]
    internal static class DecodeAndLoadCIL
    {
        [DecompilationStep(Weight = 1)]
        public static IMethodBody DoLoadCIL(Context ctx)
        {
            var body = ctx.Method.ParseBody(ctx.Domain.Semantics.LoadDebugInfo);

            // todo. currently we don't support protected regions
            // even if ILParserApi will support them, we'll still need to implement those here
            body.RawEHC.AssertEmpty();

            // todo. ensure that no endfilter/endfinally ops are leaked here
            // this requires knowledge about how to iterate inflated patches
            // however, that API is still being implemented so no luck for now
            // thus, no code here for now until ILParseApi implements protected regions

            // todo. implement support for pointers and related syntactic constructs
            // e.g. review current addr/deref usage, support fixed, stackalloc and possible others
            // see information in t0do-deco.txt for more details
            body.Method.Params().AssertNone(p => p.IsPointer);
            body.Locals.AssertNone(l => l.Type.IsPointer);

            return body;
        }

        [DecompilationStep(Weight = 2)]
        public static Sig DoLoadSignature(Context ctx)
        {
            return new Sig(ctx.Method);
        }

        [DecompilationStep(Weight = 3)]
        public static Symbols DoLoadSymbols(Context ctx)
        {
            return new Symbols(ctx);
        }
    }
}