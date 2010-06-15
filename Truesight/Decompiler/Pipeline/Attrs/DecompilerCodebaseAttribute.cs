using System;
using System.Diagnostics;
using Truesight.Decompiler.Framework.Annotations;

namespace Truesight.Decompiler.Pipeline.Attrs
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    [DebuggerNonUserCode]
    internal class DecompilerCodebaseAttribute : PipelinePlantAttribute
    {
    }
}