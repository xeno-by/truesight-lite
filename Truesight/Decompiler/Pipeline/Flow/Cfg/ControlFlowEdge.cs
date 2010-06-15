using System;
using System.Diagnostics;
using QuickGraph;
using XenoGears.Assertions;
using XenoGears.Traits.Cloneable;

namespace Truesight.Decompiler.Pipeline.Flow.Cfg
{
    [DebuggerDisplay("{ToString()}")]
    internal class ControlFlowEdge : TaggedEdge<ControlFlowBlock, PredicateType?>, ICloneable2
    {

        public PredicateType Condition
        {
            get
            {
                Tag.AssertNotNull();
                return Tag.Value;
            }

            set
            {
                Tag = value;
            }
        }

        public bool IsConditional
        {
            get
            {
                return Tag != null;
            }

            set
            {
                value.AssertFalse();
                Tag = null;
            }
        }

        public bool IsUnconditional
        {
            get
            {
                return Tag == null;
            }

            set
            {
                value.AssertTrue();
                Tag = null;
            }
        }

        public ControlFlowEdge(ControlFlowBlock source, ControlFlowBlock target)
            : base(source.AssertNotNull(), target.AssertNotNull(), null)
        {
        }

        public ControlFlowEdge(ControlFlowBlock source, ControlFlowBlock target, PredicateType tag)
            : base(source.AssertNotNull(), target.AssertNotNull(), tag)
        {
        }

        public ControlFlowEdge(ControlFlowBlock source, ControlFlowBlock target, PredicateType? tag)
            : base(source.AssertNotNull(), target.AssertNotNull(), tag)
        {
        }

        private readonly Guid _uniqueId = Guid.NewGuid();
        public Guid UniqueId { get { return _uniqueId; } }
        private Guid _protoId = Guid.NewGuid();
        public Guid ProtoId { get { return _protoId; } }
        T ICloneable2.DeepClone<T>() { throw new NotSupportedException(); }
        public ControlFlowEdge ShallowClone() { return ((ICloneable2)this).ShallowClone<ControlFlowEdge>(); }
        T ICloneable2.ShallowClone<T>()
        {
            var clone = new ControlFlowEdge(Source, Target, Tag);
            clone._protoId = _protoId;
            return clone.AssertCast<T>();
        }

        public override String ToString()
        {
            if (IsUnconditional)
            {
                return String.Format("{0} ---> {1}", Source, Target);
            }
            else
            {
                return String.Format("{0} --[{2}]--> {1}", Source, Target, Condition);
            }
        }
    }
}