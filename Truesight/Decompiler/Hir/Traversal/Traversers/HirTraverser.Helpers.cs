using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Traits.Disposable;

namespace Truesight.Decompiler.Hir.Traversal.Traversers
{
    public partial class HirTraverser
    {
        private static readonly List<AbstractHirTraverser> _runningVisitors = new List<AbstractHirTraverser>();
        public static AbstractHirTraverser Current { get { return _runningVisitors.LastOrDefault(); } }
        internal static IDisposable SetCurrent(AbstractHirTraverser self)
        {
            _runningVisitors.Add(self);
            var thunk = new ResetCurrent_Thunk(self);
            return new DisposableAction(thunk.Snippet);
        }

        [DebuggerNonUserCode]
        private class ResetCurrent_Thunk
        {
            private readonly AbstractHirTraverser _self;
            public ResetCurrent_Thunk(AbstractHirTraverser self) { _self = self; }

            public void Snippet()
            {
                (Current == _self).AssertTrue();
                _runningVisitors.RemoveLast();
            }
        }

        public static void Traverse(this Node node)
        {
            var current = Current.AssertNotNull();
            current.Traverse(node);
        }
    }
}
