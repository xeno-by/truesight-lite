using System;
using System.Collections.Generic;
using System.Diagnostics;
using XenoGears.Assertions;
using XenoGears.Reflection;

namespace Truesight.Decompiler.Hir.Traversal.Reducers
{
    [DebuggerNonUserCode]
    public static partial class HirReducer
    {
        public static T Reduce<R, T>(this Node e)
            where R : AbstractHirReducer<T>
        {
            return e.Reduce(() => typeof(R).CreateInstance().AssertCast<R>());
        }

        public static T Reduce<T>(this Node e, Func<AbstractHirReducer<T>> factory)
        {
            return factory().Reduce(e);
        }

        public static T Reduce<TIn1, T>(this Node e, Func<TIn1, T> reducer1)
            where TIn1 : Node
        {
            var map = new Dictionary<Type, Func<Node, T>>();
            map.Add(typeof(TIn1), new Reducer_Thunk<TIn1, T>(reducer1).Snippet);
            return new LateBoundHirReducer<T>(map).Reduce(e);
        }

        public static T Reduce<TIn1, TIn2, T>(this Node e, Func<TIn1, T> reducer1, Func<TIn2, T> reducer2)
            where TIn1 : Node
            where TIn2 : Node
        {
            var map = new Dictionary<Type, Func<Node, T>>();
            map.Add(typeof(TIn1), new Reducer_Thunk<TIn1, T>(reducer1).Snippet);
            map.Add(typeof(TIn2), new Reducer_Thunk<TIn2, T>(reducer2).Snippet);
            return new LateBoundHirReducer<T>(map).Reduce(e);
        }

        public static T Reduce<TIn1, TIn2, TIn3, T>(this Node e, Func<TIn1, T> reducer1, Func<TIn2, T> reducer2, Func<TIn3, T> reducer3)
            where TIn1 : Node
            where TIn2 : Node
            where TIn3 : Node
        {
            var map = new Dictionary<Type, Func<Node, T>>();
            map.Add(typeof(TIn1), new Reducer_Thunk<TIn1, T>(reducer1).Snippet);
            map.Add(typeof(TIn2), new Reducer_Thunk<TIn2, T>(reducer2).Snippet);
            map.Add(typeof(TIn3), new Reducer_Thunk<TIn3, T>(reducer3).Snippet);
            return new LateBoundHirReducer<T>(map).Reduce(e);
        }

        public static T Reduce<TIn1, TIn2, TIn3, TIn4, T>(this Node e, Func<TIn1, T> reducer1, Func<TIn2, T> reducer2, Func<TIn3, T> reducer3, Func<TIn4, T> reducer4)
            where TIn1 : Node
            where TIn2 : Node
            where TIn3 : Node
            where TIn4 : Node
        {
            var map = new Dictionary<Type, Func<Node, T>>();
            map.Add(typeof(TIn1), new Reducer_Thunk<TIn1, T>(reducer1).Snippet);
            map.Add(typeof(TIn2), new Reducer_Thunk<TIn2, T>(reducer2).Snippet);
            map.Add(typeof(TIn3), new Reducer_Thunk<TIn3, T>(reducer3).Snippet);
            map.Add(typeof(TIn4), new Reducer_Thunk<TIn4, T>(reducer4).Snippet);
            return new LateBoundHirReducer<T>(map).Reduce(e);
        }

        public static T Reduce<TIn1, TIn2, TIn3, TIn4, TIn5, T>(this Node e, Func<TIn1, T> reducer1, Func<TIn2, T> reducer2, Func<TIn3, T> reducer3, Func<TIn4, T> reducer4, Func<TIn5, T> reducer5)
            where TIn1 : Node
            where TIn2 : Node
            where TIn3 : Node
            where TIn4 : Node
            where TIn5 : Node
        {
            var map = new Dictionary<Type, Func<Node, T>>();
            map.Add(typeof(TIn1), new Reducer_Thunk<TIn1, T>(reducer1).Snippet);
            map.Add(typeof(TIn2), new Reducer_Thunk<TIn2, T>(reducer2).Snippet);
            map.Add(typeof(TIn3), new Reducer_Thunk<TIn3, T>(reducer3).Snippet);
            map.Add(typeof(TIn4), new Reducer_Thunk<TIn4, T>(reducer4).Snippet);
            map.Add(typeof(TIn5), new Reducer_Thunk<TIn5, T>(reducer5).Snippet);
            return new LateBoundHirReducer<T>(map).Reduce(e);
        }

        [DebuggerNonUserCode]
        private class Reducer_Thunk<TIn, T>
            where TIn : Node
        {
            private readonly Func<TIn, T> _reducer;
            public Reducer_Thunk(Func<TIn, T> reducer) { _reducer = reducer; }

            public T Snippet(Node node)
            {
                return _reducer((TIn)node);
            }
        }
    }
}