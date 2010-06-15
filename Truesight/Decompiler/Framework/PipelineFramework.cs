using System;
using System.Diagnostics;
using System.Reflection;
using Truesight.Decompiler.Framework.Annotations;
using Truesight.Decompiler.Framework.Core;
using Truesight.Decompiler.Framework.Impl;
using XenoGears.Reflection.Attributes;

namespace Truesight.Decompiler.Framework
{
    [DebuggerNonUserCode]
    internal static class PipelineFramework
    {
        public static IPipeline BuildPipeline(ICustomAttributeProvider annotatedMedium)
        {
            return BuildPipeline(annotatedMedium, null);
        }

        public static IPipeline BuildPipeline(PipelineAttribute metadata)
        {
            return BuildPipeline(metadata, null);
        }

        public static IPipeline BuildPipeline(ICustomAttributeProvider annotatedMedium, String name)
        {
            var metadata = annotatedMedium.Attr<PipelineAttribute>();
            return BuildPipeline(metadata, name);
        }

        public static IPipeline BuildPipeline(PipelineAttribute metadata, String name)
        {
            var steps = Registry.For(metadata);
            return new Impl.Pipeline(name, metadata, steps);
        }
    }
}
