using System;
using System.Collections.Generic;
using System.Diagnostics;
using XenoGears.Assertions;
using XenoGears.Reflection;

namespace Truesight.Decompiler.Hir.Traversal.Transformers
{
    [DebuggerNonUserCode]
    public static partial class HirTransformer
    {
        public static Node Transform<T>(this Node e)
            where T : AbstractHirTransformer
        {
            return e.Transform(() => typeof(T).CreateInstance().AssertCast<T>());
        }

        public static Node Transform<T>(this Node e, Func<T> factory)
            where T : AbstractHirTransformer
        {
            return factory().Transform(e);
        }

        public static Node Transform<TIn1, TOut1>(this Node e, Func<TIn1, TOut1> transformer1)
            where TIn1 : Node
            where TOut1 : Node
        {
            var map = new Dictionary<Type, Func<Node, Node>>();
            map.Add(typeof(TIn1), new Transformer_Thunk<TIn1, TOut1>(transformer1).Snippet);
            return new LateBoundHirTransformer(map).Transform(e);
        }

        public static Node Transform<TIn1, TOut1, TIn2, TOut2>(this Node e, Func<TIn1, TOut1> transformer1, Func<TIn2, TOut2> transformer2)
            where TIn1 : Node
            where TOut1 : Node
            where TIn2 : Node
            where TOut2 : Node
        {
            var map = new Dictionary<Type, Func<Node, Node>>();
            map.Add(typeof(TIn1), new Transformer_Thunk<TIn1, TOut1>(transformer1).Snippet);
            map.Add(typeof(TIn2), new Transformer_Thunk<TIn2, TOut2>(transformer2).Snippet);
            return new LateBoundHirTransformer(map).Transform(e);
        }

        public static Node Transform<TIn1, TOut1, TIn2, TOut2, TIn3, TOut3>(this Node e, Func<TIn1, TOut1> transformer1, Func<TIn2, TOut2> transformer2, Func<TIn3, TOut3> transformer3)
            where TIn1 : Node
            where TOut1 : Node
            where TIn2 : Node
            where TOut2 : Node
            where TIn3 : Node
            where TOut3 : Node
        {
            var map = new Dictionary<Type, Func<Node, Node>>();
            map.Add(typeof(TIn1), new Transformer_Thunk<TIn1, TOut1>(transformer1).Snippet);
            map.Add(typeof(TIn2), new Transformer_Thunk<TIn2, TOut2>(transformer2).Snippet);
            map.Add(typeof(TIn3), new Transformer_Thunk<TIn3, TOut3>(transformer3).Snippet);
            return new LateBoundHirTransformer(map).Transform(e);
        }

        public static Node Transform<TIn1, TOut1, TIn2, TOut2, TIn3, TOut3, TIn4, TOut4>(this Node e, Func<TIn1, TOut1> transformer1, Func<TIn2, TOut2> transformer2, Func<TIn3, TOut3> transformer3, Func<TIn4, TOut4> transformer4)
            where TIn1 : Node
            where TOut1 : Node
            where TIn2 : Node
            where TOut2 : Node
            where TIn3 : Node
            where TOut3 : Node
            where TIn4 : Node
            where TOut4 : Node
        {
            var map = new Dictionary<Type, Func<Node, Node>>();
            map.Add(typeof(TIn1), new Transformer_Thunk<TIn1, TOut1>(transformer1).Snippet);
            map.Add(typeof(TIn2), new Transformer_Thunk<TIn2, TOut2>(transformer2).Snippet);
            map.Add(typeof(TIn3), new Transformer_Thunk<TIn3, TOut3>(transformer3).Snippet);
            map.Add(typeof(TIn4), new Transformer_Thunk<TIn4, TOut4>(transformer4).Snippet);
            return new LateBoundHirTransformer(map).Transform(e);
        }

        public static Node Transform<TIn1, TOut1, TIn2, TOut2, TIn3, TOut3, TIn4, TOut4, TIn5, TOut5>(this Node e, Func<TIn1, TOut1> transformer1, Func<TIn2, TOut2> transformer2, Func<TIn3, TOut3> transformer3, Func<TIn4, TOut4> transformer4, Func<TIn5, TOut5> transformer5)
            where TIn1 : Node
            where TOut1 : Node
            where TIn2 : Node
            where TOut2 : Node
            where TIn3 : Node
            where TOut3 : Node
            where TIn4 : Node
            where TOut4 : Node
            where TIn5 : Node
            where TOut5 : Node
        {
            var map = new Dictionary<Type, Func<Node, Node>>();
            map.Add(typeof(TIn1), new Transformer_Thunk<TIn1, TOut1>(transformer1).Snippet);
            map.Add(typeof(TIn2), new Transformer_Thunk<TIn2, TOut2>(transformer2).Snippet);
            map.Add(typeof(TIn3), new Transformer_Thunk<TIn3, TOut3>(transformer3).Snippet);
            map.Add(typeof(TIn4), new Transformer_Thunk<TIn4, TOut4>(transformer4).Snippet);
            map.Add(typeof(TIn5), new Transformer_Thunk<TIn5, TOut5>(transformer5).Snippet);
            return new LateBoundHirTransformer(map).Transform(e);
        }

