using System.Diagnostics;
using System.IO;
using XenoGears.Strings;

namespace Truesight.Decompiler.Hir.Prettyprint
{
    [DebuggerNonUserCode]
    public class CSharpPrettyprintWriter : IndentedTextWriter
    {
        public CSharpPrettyprintWriter(TextWriter writer)
            : base(writer)
        {
        }
    }
}
