using Truesight.Decompiler.Hir.Traversal.Reducers;
using Truesight.Decompiler.Hir.Traversal.Transformers;
using Truesight.Decompiler.Hir.Traversal.Traversers;

namespace Truesight.Decompiler.Hir
{
    public abstract partial class Node
    {
        public abstract T AcceptReducer<T>(AbstractHirReducer<T> reducer);
        public abstract void AcceptTraverser(AbstractHirTraverser traverser);
        public abstract Node AcceptTransformer(AbstractHirTransformer transformer, bool forceDefaultImpl);
    }
}
