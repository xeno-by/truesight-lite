using System;
using System.Text.RegularExpressions;
using Truesight.Parser.Api;
using XenoGears.Playground.Framework;
using XenoGears.Functional;

namespace Truesight.Playground.Parser
{
    public abstract class Tests : BaseTests
    {
        protected void VerifyResult(IMethodBody mb)
        {
            var s_actual = mb.StringJoin(Environment.NewLine);
            s_actual = Regex.Replace(s_actual, @"<>c__DisplayClass.*::", "<Closure>::");
            s_actual = Regex.Replace(s_actual, @"CS\$<>8__locals.*\)", "<Closure>)");
            s_actual = Regex.Replace(s_actual, @"0x[a-fA-f0-9]{8}", "<MetadataToken>");
            VerifyResult(s_actual);
        }
    }
}
