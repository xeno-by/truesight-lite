using System.Reflection;
using Truesight.Decompiler.Framework.Annotations;

namespace Truesight.Decompiler.Framework.Core
{
    internal interface IPipelineStep
    {
        PipelinePlantAttribute PlantMetadata { get; }
        PipelineWorkshopAttribute WorkshopMetadata { get; }
        PipelineStepAttribute StepMetadata { get; }
        MethodInfo Code { get;}
    }
}