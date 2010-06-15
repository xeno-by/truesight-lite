using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Truesight.Decompiler.Hir;
using Truesight.Decompiler.Hir.Core.Functional;
using XenoGears.Collections.Weak;

namespace Truesight.Decompiler.Domains
{
    [DebuggerNonUserCode]
    public partial class Domain
    {
        public Semantics Semantics { get; private set; }

        // todo. design and implement cache eviction algorithm
        internal Dictionary<MethodBase, Lambda> DecompilationCache { get; private set; }
        internal WeakKeyDictionary<Node, String> DumpAsTextCache { get; private set; }
        internal WeakKeyDictionary<Node, Type> TypeInferenceCache { get; private set; }

        public Domain()
#if DEBUG
            : this(Semantics.CSharp35_WithDebugInfo)
#else
            : this(Semantics.CSharp35_WithoutDebugInfo)
#endif
        {
        }

        public Domain(Semantics semantics)
        {
            Semantics = semantics;

            DecompilationCache = new Dictionary<MethodBase, Lambda>();
            DumpAsTextCache = new WeakKeyDictionary<Node, String>();
            TypeInferenceCache = new WeakKeyDictionary<Node, Type>();
        }

        private readonly Guid _uniqueId = Guid.NewGuid();
        public override String ToString()
        {
            return String.Format("{0}, id={1}", Semantics, _uniqueId);
        }
    }
}