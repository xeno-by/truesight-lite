using System;
using System.Collections.Generic;
using System.Diagnostics;
using XenoGears.Collections.Dictionaries;
using XenoGears.Functional;

namespace Truesight.Decompiler.Hir.TypeInference
{
    [DebuggerNonUserCode]
    public class TypeInferenceCache : ReadOnlyDictionary<Node, Type>
    {
        public TypeInferenceCache(ReadOnlyDictionary<Node, Type> impl) : base(impl) {}
        public static implicit operator TypeInferenceCache(Dictionary<Node, Type> impl) { return new TypeInferenceCache(impl.ToReadOnly()); }

        public override bool TryGetValue(Node key, out Type value)
        {
            if (key == null)
            {
                value = null;
                return true;
            }
            else
            {
                return base.TryGetValue(key, out value);
            }
        }
    }
}