        public static Node Transform<TIn1, TOut1, TIn2, TOut2, TIn3, TOut3, TIn4, TOut4, TIn5, TOut5, TIn6, TOut6>(this Node e, Func<TIn1, TOut1> transformer1, Func<TIn2, TOut2> transformer2, Func<TIn3, TOut3> transformer3, Func<TIn4, TOut4> transformer4, Func<TIn5, TOut5> transformer5, Func<TIn6, TOut6> transformer6)
            where TIn1 : Node
            where TOut1 : Node
            where TIn2 : Node
            where TOut2 : Node
            where TIn3 : Node
            where TOut3 : Node
            where TIn4 : Node
            where TOut4 : Node
            where TIn5 : Node
            where TOut5 : Node
            where TIn6 : Node
            where TOut6 : Node
        {
            var map = new Dictionary<Type, Func<Node, Node>>();
            map.Add(typeof(TIn1), new Transformer_Thunk<TIn1, TOut1>(transformer1).Snippet);
            map.Add(typeof(TIn2), new Transformer_Thunk<TIn2, TOut2>(transformer2).Snippet);
            map.Add(typeof(TIn3), new Transformer_Thunk<TIn3, TOut3>(transformer3).Snippet);
            map.Add(typeof(TIn4), new Transformer_Thunk<TIn4, TOut4>(transformer4).Snippet);
            map.Add(typeof(TIn5), new Transformer_Thunk<TIn5, TOut5>(transformer5).Snippet);
            map.Add(typeof(TIn6), new Transformer_Thunk<TIn6, TOut6>(transformer6).Snippet);
            return new LateBoundHirTransformer(map).Transform(e);
        }

        public static Node Transform<TIn1, TOut1, TIn2, TOut2, TIn3, TOut3, TIn4, TOut4, TIn5, TOut5, TIn6, TOut6, TIn7, TOut7>(this Node e, Func<TIn1, TOut1> transformer1, Func<TIn2, TOut2> transformer2, Func<TIn3, TOut3> transformer3, Func<TIn4, TOut4> transformer4, Func<TIn5, TOut5> transformer5, Func<TIn6, TOut6> transformer6, Func<TIn7, TOut7> transformer7)
            where TIn1 : Node
            where TOut1 : Node
            where TIn2 : Node
            where TOut2 : Node
            where TIn3 : Node
            where TOut3 : Node
            where TIn4 : Node
            where TOut4 : Node
            where TIn5 : Node
            where TOut5 : Node
            where TIn6 : Node
            where TOut6 : Node
            where TIn7 : Node
            where TOut7 : Node
        {
            var map = new Dictionary<Type, Func<Node, Node>>();
            map.Add(typeof(TIn1), new Transformer_Thunk<TIn1, TOut1>(transformer1).Snippet);
            map.Add(typeof(TIn2), new Transformer_Thunk<TIn2, TOut2>(transformer2).Snippet);
            map.Add(typeof(TIn3), new Transformer_Thunk<TIn3, TOut3>(transformer3).Snippet);
            map.Add(typeof(TIn4), new Transformer_Thunk<TIn4, TOut4>(transformer4).Snippet);
            map.Add(typeof(TIn5), new Transformer_Thunk<TIn5, TOut5>(transformer5).Snippet);
            map.Add(typeof(TIn6), new Transformer_Thunk<TIn6, TOut6>(transformer6).Snippet);
            map.Add(typeof(TIn7), new Transformer_Thunk<TIn7, TOut7>(transformer7).Snippet);
            return new LateBoundHirTransformer(map).Transform(e);
        }

        public static Node Transform<TIn1, TOut1, TIn2, TOut2, TIn3, TOut3, TIn4, TOut4, TIn5, TOut5, TIn6, TOut6, TIn7, TOut7, TIn8, TOut8>(this Node e, Func<TIn1, TOut1> transformer1, Func<TIn2, TOut2> transformer2, Func<TIn3, TOut3> transformer3, Func<TIn4, TOut4> transformer4, Func<TIn5, TOut5> transformer5, Func<TIn6, TOut6> transformer6, Func<TIn7, TOut7> transformer7, Func<TIn8, TOut8> transformer8)
            where TIn1 : Node
            where TOut1 : Node
            where TIn2 : Node
            where TOut2 : Node
            where TIn3 : Node
            where TOut3 : Node
            where TIn4 : Node
            where TOut4 : Node
            where TIn5 : Node
            where TOut5 : Node
            where TIn6 : Node
            where TOut6 : Node
            where TIn7 : Node
            where TOut7 : Node
            where TIn8 : Node
            where TOut8 : Node
        {
            var map = new Dictionary<Type, Func<Node, Node>>();
            map.Add(typeof(TIn1), new Transformer_Thunk<TIn1, TOut1>(transformer1).Snippet);
            map.Add(typeof(TIn2), new Transformer_Thunk<TIn2, TOut2>(transformer2).Snippet);
            map.Add(typeof(TIn3), new Transformer_Thunk<TIn3, TOut3>(transformer3).Snippet);
            map.Add(typeof(TIn4), new Transformer_Thunk<TIn4, TOut4>(transformer4).Snippet);
            map.Add(typeof(TIn5), new Transformer_Thunk<TIn5, TOut5>(transformer5).Snippet);
            map.Add(typeof(TIn6), new Transformer_Thunk<TIn6, TOut6>(transformer6).Snippet);
            map.Add(typeof(TIn7), new Transformer_Thunk<TIn7, TOut7>(transformer7).Snippet);
            map.Add(typeof(TIn8), new Transformer_Thunk<TIn8, TOut8>(transformer8).Snippet);
            return new LateBoundHirTransformer(map).Transform(e);
        }

        [DebuggerNonUserCode]
        private class Transformer_Thunk<TIn, TOut>
            where TIn : Node
            where TOut : Node
        {
            private readonly Func<TIn, TOut> _transformer;
            public Transformer_Thunk(Func<TIn, TOut> transformer) { _transformer = transformer; }

            public Node Snippet(Node node)
            {
                return _transformer((TIn)node);
            }
        }
    }
}