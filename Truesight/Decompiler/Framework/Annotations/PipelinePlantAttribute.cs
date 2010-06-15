using System;
using System.Diagnostics;

namespace Truesight.Decompiler.Framework.Annotations
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    [DebuggerNonUserCode]
    internal abstract class PipelinePlantAttribute : PipelineElementAttribute
    {
        protected PipelinePlantAttribute()
        {
            LogEnabled = true;
        }
    }
}
