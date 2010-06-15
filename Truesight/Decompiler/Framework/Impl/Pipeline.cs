using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using XenoGears.Functional;
using Truesight.Decompiler.Framework.Annotations;
using Truesight.Decompiler.Framework.Core;

namespace Truesight.Decompiler.Framework.Impl
{
    [DebuggerNonUserCode]
    internal class Pipeline : IPipeline
    {
        public String Name { get; set; }
        public PipelineAttribute Metadata { get; private set; }
        public ReadOnlyCollection<PipelineStep> Steps { get; private set; }
        ReadOnlyCollection<IPipelineStep> IPipeline.Steps { get { return Steps.Cast<IPipelineStep>().ToReadOnly(); } }

        public Pipeline(String name, PipelineAttribute metadata, ReadOnlyCollection<PipelineStep> steps)
        {
            Name = name;
            Metadata = metadata;
            Steps = steps;
        }

        public T Process<T>(T context)
        {
            var compiled = this.CompilePipeline<T>();
            return compiled(context);
        }
    }
}