using System;
using System.Diagnostics;
using Truesight.Decompiler.Hir.Traversal.Traversers;
using XenoGears.Functional;

namespace Truesight.Decompiler.Hir.TypeInference
{
    [DebuggerNonUserCode]
    public static class TypeInferenceApi
    {
        public static Type InferType(this Node n)
        {
            if (n == null) return null;

            var cached = n.Domain == null ? null : n.Domain.TypeInferenceCache.GetOrDefault(n);
            if (cached != null) return cached;
            else
            {
                // caching of this very invocation is performed within the traverser
                return n.Traverse<TypeInferenceTraverser>().Types[n];
            }
        }

        public static TypeInferenceCache InferTypes(this Node n)
        {
            if (n == null) return null;
            return n.Traverse<TypeInferenceTraverser>().Types;
        }
    }
}