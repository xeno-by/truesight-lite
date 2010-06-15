using Truesight.Decompiler.Hir.Core.ControlFlow;
using XenoGears.Assertions;
using XenoGears.Functional;

namespace Truesight.Decompiler.Hir
{
    public abstract partial class Node
    {
        public void ReplaceWith(Node node)
        {
            if (node == null) RemoveSelf();

            var blk = node as Block;
            if (blk != null && blk.IsEmpty()) RemoveSelf();
            else
            {
                Parent.AssertNotNull();
                Parent.Children[Index] = node;
            }
        }

        public void RemoveSelf()
        {
            var blk = Parent as Block;
            if (blk != null) blk.Remove(this);
            else
            {
                Parent.AssertNotNull();
                Parent.Children[Index] = null;
            }
        }
    }
}
