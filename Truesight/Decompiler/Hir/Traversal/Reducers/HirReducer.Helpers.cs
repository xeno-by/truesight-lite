using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Traits.Disposable;

namespace Truesight.Decompiler.Hir.Traversal.Reducers
{
    public partial class HirReducer
    {
        private static readonly List<AbstractHirReducer> _runningVisitors = new List<AbstractHirReducer>();
        public static AbstractHirReducer Current { get { return _runningVisitors.LastOrDefault(); } }
        internal static IDisposable SetCurrent(AbstractHirReducer self)
        {
            _runningVisitors.Add(self);
            var thunk = new ResetCurrent_Thunk(self);
            return new DisposableAction(thunk.Snippet);
        }

        [DebuggerNonUserCode]
        private class ResetCurrent_Thunk
        {
            private readonly AbstractHirReducer _self;
            public ResetCurrent_Thunk(AbstractHirReducer self) { _self = self; }

            public void Snippet()
            {
                (Current == _self).AssertTrue();
                _runningVisitors.RemoveLast();
            }
        }

        public static R Reduce<R>(this Node node)
        {
            var current = Current.AssertNotNull();
            return current.Cast<R>().Reduce(node);
        }
    }
}
