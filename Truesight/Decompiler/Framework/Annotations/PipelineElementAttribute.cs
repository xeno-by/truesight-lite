using System;
using System.Diagnostics;
using XenoGears.Assertions;
using XenoGears.Reflection.Attributes.Weight;

namespace Truesight.Decompiler.Framework.Annotations
{
    [DebuggerNonUserCode]
    internal abstract class PipelineElementAttribute : WeightedAttribute
    {
        // note. unlike other names this name can be null
        // in that case for plants and workshops it will be ignored
        // in that case for steps it will be taken from the annotated method
        public String Name { get; set; }

        public virtual bool LogDisabled
        {
            get { return !_logEnabled; }
            set
            {
                value.AssertTrue(); 
                _logEnabled = !value;
            }
        }

        private bool _logEnabled = true;
        public virtual bool LogEnabled
        {
            get { return _logEnabled; }
            set
            {
                value.AssertTrue();
                _logEnabled = value;
            }
        }
    }
}