using System.Diagnostics;
using System.IO;
using XenoGears.Strings.Writers;

namespace Truesight.Decompiler.Hir.Prettyprint
{
    [DebuggerNonUserCode]
    public class CSharpPrettyprintWriter : IndentedWriter
    {
        public CSharpPrettyprintWriter(TextWriter writer)
            : base(writer)
        {
        }
    }
}
