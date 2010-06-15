using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Truesight.Decompiler.Pipeline.Attrs;

[assembly: AssemblyProduct("Truesight")]
[assembly: AssemblyTitle("Truesight")]
[assembly: AssemblyCopyright("Copyright © 2009-2010 xeno.by")]

[assembly: ComVisible(false)]
[assembly: DecompilerCodebase]
[assembly: InternalsVisibleTo("Truesight.DebuggerVisualizers")]
[assembly: InternalsVisibleTo("Truesight.Playground")]
[assembly: InternalsVisibleTo("Truesight.TextGenerators")]
[assembly: InternalsVisibleTo("Truesight.Decompiler.Pipeline")]

[assembly: Guid("5f2fcd01-9556-4a8b-ba3a-811699e4b988")]
[assembly: AssemblyVersion("0.0.0.0")]
