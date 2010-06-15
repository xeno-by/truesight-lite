using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Traits.Disposable;

namespace Truesight.Decompiler.Hir.Traversal.Transformers
{
    public partial class HirTransformer
    {
        private static readonly List<AbstractHirTransformer> _runningVisitors = new List<AbstractHirTransformer>();
        public static AbstractHirTransformer Current { get { return _runningVisitors.LastOrDefault(); } }
        internal static IDisposable SetCurrent(AbstractHirTransformer self)
        {
            _runningVisitors.Add(self);
            var thunk = new ResetCurrent_Thunk(self);
            return new DisposableAction(thunk.Snippet);
        }

        [DebuggerNonUserCode]
        private class ResetCurrent_Thunk
        {
            private readonly AbstractHirTransformer _self;
            public ResetCurrent_Thunk(AbstractHirTransformer self) { _self = self; }

            public void Snippet()
            {
                (Current == _self).AssertTrue();
                _runningVisitors.RemoveLast();
            }
        }

        public static T DefaultTransform<T>(this T node)
            where T : Node
        {
            var current = Current.AssertNotNull();
            return (T)node.AcceptTransformer(current, true);
        }

        public static Node CurrentTransform(this Node node)
        {
            var current = Current.AssertNotNull();
            return current.Transform(node);
        }

        public static T CurrentTransform<T>(this Node node)
            where T : Node
        {
            var current = Current.AssertNotNull();
            return (T)current.Transform(node);
        }

        public static T CurrentTransform<T>(this T node)
            where T : Node
        {
            var current = Current.AssertNotNull();
            return (T)current.Transform(node);
        }
    }
}
