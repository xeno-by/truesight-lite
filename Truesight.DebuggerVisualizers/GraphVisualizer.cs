using System;
using System.Diagnostics;
using System.IO;
using Truesight.DebuggerVisualizers;
using Truesight.Decompiler.Pipeline.Flow.Cfg;
using Microsoft.VisualStudio.DebuggerVisualizers;
using XenoGears.Assertions;
using XenoGears.DebuggerVisualizers.DumpableAsText;

[assembly: DebuggerVisualizer(typeof(GraphVisualizer), typeof(GraphSource), Target = typeof(BaseControlFlowGraph), Description = "CFG Visualizer")]
[assembly: DebuggerVisualizer(typeof(DumpableAsTextVisualizer), typeof(DumpableAsTextSource), Target = typeof(ControlFlowBlock), Description = "CFG Vertex Visualizer")]
[assembly: DebuggerVisualizer(typeof(DumpableAsTextVisualizer), typeof(DumpableAsTextSource), Target = typeof(ControlFlowEdge), Description = "CFG Edge Visualizer")]

namespace Truesight.DebuggerVisualizers
{
    public class GraphVisualizer : DialogDebuggerVisualizer
    {
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            var cfgInDotFormat = objectProvider.GetObject().AssertCast<String>();
            var dotFile = Path.GetTempFileName();
            File.WriteAllText(dotFile, cfgInDotFormat);

            var pngFile = dotFile.RenderAsPng();
            if (pngFile != null)
            {
                Process.Start(pngFile.FullName);
            }
        }
    }
}