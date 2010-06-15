using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Reflection;
using XenoGears.Functional;
using XenoGears.Collections;
using Truesight.Parser.Api;
using Truesight.Parser.Api.DebugInfo;

namespace Truesight.Parser.Impl.DebugInfo
{
    [DebuggerNonUserCode]
    internal class MethodDebugInfo : IMethodDebugInfo
    {
        private IMethodBody Body { get; set; }
        public MethodBase Method { get { return Body.Method; } }

        public ReadOnlyCollection<SequencePoint> SequencePoints { get; private set; }
        ReadOnlyCollection<ISequencePoint> IMethodDebugInfo.SequencePoints { get { return SequencePoints.Cast<ISequencePoint>().ToReadOnly(); } }

        public ReadOnlyDictionary<int, String> LocalNames { get; private set; }
        ReadOnlyDictionary<int, String> IMethodDebugInfo.LocalNames { get { return LocalNames; } }
        public String this[ILocalVar local] { get { return LocalNames[local.Index]; } }

        public MethodDebugInfo(MethodBody body, ISymbolMethod pdb)
        {
            Body = body;

            // todo. support multiple locals sharing the same local slot 
            // (but having different Start::End offsets, of course)
            // so far we just silently ignore them taking into account only the first one
            var locals = pdb.RootScope.Flatten(ss => ss.GetChildren()).SelectMany(ss => ss.GetLocals());
            LocalNames = locals.Select(lv => new {Index = lv.AddressField1, Name = lv.Name})
                .Distinct().ToDictionary(lv => lv.Index, lv => lv.Name).ToReadOnly();

            var count = pdb.SequencePointCount;
            var offsets = new int[count];
            var documents = new ISymbolDocument[count];
            var lines = new int[count];
            var columns = new int[count];
            var endLines = new int[count];
            var endColumns = new int[count];
            pdb.GetSequencePoints(offsets, documents, lines, columns, endLines, endColumns);

            SequencePoints = 0.UpTo(count - 1).Select(i => 
                new SequencePoint(offsets[i], documents[i], lines[i], columns[i], endLines[i], endColumns[i])
            ).ToReadOnly();
        }

        ITextRun IMethodDebugInfo.this[int ilOffset] { get { return this[ilOffset]; } }
        public TextRun this[int ilOffset]
        {
            get
            {
                // todo. support 0xfeefee semantics here
                // see http://blogs.msdn.com/jmstall/archive/2005/06/19/FeeFee_SequencePoints.aspx

                if (ilOffset < 0 || Body.Last().OffsetOfNextOp <= ilOffset)
                {
                    return null;
                }
                else
                {
                    return SequencePoints.SlideOuter2().Skip(1).First(pair =>
                    {
                        var start1 = pair.Item1 == null ? int.MinValue : pair.Item1.ILOffset;
                        var start2 = pair.Item2 == null ? int.MinValue : pair.Item2.ILOffset;
                        return start1 <= ilOffset && ilOffset < start2;
                    }).Item1.TextRun;
                }
            }
        }
    }
}