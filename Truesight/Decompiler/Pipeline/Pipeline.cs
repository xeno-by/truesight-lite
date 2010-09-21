using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Truesight.Decompiler.Domains;
using Truesight.Decompiler.Hir;
using Truesight.Decompiler.Hir.Core.Functional;
using Truesight.Decompiler.Hir.Traversal;
using Truesight.Decompiler.Pipeline.Attrs;
using Truesight.Decompiler.Pipeline.Cil;
using Truesight.Decompiler.Pipeline.Flow;
using Truesight.Decompiler.Pipeline.Hir;
using Truesight.Parser.Api.DebugInfo;
using XenoGears;
using Truesight.Decompiler.Framework.Annotations;
using XenoGears.Collections.Dictionaries;
using XenoGears.Functional;
using XenoGears.Reflection.Attributes;
using XenoGears.Traits.Dumpable;

namespace Truesight.Decompiler.Pipeline
{
    internal class Pipeline
    {
        private readonly Domain _domain;

        public Pipeline(Domain domain)
        {
            _domain = domain;
        }

        [Pipeline(
            Name = "CIL to HIR Decompilation",
            PlantMarker = typeof(DecompilerCodebaseAttribute),
            WorkshopMarker = typeof(DecompilerAttribute),
            StepMarker = typeof(DecompilationStepAttribute))]
        public Lambda Process(MethodBase method)
        {
            // todo. turn this back on when it will have reasonable performance
            // here we need to implement logically correct and performant caching policy,
            // that doesn't have penalties for development mode
            // e.g. that doesn't stupidly rebuild pipeline after every rebuild

//            var metadata = MethodInfo.GetCurrentMethod().Attr<PipelineAttribute>();
//            var pipeline = PipelineFramework.BuildPipeline(metadata);
//            pipeline.Name = String.Format("{0}_{1}", method.DeclaringType.Name, method.Name);
//
//            var ctx = new Context(_semantics, method);
//            pipeline.Process(ctx);
//            return new Lambda(ctx.Method, ctx.Sig, ctx.Body);

            var ctx = new Context(_domain, method);
            var pipelineName = String.Format("{0}_{1}", method.DeclaringType.Name, method.Name);
            CIL_to_HIR_Decompilation.Process(pipelineName, ctx);
            return new Lambda(ctx.Method, ctx.Sig, ctx.Body){Domain = _domain};
        }

        internal class CIL_to_HIR_Decompilation
        {
            public static Context Process(String nameOfPipeline, Context ctx)
            {
                Func<ReadOnlyDictionary<Node, ITextRun>> srcs = () => ctx.Cfg.Vertices[2].BalancedCode[0].Family().Where(n => n != null && n.Src != null).ToDictionary(n => n, n => n.Src).ToReadOnly();
                Func<ReadOnlyDictionary<Node, Node>> protos = () => ctx.Cfg.Vertices[2].BalancedCode[0].Family().Where(n => n != null && n.Proto != null).ToDictionary(n => n, n => n.Proto).ToReadOnly();

                ctx.Cil = DecodeAndLoadCIL.DoLoadCIL(ctx);
                ctx.Sig = DecodeAndLoadCIL.DoLoadSignature(ctx);
                ctx.Symbols = DecodeAndLoadCIL.DoLoadSymbols(ctx);
                ctx.Cfg = BuildControlFlowGraph.DoBuildControlFlowGraph(ctx.Cil, ctx.Symbols);
                DumpAsText(ctx.Cfg, nameOfPipeline, "After BuildControlFlowGraph");
                RestoreObjectInitializers.DoRestoreObjectInitializers(ctx.Cfg);
                RestoreCollectionInitializers.DoRestoreCollectionInitializers(ctx.Cfg);
                RestoreOpAssignOperators.DoRestoreOpAssignOperators(ctx.Cfg, ctx.Symbols);
//                DumpAsText(ctx.Cfg, nameOfPipeline, "Before RestoreConditionalOperators");
                RestoreConditionalOperators.DoRestoreConditionalOperators(ctx.Cfg);
                RestoreCoalesceOperators.DoRestoreCoalesceOperators(ctx.Cfg);
                StripOffRedundancies.DoStripOffRedundancies(ctx.Cfg);
                EvictUnreachableCode.DoEvictUnreachableCode(ctx.Cfg);
                NormalizeEdgeTags.DoNormalizeEdgeTags(ctx.Cfg);
//                DumpAsText(ctx.Cfg, nameOfPipeline, "Before DecompileComplexConditions");
                DecompileComplexConditions.DoDecompileComplexConditions(ctx.Cfg);
                RemoveReturnThunk.DoRemoveReturnThunk(ctx.Cfg);
                SplitBlocksIntoAssignmentsAndPredicates.DoSplitBlocksIntoAssignmentsAndPredicates(ctx.Cfg);
                ctx.Body = DecompileScopes.DoDecompileScopes(ctx.Cfg);
                DecompileScopes.DoInferScopesForLocals(ctx);
                InferLoopIters.DoInferLoopIters(ctx.Body);
                ctx.Body = RestoreBooleans.DoRestoreBooleans(ctx.Body);
                ctx.Body = RestoreEnums.DoRestoreEnums(ctx.Body);
                ctx.Body = RestoreTypeIs.DoRestoreTypeIs(ctx.Body);
                ctx.Body = RestoreUsings.DoRestoreUsings(ctx.Body);
                ctx.Body = RestoreIters.DoRestoreIters(ctx.Body);
                ctx.Body = SimplifyConditions1.DoSimplifyConditions(ctx.Body);
                return ctx;
            }

            private static void DumpAsText<T>(T subject, String nameOfPipeline, String nameOfMoment) 
                where T: IDumpableAsText
            {
                var path = @".\CIL to HIR Decompilation\";
                var str2 = ((String.IsNullOrEmpty(nameOfPipeline) ? Guid.NewGuid().ToString() : nameOfPipeline) + " - ") + nameOfMoment + (String.IsNullOrEmpty(UnitTest.PersistentId) ? null : (", " + UnitTest.PersistentId));
                var str3 = (subject == null) ? null : ((String) ((subject.GetType().AttrOrNull<DumpFormatAttribute>() == null) ? null : subject.GetType().AttrOrNull<DumpFormatAttribute>().DefaultExtension));
                var str4 = path + str2 + "." + str3;
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                File.WriteAllText(str4, subject.DumpAsText());
            }
        }
    }
}
