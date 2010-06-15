using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Truesight.Parser.Api;
using XenoGears;
using XenoGears.Reflection.Generics;
using XenoGears.Functional;
using XenoGears.Strings;
using NUnit.Framework;

namespace Truesight.Playground.Parser
{
    public abstract class Tests
    {
        protected void TestParseResult(IMethodBody mb)
        {
            var s_actual = mb.StringJoin(Environment.NewLine);
            s_actual = Regex.Replace(s_actual, @"<>c__DisplayClass.*::", "<Closure>::");
            s_actual = Regex.Replace(s_actual, @"CS\$<>8__locals.*\)", "<Closure>)");
            s_actual = Regex.Replace(s_actual, @"0x[a-fA-f0-9]{8}", "<MetadataToken>");

            var fnameWannabes = new List<String>();
            var ut = UnitTest.Current;
            var s_name = ut.Name;
            var s_sig = ut.Params().Select(p => p.GetCSharpRef(ToCSharpOptions.Informative).Replace("<", "[").Replace(">", "]").Replace("&", "!").Replace("*", "!")).StringJoin("_");
            var s_declt = ut.DeclaringType.GetCSharpRef(ToCSharpOptions.Informative).Replace("<", "[").Replace(">", "]").Replace("&", "!").Replace("*", "!");
            fnameWannabes.Add(s_name);
            fnameWannabes.Add(s_name + "_" + s_sig);
            fnameWannabes.Add(s_declt + "_" + s_name);
            fnameWannabes.Add(s_declt + "_" + s_name + "_" + s_sig);

            var asm = MethodInfo.GetCurrentMethod().DeclaringType.Assembly;
            var @namespace = MethodInfo.GetCurrentMethod().DeclaringType.Namespace + ".Reference.";
            var allResourceNames = asm.GetManifestResourceNames();
            var referenceFileName = fnameWannabes.SingleOrDefault2(wannabe =>
                allResourceNames.ExactlyOne(n => String.Compare(n, @namespace + wannabe, true) == 0));

            var success = false;
            String failMsg = null;
            if (referenceFileName != null)
            {
                String s_reference;
                using (var stream = asm.GetManifestResourceStream(@namespace + referenceFileName))
                {
                    s_reference = new StreamReader(stream).ReadToEnd();
                }

                if (s_reference.IsEmpty())
                {
                    Trace.WriteLine(s_actual);

                    Assert.Fail(String.Format(
                        "Reference parse result for the unit test '{1}' is empty.{0}" +
                        "Please, verify the trace dumped above and put in into the resource file.",
                        Environment.NewLine, ut.GetCSharpDecl(ToCSharpOptions.Informative)));
                }
                else
                {
                    var expected = s_reference.SplitLines();
                    var actual = s_actual.SplitLines();
                    if (expected.Count() != actual.Count())
                    {
                        failMsg = String.Format(
                            "Number of lines doesn't match. Expected {0}, actually found {1}" + Environment.NewLine,
                            expected.Count(), actual.Count());
                    }

                    success = expected.Zip(actual).SkipWhile((t, i) =>
                    {
                        if (t.Item1 != t.Item2)
                        {
                            var maxLines = Math.Max(actual.Count(), expected.Count());
                            var maxDigits = (int)Math.Floor(Math.Log10(maxLines)) + 1;
                            failMsg = String.Format(
                                "Line {1} (starting from 1) doesn't match.{0}{4}{2}{0}{5}{3}{0}",
                                Environment.NewLine, i + 1,
                                t.Item1.Replace(" ", "·"), t.Item2.Replace(" ", "·"),
                                "E:".PadRight(maxDigits + 3), "A:".PadRight(maxDigits + 3));
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }).IsEmpty();

                    if (!success)
                    {
                        Trace.WriteLine(String.Format(
//                            "Comparing actual and expected parse results{0}{1}",
                            "{1}",
                            Environment.NewLine, ut.GetCSharpDecl(ToCSharpOptions.InformativeWithDeclaringType)));
                        Trace.WriteLine(String.Empty);

                        var maxLines = Math.Max(actual.Count(), expected.Count());
                        var maxDigits = (int)Math.Floor(Math.Log10(maxLines)) + 1;
                        var maxActual = Math.Max(actual.MaxOrDefault(line => line.Length), "Actual".Length);
                        var maxExpected = Math.Max(expected.MaxOrDefault(line => line.Length), "Expected".Length);
                        var total = maxDigits + 3 + maxActual + 3 + maxExpected;

                        Trace.WriteLine(String.Format("{0} | {1} | {2}",
                            "N".PadRight(maxDigits),
                            "Actual".PadRight(maxActual),
                            "Expected".PadRight(maxExpected)));
                        Trace.WriteLine(total.Times("-"));

                        0.UpTo(maxLines - 1).ForEach(i =>
                        {
                            var l_actual = actual.ElementAtOrDefault(i, String.Empty);
                            var l_expected = expected.ElementAtOrDefault(i, String.Empty);
                            Trace.WriteLine(String.Format("{0} | {1} | {2}",
                                i.ToString().PadLeft(maxDigits),
                                l_actual.PadRight(maxActual),
                                l_expected.PadRight(maxExpected)));
                        });

                        Trace.WriteLine(String.Empty);
                        Trace.WriteLine(failMsg);
                        Trace.WriteLine(String.Empty);
                        Assert.Fail("Parse result doesn't match reference result.");
                    }
                }
            }
            else
            {
                Trace.WriteLine(s_actual);

                Assert.Fail(String.Format(
                    "Couldn't find a file in resource that contains reference parse result for the unit test '{1}'.{0}" +
                    "Please, verify the trace dumped above and put in into the one of the following files under the Reference folder next to the test suite: " +
                    fnameWannabes.Select(s => String.Format("\"{0}\"", s)).StringJoin(", ") + ".{0}" +
                    "Also be sure not to forget to select build action 'Embedded Resource' in file properties widget.",
                    Environment.NewLine, ut.GetCSharpDecl(ToCSharpOptions.Informative)));
            }
        }
    }
}
