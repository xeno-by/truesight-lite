using System;
using System.Diagnostics;
using Truesight.Decompiler.Framework.Annotations;

namespace Truesight.Decompiler.Pipeline.Attrs
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    [DebuggerNonUserCode]
    internal class DecompilationStepAttribute : PipelineStepAttribute
    {
    }
}