using System;
using Truesight.Decompiler.Hir.Traversal.Transformers;
using XenoGears.Assertions;
using XenoGears.Traits.Cloneable;

namespace Truesight.Decompiler.Hir
{
    public abstract partial class Node : ICloneable2
    {
        private readonly Guid _uniqueId = Guid.NewGuid();
        public Guid UniqueId { get { return _uniqueId; } }

        private Guid _protoId = Guid.NewGuid();
        public Guid ProtoId { get { return _protoId; } }

        T ICloneable2.ShallowClone<T>()
        {
            throw new NotSupportedException();
        }

        public Node DeepClone() { return ((ICloneable2)this).DeepClone<Node>(); }
        T ICloneable2.DeepClone<T>()
        {
            return this.Transform<Node, Node>(DeepClone_Transform).AssertCast<T>();
        }

        private Node DeepClone_Transform(Node node)
        {
            var clone = node.DefaultTransform();
            if (clone != null)
            {
                clone._protoId = node._protoId;
                clone.Domain = node.Domain;
            }

            return clone;
        }
    }
}
