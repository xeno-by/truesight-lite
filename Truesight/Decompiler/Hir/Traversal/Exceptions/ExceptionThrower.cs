using System.Diagnostics;

namespace Truesight.Decompiler.Hir.Traversal.Exceptions
{
    [DebuggerNonUserCode]
    public static class ExceptionThrower
    {
        public static UnexpectedNodeException Unexpected<T>(this T node)
        {
            throw new UnexpectedNodeException(node, typeof(T));
        }

        public static UnsupportedNodeException Unsupported(this Node node)
        {
            throw new UnsupportedNodeException(node);
        }
    }
}
