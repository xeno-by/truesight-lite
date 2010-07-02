using System;
using System.Collections.Generic;
using System.Diagnostics;
using XenoGears.Assertions;
using XenoGears.Reflection;

namespace Truesight.Decompiler.Hir.Traversal.Traversers
{
    [DebuggerNonUserCode]
    public static partial class HirTraverser
    {
        public static T Traverse<T>(this Node e)
            where T : AbstractHirTraverser
        {
            return e.Traverse(() => typeof(T).CreateInstance().AssertCast<T>());
        }

        public static T Traverse<T>(this Node e, Func<T> factory)
            where T : AbstractHirTraverser
        {
            var traverser = factory();
            traverser.Traverse(e);
            return traverser;
        }

        public static void Traverse<TIn1>(this Node e, Action<TIn1> traverser1)
            where TIn1 : Node
        {
            var map = new Dictionary<Type, Action<Node>>();
            map.Add(typeof(TIn1), new Traverser_Thunk<TIn1>(traverser1).Snippet);
            new LateBoundHirTraverser(map).Traverse(e);
        }

        public static void Traverse<TIn1, TIn2>(this Node e, Action<TIn1> traverser1, Action<TIn2> traverser2)
            where TIn1 : Node
            where TIn2 : Node
        {
            var map = new Dictionary<Type, Action<Node>>();
            map.Add(typeof(TIn1), new Traverser_Thunk<TIn1>(traverser1).Snippet);
            map.Add(typeof(TIn2), new Traverser_Thunk<TIn2>(traverser2).Snippet);
            new LateBoundHirTraverser(map).Traverse(e);
        }

        public static void Traverse<TIn1, TIn2, TIn3>(this Node e, Action<TIn1> traverser1, Action<TIn2> traverser2, Action<TIn3> traverser3)
            where TIn1 : Node
            where TIn2 : Node
            where TIn3 : Node
        {
            var map = new Dictionary<Type, Action<Node>>();
            map.Add(typeof(TIn1), new Traverser_Thunk<TIn1>(traverser1).Snippet);
            map.Add(typeof(TIn2), new Traverser_Thunk<TIn2>(traverser2).Snippet);
            map.Add(typeof(TIn3), new Traverser_Thunk<TIn3>(traverser3).Snippet);
            new LateBoundHirTraverser(map).Traverse(e);
        }

        public static void Traverse<TIn1, TIn2, TIn3, TIn4>(this Node e, Action<TIn1> traverser1, Action<TIn2> traverser2, Action<TIn3> traverser3, Action<TIn4> traverser4)
            where TIn1 : Node
            where TIn2 : Node
            where TIn3 : Node
            where TIn4 : Node
        {
            var map = new Dictionary<Type, Action<Node>>();
            map.Add(typeof(TIn1), new Traverser_Thunk<TIn1>(traverser1).Snippet);
            map.Add(typeof(TIn2), new Traverser_Thunk<TIn2>(traverser2).Snippet);
            map.Add(typeof(TIn3), new Traverser_Thunk<TIn3>(traverser3).Snippet);
            map.Add(typeof(TIn4), new Traverser_Thunk<TIn4>(traverser4).Snippet);
            new LateBoundHirTraverser(map).Traverse(e);
        }

        public static void Traverse<TIn1, TIn2, TIn3, TIn4, TIn5>(this Node e, Action<TIn1> traverser1, Action<TIn2> traverser2, Action<TIn3> traverser3, Action<TIn4> traverser4, Action<TIn5> traverser5)
            where TIn1 : Node
            where TIn2 : Node
            where TIn3 : Node
            where TIn4 : Node
            where TIn5 : Node
        {
            var map = new Dictionary<Type, Action<Node>>();
            map.Add(typeof(TIn1), new Traverser_Thunk<TIn1>(traverser1).Snippet);
            map.Add(typeof(TIn2), new Traverser_Thunk<TIn2>(traverser2).Snippet);
            map.Add(typeof(TIn3), new Traverser_Thunk<TIn3>(traverser3).Snippet);
            map.Add(typeof(TIn4), new Traverser_Thunk<TIn4>(traverser4).Snippet);
            map.Add(typeof(TIn5), new Traverser_Thunk<TIn5>(traverser5).Snippet);
            new LateBoundHirTraverser(map).Traverse(e);
        }

