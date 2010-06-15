using System.Diagnostics;
using System.Reflection;
using Truesight.Decompiler.Domains;
using Truesight.Decompiler.Hir.Core.Functional;

namespace Truesight.Decompiler
{
    [DebuggerNonUserCode]
    public static class DecompilerApi
    {
        public static Lambda Decompile(this MethodBase mb)
        {
#if DEBUG
            return mb.Decompile(Semantics.CSharp35_WithDebugInfo);
#else
            return mb.Decompile(Semantics.CSharp35_WithoutDebugInfo);
#endif
        }

        public static Lambda Decompile(this MethodBase mb, Language language)
        {
            return mb.Decompile(new Semantics(language, true));
        }

        public static Lambda Decompile(this Language language, MethodBase mb)
        {
            return mb.Decompile(new Semantics(language, true));
        }

        public static Lambda Decompile(this MethodBase mb, Semantics semantics)
        {
            var domain = new Domain(semantics);
            if (semantics == Domain.Current.Semantics) domain = Domain.Current;
            return mb.Decompile(domain);
        }

        public static Lambda Decompile(this Semantics semantics, MethodBase mb)
        {
            var domain = new Domain(semantics);
            if (semantics == Domain.Current.Semantics) domain = Domain.Current;
            return mb.Decompile(domain);
        }

        public static Lambda Decompile(this MethodBase mb, Domain domain)
        {
            var prev = Domain.Current;
            Domain.Current = domain;
            try { return new Lambda(mb, InvocationStyle.NonVirtual); }
            finally { Domain.Current = prev; }
        }

        public static Lambda Decompile(this Domain domain, MethodBase mb)
        {
            var prev = Domain.Current;
            Domain.Current = domain;
            try { return new Lambda(mb, InvocationStyle.NonVirtual); }
            finally { Domain.Current = prev; }
        }
    }
}