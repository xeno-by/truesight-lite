using System;
using System.Collections.ObjectModel;
using Truesight.Decompiler.Framework.Annotations;

namespace Truesight.Decompiler.Framework.Core
{
    internal interface IPipeline
    {
        String Name { get; set; }
        PipelineAttribute Metadata { get; }

        ReadOnlyCollection<IPipelineStep> Steps { get; }
        T Process<T>(T context);
    }
}