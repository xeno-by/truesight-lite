using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Truesight.Decompiler.Hir.Core.ControlFlow;
using Truesight.Decompiler.Hir.Core.Expressions;
using XenoGears.Collections.Dictionaries;
using XenoGears.Functional;
using XenoGears.Collections;
using XenoGears.Assertions;
using Truesight.Decompiler.Pipeline.Cil;

namespace Truesight.Decompiler.Hir.Core.Symbols
{
    [DebuggerNonUserCode]
    public static class SymHelpers
    {
        public static ReadOnlyCollection<Local> UsedLocals(this Node node)
        {
            return node.MkArray().UsedLocals();
        }

        public static ReadOnlyDictionary<Local, ReadOnlyCollection<Ref>> UsagesOfLocals(this Node node)
        {
            return node.MkArray().UsagesOfLocals();
        }

        public static ReadOnlyCollection<Ref> UsagesOfLocal(this Node node, Local local)
        {
            return node.MkArray().UsagesOfLocal(local);
        }

        public static ReadOnlyCollection<Local> UsedLocals(this Block block)
        {
            return ((Node)block).UsedLocals();
        }

        public static ReadOnlyDictionary<Local, ReadOnlyCollection<Ref>> UsagesOfLocals(this Block block)
        {
            return ((Node)block).UsagesOfLocals();
        }

        public static ReadOnlyCollection<Ref> UsagesOfLocal(this Block block, Local local)
        {
            return ((Node)block).UsagesOfLocal(local);
        }

        public static ReadOnlyCollection<Local> UsedLocals(this IEnumerable<Node> nodes)
        {
            return nodes.SelectMany(node => node.CSharpEvaluationOrder().OfType<Ref>()
                .Select(@ref => @ref.Sym).OfType<Local>())
                .Distinct().ToReadOnly();
        }

        public static ReadOnlyDictionary<Local, ReadOnlyCollection<Ref>> UsagesOfLocals(this IEnumerable<Node> nodes)
        {
            return nodes.SelectMany(node => node.CSharpEvaluationOrder().OfType<Ref>()
                .Where(@ref => @ref.Sym != null && @ref.Sym.IsLocal()))
                .GroupBy(@ref => @ref.Sym.AssertCast<Local>(), @ref => @ref)
                .ToDictionary(g => g.Key, g => g.ToReadOnly()).ToReadOnly();
        }

        public static ReadOnlyCollection<Ref> UsagesOfLocal(this IEnumerable<Node> nodes, Local local)
        {
            return nodes.SelectMany(node => node.CSharpEvaluationOrder().OfType<Ref>().Where(@ref => @ref.Sym == local)).ToReadOnly();
        }

        public static ReadOnlyCollection<Param> UsedParams(this Node node)
        {
            return node.MkArray().UsedParams();
        }

        public static ReadOnlyDictionary<Param, ReadOnlyCollection<Ref>> UsagesOfParams(this Node node)
        {
            return node.MkArray().UsagesOfParams();
        }

        public static ReadOnlyCollection<Ref> UsagesOfParam(this Node node, Param param)
        {
            return node.MkArray().UsagesOfParam(param);
        }

        public static ReadOnlyCollection<Param> UsedParams(this Block block)
        {
            return ((Node)block).UsedParams();
        }

        public static ReadOnlyDictionary<Param, ReadOnlyCollection<Ref>> UsagesOfParams(this Block block)
        {
            return ((Node)block).UsagesOfParams();
        }

        public static ReadOnlyCollection<Ref> UsagesOfParam(this Block block, Param param)
        {
            return ((Node)block).UsagesOfParam(param);
        }

        public static ReadOnlyCollection<Param> UsedParams(this IEnumerable<Node> nodes)
        {
            return nodes.SelectMany(node => node.CSharpEvaluationOrder().OfType<Ref>()
                .Select(@ref => @ref.Sym).OfType<Param>())
                .Distinct().ToReadOnly();
        }

        public static ReadOnlyDictionary<Param, ReadOnlyCollection<Ref>> UsagesOfParams(this IEnumerable<Node> nodes)
        {
            return nodes.SelectMany(node => node.CSharpEvaluationOrder().OfType<Ref>()
                .Where(@ref => @ref.Sym != null && @ref.Sym.IsParam()))
                .GroupBy(@ref => @ref.Sym.AssertCast<Param>(), @ref => @ref)
                .ToDictionary(g => g.Key, g => g.ToReadOnly()).ToReadOnly();
        }

        public static ReadOnlyCollection<Ref> UsagesOfParam(this IEnumerable<Node> nodes, Param param)
        {
            return nodes.SelectMany(node => node.CSharpEvaluationOrder().OfType<Ref>().Where(@ref => @ref.Sym == param)).ToReadOnly();
        }
    }
}