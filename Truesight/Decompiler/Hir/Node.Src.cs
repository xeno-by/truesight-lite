using System;
using Truesight.Parser.Api.DebugInfo;

namespace Truesight.Decompiler.Hir
{
    public static class Foo
    {
        public static T HasProto<T>(this T node, Node proto)
            where T : Node
        {
            if (node == null) return null;
            node.Proto = proto;
            return node;
        }
    }

    public partial class Node
    {
        public Node Proto { get; set; }

        private ITextRun _src = null;
        public ITextRun Src
        {
            get { return _src ?? (Proto == null ? null : Proto.Src); }
            set { _src = value; }
        }
    }
}
