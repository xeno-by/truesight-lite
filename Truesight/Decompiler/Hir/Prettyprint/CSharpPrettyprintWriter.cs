using System.Diagnostics;
using System.IO;

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
