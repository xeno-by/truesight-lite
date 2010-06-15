using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Truesight.Decompiler.Hir.Core.ControlFlow;
using Truesight.Decompiler.Hir.Core.Functional;
using Truesight.Decompiler.Hir.Core.Special;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Reflection;
using XenoGears.Reflection.Attributes;

namespace Truesight.Decompiler.Hir
{
    [DebuggerNonUserCode]
    public static class NodeDebuggabilityHelper
    {
        public static String InferDebugProxyNameFromStackTrace()
        {
            // note. for VS2008 SP1 stuff works out finely when I simply return null
            return null;
        }

        public static Object CreateDebugProxy(this Node node, Object parentProxy)
        {
            return node.CreateDebugProxy(parentProxy, null);
        }

        public static Object CreateDebugProxy(this Node node, Object parentProxy, String name)
        {
            node = node ?? new Null();

            var needsNoParent = false;
            if (parentProxy != null)
            {
                var pp = parentProxy.AssertCast<Node.INodeDebugView>();
                var h = pp.Unfoldi(p => p.Parent, p => p != null).Select(p => p.Node);

                var effParent = node.Parent;
                if (effParent is Apply) effParent = ((Apply)effParent).Parent;
                needsNoParent = h.Contains(effParent);
            }

            var attrs = node.GetType().Attrs<DebuggerTypeProxyAttribute>();
            var t_proxy = Type.GetType(attrs.First().ProxyTypeName, true);
            if (needsNoParent) t_proxy = t_proxy.Assembly.GetType(t_proxy.FullName + "_NoParent", true);
            return t_proxy.CreateInstance(node, parentProxy, name);
        }

        public static Object CreateDebugProxy(this Block block, Object parentProxy)
        {
            return ((Node)block).CreateDebugProxy(parentProxy);
        }

        public static Object CreateDebugProxy(this Block block, Object parentProxy, String name)
        {
            return ((Node)block).CreateDebugProxy(parentProxy, name);
        }

        public static Object[] CreateDebugProxy<T>(this IEnumerable<T> nodes, Object parentProxy)
            where T : Node
        {
            return nodes.CreateDebugProxy(parentProxy, (_, i) => "[" + i.ToString() + "]");
        }

        public static Object[] CreateDebugProxy<T>(this IEnumerable<T> nodes, Object parentProxy, Func<T, int, String> namer)
            where T : Node
        {
            nodes = nodes ?? Seq.Empty<T>();
            return nodes.Select((n, i) => n.CreateDebugProxy(parentProxy, namer(n, i))).ToArray();
        }
    }

}
