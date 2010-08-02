using System;
using System.Collections.Generic;

namespace Truesight.Playground
{
    internal class TestRunner
    {
        public static void Main(String[] args)
        {
            // see more details at http://www.nunit.org/index.php?p=consoleCommandLine&r=2.5.5
            var nunitArgs = new List<String>();
            nunitArgs.Add("/run:Truesight.Playground.AdHoc");
//            nunitArgs.Add("/run:Truesight.Playground.Decompiler");
//            nunitArgs.Add("/run:Truesight.Playground.Parser");
//            nunitArgs.Add("/run:Truesight.Playground.InAction");
            nunitArgs.Add("/include:Hot");
            nunitArgs.Add("/domain:None");
            nunitArgs.Add("/noshadow");
            nunitArgs.Add("/nologo");
            nunitArgs.Add("Truesight.Playground.exe");
            NUnit.ConsoleRunner.Runner.Main(nunitArgs.ToArray());
        }
    }
}
