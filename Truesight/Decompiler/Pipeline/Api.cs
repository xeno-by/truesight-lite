using System.Diagnostics;
using Truesight.Decompiler.Domains;

namespace Truesight.Decompiler.Pipeline
{
    [DebuggerNonUserCode]
    internal static class Api
    {
        public static Pipeline CreatePipeline(this Domain domain)
        {
            return new Pipeline(domain);
        }
    }
}