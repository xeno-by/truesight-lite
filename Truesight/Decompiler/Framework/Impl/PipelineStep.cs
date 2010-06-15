using System;
using System.Diagnostics;
using System.Reflection;
using Truesight.Decompiler.Framework.Annotations;
using Truesight.Decompiler.Framework.Core;
using XenoGears.Strings;

namespace Truesight.Decompiler.Framework.Impl
{
    [DebuggerNonUserCode]
    internal class PipelineStep : IPipelineStep, IComparable<PipelineStep>
    {
        public PipelinePlantAttribute PlantMetadata { get; private set; }
        public PipelineWorkshopAttribute WorkshopMetadata { get; private set; }
        public PipelineStepAttribute StepMetadata { get; private set; }
        public MethodInfo Code { get; private set; }

        public PipelineStep(PipelinePlantAttribute plantMetadata, PipelineWorkshopAttribute workshopMetadata, PipelineStepAttribute stepMetadata, MethodInfo code)
        {
            PlantMetadata = plantMetadata;
            WorkshopMetadata = workshopMetadata;
            StepMetadata = stepMetadata;
            Code = code;
        }

        private double Weight { get { return PlantMetadata.Weight + WorkshopMetadata.Weight + StepMetadata.Weight; } }
        public int CompareTo(PipelineStep other) { return Weight.CompareTo(other.Weight); }

        public override String ToString()
        {
            return Code.GetCSharpDecl(ToCSharpOptions.Informative);
        }
    }
}