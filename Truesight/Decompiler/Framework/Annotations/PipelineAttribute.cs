using System;
using System.Diagnostics;
using XenoGears.Assertions;

namespace Truesight.Decompiler.Framework.Annotations
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [DebuggerNonUserCode]
    internal class PipelineAttribute : Attribute
    {
        private String _name;
        public String Name
        {
            get { return _name; }
            set
            {
                value.AssertNotNull();
                _name = value;
            }
        }

        private Type _plantMarker;
        public Type PlantMarker
        {
            get { return _plantMarker; }
            set
            {
                value.AssertNotNull();
                (typeof(PipelinePlantAttribute).IsAssignableFrom(value)).AssertTrue();
                _plantMarker = value;
            }
        }

        private Type _workshopMarker;
        public Type WorkshopMarker
        {
            get { return _workshopMarker; }
            set
            {
                value.AssertNotNull();
                (typeof(PipelineWorkshopAttribute).IsAssignableFrom(value)).AssertTrue();
                _workshopMarker = value;
            }
        }

        private Type _stepMarker;
        public Type StepMarker
        {
            get { return _stepMarker; }
            set
            {
                value.AssertNotNull();
                (typeof(PipelineStepAttribute).IsAssignableFrom(value)).AssertTrue();
                _stepMarker = value;
            }
        }
    }
}