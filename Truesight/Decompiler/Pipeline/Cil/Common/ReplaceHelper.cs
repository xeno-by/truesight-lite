using System;
using System.Diagnostics;
using Truesight.Decompiler.Hir;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Traits.Equivatable;

namespace Truesight.Decompiler.Pipeline.Cil.Common
{
    [DebuggerNonUserCode]
    internal static class ReplaceHelper
    {
        public static void ReplaceRecursive(this Node root, Node find, Node replace)
        {
            root.ReplaceRecursive(n => find.Equiv(n), _ => replace);
        }

        public static void ReplaceRecursive(this Node root, Func<Node, bool> find, Node replace)
        {
            root.ReplaceRecursive(find, _ => replace);
        }

        public static void ReplaceRecursive(this Node root, Node find, Func<Node, Node> replace)
        {
            root.ReplaceRecursive(n => ReferenceEquals(find, n), replace);
        }

        public static void ReplaceRecursive(this Node root, Func<Node, bool> find, Func<Node, Node> replace)
        {
            if (find(root))
            {
                AssertionHelper.Fail();
            }
            else
            {
                // order of the following two lines is important
                // if you swap them, then replacing "foo" with "expr(foo)" will lead to stack overflow
                root.Children.ForEach(c => { if (c != null && !find(c)) c.ReplaceRecursive(find, replace); });
                root.Children.ReplaceElements(find, replace);
            }
        }
    }
}
