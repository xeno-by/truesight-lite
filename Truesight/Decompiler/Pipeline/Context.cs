using System.Diagnostics;
using System.Reflection;
using Truesight.Decompiler.Domains;
using Truesight.Decompiler.Hir.Core.ControlFlow;
using Truesight.Decompiler.Hir.Core.Functional;
using Truesight.Decompiler.Pipeline.Flow.Cfg;
using Truesight.Parser.Api;

namespace Truesight.Decompiler.Pipeline
{
    [DebuggerNonUserCode]
    internal class Context
    {
        public Domain Domain { get; private set; }
        public Semantics Semantics { get { return Domain.Semantics; } }
        public Language Language { get { return Semantics.Language; } }

        public MethodBase Method { get; private set; }
        public IMethodBody Cil { get; set; }
        public Sig Sig { get; set; }
        public Symbols Symbols { get; set; }

        public ControlFlowGraph Cfg { get; set; }
        public Block Body { get; set; }

        public Context(Domain domain, MethodBase method)
        {
            Domain = domain;
            Method = method;
        }
    }
}
