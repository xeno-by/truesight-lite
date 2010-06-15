using System;
using System.Diagnostics;

namespace Truesight.Decompiler.Framework.Annotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    [DebuggerNonUserCode]
    internal abstract class PipelineWorkshopAttribute : PipelineElementAttribute
    {
        protected PipelineWorkshopAttribute()
        {
            LogEnabled = true;
        }
    }
}