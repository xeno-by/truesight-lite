using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Truesight.Decompiler.Pipeline.Flow.Cfg;
using Microsoft.VisualStudio.DebuggerVisualizers;
using XenoGears.Assertions;
using XenoGears.Traits.Dumpable;

namespace Truesight.DebuggerVisualizers
{
    public class GraphSource : VisualizerObjectSource
    {
        public override void GetData(Object target, Stream outgoingData)
        {
            var cfg = target.AssertCast<BaseControlFlowGraph>();
            new BinaryFormatter().Serialize(outgoingData, cfg.DumpAsText());
        }

        public override void TransferData(Object target, Stream incomingData, Stream outgoingData)
        {
            throw new NotImplementedException();
        }

        public override Object CreateReplacementObject(Object target, Stream incomingData)
        {
            throw new NotImplementedException();
        }
    }
}