        public static void Traverse<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6>(this Node e, Action<TIn1> traverser1, Action<TIn2> traverser2, Action<TIn3> traverser3, Action<TIn4> traverser4, Action<TIn5> traverser5, Action<TIn6> traverser6)
            where TIn1 : Node
            where TIn2 : Node
            where TIn3 : Node
            where TIn4 : Node
            where TIn5 : Node
            where TIn6 : Node
        {
            var map = new Dictionary<Type, Action<Node>>();
            map.Add(typeof(TIn1), new Traverser_Thunk<TIn1>(traverser1).Snippet);
            map.Add(typeof(TIn2), new Traverser_Thunk<TIn2>(traverser2).Snippet);
            map.Add(typeof(TIn3), new Traverser_Thunk<TIn3>(traverser3).Snippet);
            map.Add(typeof(TIn4), new Traverser_Thunk<TIn4>(traverser4).Snippet);
            map.Add(typeof(TIn5), new Traverser_Thunk<TIn5>(traverser5).Snippet);
            map.Add(typeof(TIn6), new Traverser_Thunk<TIn6>(traverser6).Snippet);
            new LateBoundHirTraverser(map).Traverse(e);
        }

        public static void Traverse<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7>(this Node e, Action<TIn1> traverser1, Action<TIn2> traverser2, Action<TIn3> traverser3, Action<TIn4> traverser4, Action<TIn5> traverser5, Action<TIn6> traverser6, Action<TIn7> traverser7)
            where TIn1 : Node
            where TIn2 : Node
            where TIn3 : Node
            where TIn4 : Node
            where TIn5 : Node
            where TIn6 : Node
            where TIn7 : Node
        {
            var map = new Dictionary<Type, Action<Node>>();
            map.Add(typeof(TIn1), new Traverser_Thunk<TIn1>(traverser1).Snippet);
            map.Add(typeof(TIn2), new Traverser_Thunk<TIn2>(traverser2).Snippet);
            map.Add(typeof(TIn3), new Traverser_Thunk<TIn3>(traverser3).Snippet);
            map.Add(typeof(TIn4), new Traverser_Thunk<TIn4>(traverser4).Snippet);
            map.Add(typeof(TIn5), new Traverser_Thunk<TIn5>(traverser5).Snippet);
            map.Add(typeof(TIn6), new Traverser_Thunk<TIn6>(traverser6).Snippet);
            map.Add(typeof(TIn7), new Traverser_Thunk<TIn7>(traverser7).Snippet);
            new LateBoundHirTraverser(map).Traverse(e);
        }

        public static void Traverse<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8>(this Node e, Action<TIn1> traverser1, Action<TIn2> traverser2, Action<TIn3> traverser3, Action<TIn4> traverser4, Action<TIn5> traverser5, Action<TIn6> traverser6, Action<TIn7> traverser7, Action<TIn8> traverser8)
            where TIn1 : Node
            where TIn2 : Node
            where TIn3 : Node
            where TIn4 : Node
            where TIn5 : Node
            where TIn6 : Node
            where TIn7 : Node
            where TIn8 : Node
        {
            var map = new Dictionary<Type, Action<Node>>();
            map.Add(typeof(TIn1), new Traverser_Thunk<TIn1>(traverser1).Snippet);
            map.Add(typeof(TIn2), new Traverser_Thunk<TIn2>(traverser2).Snippet);
            map.Add(typeof(TIn3), new Traverser_Thunk<TIn3>(traverser3).Snippet);
            map.Add(typeof(TIn4), new Traverser_Thunk<TIn4>(traverser4).Snippet);
            map.Add(typeof(TIn5), new Traverser_Thunk<TIn5>(traverser5).Snippet);
            map.Add(typeof(TIn6), new Traverser_Thunk<TIn6>(traverser6).Snippet);
            map.Add(typeof(TIn7), new Traverser_Thunk<TIn7>(traverser7).Snippet);
            map.Add(typeof(TIn8), new Traverser_Thunk<TIn8>(traverser8).Snippet);
            new LateBoundHirTraverser(map).Traverse(e);
        }

        [DebuggerNonUserCode]
        private class Traverser_Thunk<TIn>
            where TIn : Node
        {
            private readonly Action<TIn> _traverser;
            public Traverser_Thunk(Action<TIn> traverser) { _traverser = traverser; }

            public void Snippet(Node node)
            {
                _traverser((TIn)node);
            }
        }
    }
}