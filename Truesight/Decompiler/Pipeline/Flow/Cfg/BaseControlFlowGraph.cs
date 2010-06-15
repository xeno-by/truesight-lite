using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using QuickGraph;
using QuickGraph.Algorithms.Search;
using QuickGraph.Graphviz;
using QuickGraph.Graphviz.Dot;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Collections;
using QuickGraph.Algorithms;
using XenoGears.Traits.Dumpable;

namespace Truesight.Decompiler.Pipeline.Flow.Cfg
{
    [DumpFormat(NullObjectFormat = "", DefaultExtension = "dotgraph")]
    [DebuggerDisplay("{ToString(), nq}")]
    [DebuggerTypeProxy(typeof(DebugView))]
    internal abstract class BaseControlFlowGraph : BaseNamedEntity, IMutableVertexAndEdgeListGraph<ControlFlowBlock, ControlFlowEdge>, IDumpableAsText
    {
        public abstract ControlFlowBlock Start { get; }
        public abstract ControlFlowBlock Finish { get; }
        protected abstract IList<ControlFlowBlock> _vertices { get; }
        protected abstract IList<ControlFlowEdge> _edges { get; }

        protected BaseControlFlowGraph()
        {
            HookClearControlFlowCaches();
            HookSyncEdgeLookupCache();
        }

        protected BaseControlFlowGraph(BaseControlFlowGraph proto, bool deep)
            : this()
        {
            proto.Vertices.ForEach(v => AddVertex(v));
            proto.Edges().ForEach(e => AddEdge(deep ? e.ShallowClone() : e));
        }

        public new BaseControlFlowGraph SetName(String name) { return (BaseControlFlowGraph)base.SetName(name); }
        public new BaseControlFlowGraph SetName(Func<String> name) { return (BaseControlFlowGraph)base.SetName(name); }

        #region Vertices API

        public event VertexAction<ControlFlowBlock> VertexAdded;
        public event VertexAction<ControlFlowBlock> VertexRemoved;

//        protected abstract List<ControlFlowBlock> _vertices { get; }
        public ReadOnlyCollection<ControlFlowBlock> Vertices { get { return _vertices.ToReadOnly(); } }
//        public abstract ControlFlowBlock Start { get; }
//        public abstract ControlFlowBlock Finish { get; }

        public bool AddVertex(ControlFlowBlock v)
        {
            if (_vertices.Contains(v))
            {
                return false;
            }
            else
            {
                _vertices.Add(v);
                OnVertexAdded(v);
                return true;
            }
        }

        public int AddVertices(params ControlFlowBlock[] vertices)
        {
            return AddVertices((IEnumerable<ControlFlowBlock>)vertices);
        }

        public int AddVertices(IEnumerable<ControlFlowBlock> vertices)
        {
            return vertices.Select(v => AddVertex(v) ? 1 : 0).Sum();
        }

        int IMutableVertexSet<ControlFlowBlock>.AddVertexRange(IEnumerable<ControlFlowBlock> vertices)
        {
            return AddVertices(vertices);
        }

        public bool RemoveVertex(ControlFlowBlock v)
        {
            if (!Vertices.Contains(v))
            {
                return false;
            }
            else
            {
                var sentenced = _edges.Where(e => e.Source == v || e.Target == v).ToReadOnly();
                sentenced.ForEach(e => { _edges.Remove(e); OnEdgeRemoved(e); });

                _vertices.Remove(v);
                OnVertexRemoved(v);
                return true;
            }
        }

        public int RemoveVertices(params ControlFlowBlock[] vertices)
        {
            return RemoveVertices((IEnumerable<ControlFlowBlock>)vertices);
        }

        public int RemoveVertices(IEnumerable<ControlFlowBlock> vertices)
        {
            return vertices.Select(v => RemoveVertex(v) ? 1 : 0).Sum();
        }

        public int RemoveVertexIf(VertexPredicate<ControlFlowBlock> predicate)
        {
            var sentenced = Vertices.Where(v => predicate(v)).ToList();
            sentenced.ForEach(v => RemoveVertex(v));
            return sentenced.Count();
        }

        #endregion

        #region Edges read-only API

        public event EdgeAction<ControlFlowBlock, ControlFlowEdge> EdgeAdded;
        public event EdgeAction<ControlFlowBlock, ControlFlowEdge> EdgeRemoved;
        public ReadOnlyCollection<ControlFlowEdge> Edges() { return _edges.ToReadOnly(); }

        private readonly Dictionary<ControlFlowBlock, Dictionary<ControlFlowBlock, ControlFlowEdge>> 
            _inEdgesCache = new Dictionary<ControlFlowBlock, Dictionary<ControlFlowBlock, ControlFlowEdge>>();
        private Dictionary<ControlFlowBlock, ControlFlowEdge> _iec(ControlFlowBlock cfb) {
            return _inEdgesCache.GetOrCreate(cfb, () => new Dictionary<ControlFlowBlock, ControlFlowEdge>()); }
        private readonly Dictionary<ControlFlowBlock, Dictionary<ControlFlowBlock, ControlFlowEdge>> 
            _outEdgesCache = new Dictionary<ControlFlowBlock, Dictionary<ControlFlowBlock, ControlFlowEdge>>();
        private Dictionary<ControlFlowBlock, ControlFlowEdge> _oec(ControlFlowBlock cfb) {
            return _outEdgesCache.GetOrCreate(cfb, () => new Dictionary<ControlFlowBlock, ControlFlowEdge>()); }

        private void HookSyncEdgeLookupCache()
        {
            this.EdgeAdded += e =>
            {
                _oec(e.Source).Add(e.Target, e);
                _iec(e.Target).Add(e.Source, e);
            };

            this.EdgeRemoved += e =>
            {
                _oec(e.Source).Remove(e.Target);
                _iec(e.Target).Remove(e.Source);
            };
        }

        public ControlFlowEdge Vedge(ControlFlowBlock source, ControlFlowBlock target)
        {
            if (source == null || target == null)
            {
                IEnumerable<ControlFlowEdge> vedges;
                if (source == null && target == null)
                {
                    vedges = Edges();
                }
                else if (source != null && target == null)
                {
                    vedges = Vedges(source, null);
                }
                else
                {
                    vedges = Vedges(null, target);
                }

                return vedges.SingleOrDefault();
            }
            else
            {
                return _oec(source).GetOrDefault(target);
            }
        }

        public ReadOnlyCollection<ControlFlowEdge> Vedges(ControlFlowBlock source, IEnumerable<ControlFlowBlock> targets)
        {
            if (source != null && targets == null)
            {
                return _oec(source).Values.ToReadOnly();
            }
            else
            {
                return Edges(source == null ? null : source.MkArray(), targets);
            }
        }

        public ReadOnlyCollection<ControlFlowEdge> Vedges(IEnumerable<ControlFlowBlock> sources, ControlFlowBlock target)
        {
            if (sources == null && target != null)
            {
                return _iec(target).Values.ToReadOnly();
            }
            else
            {
                return Edges(sources, target == null ? null : target.MkArray());
            }
        }

        public ReadOnlyCollection<ControlFlowEdge> Edges(IEnumerable<ControlFlowBlock> sources, IEnumerable<ControlFlowBlock> targets)
        {
            Func<ControlFlowBlock, bool> sourceFilter = cfb => sources == null ? true : sources.Contains(cfb);
            Func<ControlFlowBlock, bool> targetFilter = cfb => targets == null ? true : targets.Contains(cfb);
            return Edges(sourceFilter, targetFilter);
        }

        public ReadOnlyCollection<ControlFlowEdge> Edges(Func<ControlFlowBlock, bool> sourceFilter, Func<ControlFlowBlock, bool> targetFilter)
        {
            sourceFilter = sourceFilter ?? ((Func<ControlFlowBlock, bool>)(cfb => true));
            targetFilter = targetFilter ?? ((Func<ControlFlowBlock, bool>)(cfb => true));
            return Edges().Where(e => sourceFilter(e.Source) && targetFilter(e.Target)).ToReadOnly();
        }

        private ReadOnlyCollection<ControlFlowEdge> _backEdgesCache = null;
        private ReadOnlyCollection<ControlFlowEdge> _treeEdgesCache = null;
        private void EnsureDfs()
        {
            if (_backEdgesCache == null || _treeEdgesCache == null)
            {
                var backEdges = new List<ControlFlowEdge>();
                var treeEdges = new List<ControlFlowEdge>();

                var dfs = new DepthFirstSearchAlgorithm<ControlFlowBlock, ControlFlowEdge>(this);
                dfs.BackEdge += backEdges.Add;
                dfs.TreeEdge += treeEdges.Add;
                dfs.ForwardOrCrossEdge += treeEdges.Add;
                dfs.Compute(Start);

                _backEdgesCache = backEdges.ToReadOnly();
                _treeEdgesCache = treeEdges.ToReadOnly();
            }
        }

        // todo. I humbly assume that DFS correctly finds all back-edges of the flow graph
        // where "back-edge" is defined as "edge that indicates a loop"
        public ReadOnlyCollection<ControlFlowEdge> BackEdges()
        {
            EnsureDfs();
            return _backEdgesCache;
        }

        public ControlFlowEdge BackVedge(ControlFlowBlock source, ControlFlowBlock target)
        {
            return BackEdges(source == null ? null : source.MkArray(), target == null ? null : target.MkArray()).SingleOrDefault();
        }

        public ReadOnlyCollection<ControlFlowEdge> BackVedges(ControlFlowBlock source, IEnumerable<ControlFlowBlock> targets)
        {
            return BackEdges(source == null ? null : source.MkArray(), targets);
        }

        public ReadOnlyCollection<ControlFlowEdge> BackVedges(IEnumerable<ControlFlowBlock> sources, ControlFlowBlock target)
        {
            return BackEdges(sources, target == null ? null : target.MkArray());
        }

        public ReadOnlyCollection<ControlFlowEdge> BackEdges(IEnumerable<ControlFlowBlock> sources, IEnumerable<ControlFlowBlock> targets)
        {
            Func<ControlFlowBlock, bool> sourceFilter = cfb => sources == null ? true : sources.Contains(cfb);
            Func<ControlFlowBlock, bool> targetFilter = cfb => targets == null ? true : targets.Contains(cfb);
            return BackEdges(sourceFilter, targetFilter);
        }

        public ReadOnlyCollection<ControlFlowEdge> BackEdges(Func<ControlFlowBlock, bool> sourceFilter, Func<ControlFlowBlock, bool> targetFilter)
        {
            sourceFilter = sourceFilter ?? ((Func<ControlFlowBlock, bool>)(cfb => true));
            targetFilter = targetFilter ?? ((Func<ControlFlowBlock, bool>)(cfb => true));
            return BackEdges().Where(e => sourceFilter(e.Source) && targetFilter(e.Target)).ToReadOnly();
        }

        // todo. I humbly assume that DFS correctly finds all tree-edges of the flow graph
        // where "tree-edge" is defined as "edge that doesn't participate in a loop"
        public ReadOnlyCollection<ControlFlowEdge> TreeEdges()
        {
            EnsureDfs();
            return _treeEdgesCache;
        }

        public ControlFlowEdge TreeVedge(ControlFlowBlock source, ControlFlowBlock target)
        {
            return TreeEdges(source == null ? null : source.MkArray(), target == null ? null : target.MkArray()).SingleOrDefault();
        }

        public ReadOnlyCollection<ControlFlowEdge> TreeVedges(ControlFlowBlock source, IEnumerable<ControlFlowBlock> targets)
        {
            return TreeEdges(source == null ? null : source.MkArray(), targets);
        }

        public ReadOnlyCollection<ControlFlowEdge> TreeVedges(IEnumerable<ControlFlowBlock> sources, ControlFlowBlock target)
        {
            return TreeEdges(sources, target == null ? null : target.MkArray());
        }

        public ReadOnlyCollection<ControlFlowEdge> TreeEdges(IEnumerable<ControlFlowBlock> sources, IEnumerable<ControlFlowBlock> targets)
        {
            Func<ControlFlowBlock, bool> sourceFilter = cfb => sources == null ? true : sources.Contains(cfb);
            Func<ControlFlowBlock, bool> targetFilter = cfb => targets == null ? true : targets.Contains(cfb);
            return TreeEdges(sourceFilter, targetFilter);
        }

        public ReadOnlyCollection<ControlFlowEdge> TreeEdges(Func<ControlFlowBlock, bool> sourceFilter, Func<ControlFlowBlock, bool> targetFilter)
        {
            sourceFilter = sourceFilter ?? ((Func<ControlFlowBlock, bool>)(cfb => true));
            targetFilter = targetFilter ?? ((Func<ControlFlowBlock, bool>)(cfb => true));
            return TreeEdges().Where(e => sourceFilter(e.Source) && targetFilter(e.Target)).ToReadOnly();
        }

        #endregion

        #region Edges read-write API

        public bool AddEdge(ControlFlowEdge e)
        {
            if (Vedge(e.Source, e.Target) != null)
            {
                return false;
            }
            else
            {
                _vertices.Contains(e.Source).AssertTrue();
                _vertices.Contains(e.Target).AssertTrue();
                _edges.Add(e);
                OnEdgeAdded(e);
                return true;
            }
        }

        public int AddEdges(params ControlFlowEdge[] edges)
        {
            return AddEdges((IEnumerable<ControlFlowEdge>)edges);
        }

        public int AddEdges(IEnumerable<ControlFlowEdge> edges)
        {
            return edges.Select(e => AddEdge(e) ? 1 : 0).Sum();
        }

        int IMutableEdgeListGraph<ControlFlowBlock, ControlFlowEdge>.AddEdgeRange(IEnumerable<ControlFlowEdge> edges)
        {
            return AddEdges(edges);
        }

        public bool AddVerticesAndEdge(ControlFlowEdge e)
        {
            AddVertex(e.Source);
            AddVertex(e.Target);
            return AddEdge(e);
        }

        public int AddVerticesAndEdges(params ControlFlowEdge[] edges)
        {
            return AddVerticesAndEdges((IEnumerable<ControlFlowEdge>)edges);
        }

        public int AddVerticesAndEdges(IEnumerable<ControlFlowEdge> edges)
        {
            return edges.Select(e => AddVerticesAndEdge(e) ? 1 : 0).Sum();
        }

        int IMutableVertexAndEdgeSet<ControlFlowBlock, ControlFlowEdge>.AddVerticesAndEdgeRange(IEnumerable<ControlFlowEdge> edges)
        {
            return AddVerticesAndEdges(edges);
        }

        public bool RemoveEdge(ControlFlowEdge e)
        {
            if (_edges.Contains(e) && _edges.Remove(e))
            {
                OnEdgeRemoved(e);
                return true;
            }
            else
            {
                return false;
            }
        }

        public int RemoveEdges(params ControlFlowEdge[] edges)
        {
            return RemoveEdges((IEnumerable<ControlFlowEdge>)edges);
        }

        public int RemoveEdges(IEnumerable<ControlFlowEdge> edges)
        {
            return edges.Select(e => RemoveEdge(e) ? 1 : 0).Sum();
        }

        public int RemoveEdgeIf(EdgePredicate<ControlFlowBlock, ControlFlowEdge> predicate)
        {
            var sentenced = _edges.Where(e => predicate(e)).ToReadOnly();
            return sentenced.Select(e => RemoveEdge(e)).Count(wasRemoved => wasRemoved);
        }

        public void Clear()
        {
            Vertices.ForEach(v => RemoveVertex(v));
        }

        #endregion

        #region Clone API

        public ControlFlowGraph ShallowClone()
        {
            return new ControlFlowGraph(this, false);
        }

        public ControlFlowGraph DeepClone()
        {
            return new ControlFlowGraph(this, true);
        }

        #endregion

        #region View API

        // sequences

        public ViewOfControlFlowGraph CreateView(IEnumerable<ControlFlowBlock> vertices) { return new ViewOfControlFlowGraph(this, vertices); }
        public ViewOfControlFlowGraph CreateView(IEnumerable<ControlFlowBlock> vertices, Action<ControlFlowEdge, ViewOfControlFlowGraph> onAlienEdge) { return new ViewOfControlFlowGraph(this, vertices, onAlienEdge); }
        public ViewOfControlFlowGraph CreateView(IEnumerable<ControlFlowEdge> edges) { return new ViewOfControlFlowGraph(this, edges); }
        public ViewOfControlFlowGraph CreateView(IEnumerable<ControlFlowEdge> edges, Action<ControlFlowEdge, ViewOfControlFlowGraph> onAlienEdge) { return new ViewOfControlFlowGraph(this, edges, onAlienEdge); }
        public ViewOfControlFlowGraph CreateView(IEnumerable<ControlFlowBlock> vertices, IEnumerable<ControlFlowEdge> edges) { return new ViewOfControlFlowGraph(this, vertices, edges); }
        public ViewOfControlFlowGraph CreateView(IEnumerable<ControlFlowBlock> vertices, IEnumerable<ControlFlowEdge> edges, Action<ControlFlowEdge, ViewOfControlFlowGraph> onAlienEdge) { return new ViewOfControlFlowGraph(this, vertices, edges, onAlienEdge); }

        // functions

        public ViewOfControlFlowGraph CreateView(Func<ControlFlowBlock, bool> vertexFilter) { return new ViewOfControlFlowGraph(this, vertexFilter); }
        public ViewOfControlFlowGraph CreateView(Func<ControlFlowBlock, bool> vertexFilter, Action<ControlFlowEdge, ViewOfControlFlowGraph> onAlienEdge) { return new ViewOfControlFlowGraph(this, vertexFilter, onAlienEdge); }
        public ViewOfControlFlowGraph CreateView(Func<ControlFlowEdge, bool> edgeFilter) { return new ViewOfControlFlowGraph(this, edgeFilter); }
        public ViewOfControlFlowGraph CreateView(Func<ControlFlowEdge, bool> edgeFilter, Action<ControlFlowEdge, ViewOfControlFlowGraph> onAlienEdge) { return new ViewOfControlFlowGraph(this, edgeFilter, onAlienEdge); }
        public ViewOfControlFlowGraph CreateView(Func<ControlFlowBlock, bool> vertexFilter, Func<ControlFlowEdge, bool> edgeFilter) { return new ViewOfControlFlowGraph(this, vertexFilter, edgeFilter); }
        public ViewOfControlFlowGraph CreateView(Func<ControlFlowBlock, bool> vertexFilter, Func<ControlFlowEdge, bool> edgeFilter, Action<ControlFlowEdge, ViewOfControlFlowGraph> onAlienEdge) { return new ViewOfControlFlowGraph(this, vertexFilter, edgeFilter, onAlienEdge); }

        // hybrids

        public ViewOfControlFlowGraph CreateView(Func<ControlFlowBlock, bool> vertexFilter, IEnumerable<ControlFlowEdge> edges) { return new ViewOfControlFlowGraph(this, vertexFilter, edges); }
        public ViewOfControlFlowGraph CreateView(Func<ControlFlowBlock, bool> vertexFilter, IEnumerable<ControlFlowEdge> edges, Action<ControlFlowEdge, ViewOfControlFlowGraph> onAlienEdge) { return new ViewOfControlFlowGraph(this, vertexFilter, edges, onAlienEdge); }
        public ViewOfControlFlowGraph CreateView(IEnumerable<ControlFlowBlock> vertices, Func<ControlFlowEdge, bool> edgeFilter) { return new ViewOfControlFlowGraph(this, vertices, edgeFilter); }
        public ViewOfControlFlowGraph CreateView(IEnumerable<ControlFlowBlock> vertices, Func<ControlFlowEdge, bool> edgeFilter, Action<ControlFlowEdge, ViewOfControlFlowGraph> onAlienEdge) { return new ViewOfControlFlowGraph(this, vertices, edgeFilter, onAlienEdge); }

        #endregion

        #region Dump API

        public override String ToString()
        {
            var fmt = String.Format("{0} vertices, {1} edges", Vertices.Count(), Edges().Count());
            if (Name.IsNeitherNullNorEmpty()) fmt = Name + " (" + fmt + ")";
            return fmt;
        }

        private class DebugView
        {
            private readonly BaseControlFlowGraph _g;
            public DebugView(BaseControlFlowGraph g)
            {
                _g = g;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public ReadOnlyCollection<ControlFlowEdge> Edges
            {
                get { return _g.Edges(); }
            }

            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public ReadOnlyCollection<ControlFlowBlock> Vertices
            {
                get { return _g.Vertices; }
            }
        }

        void IDumpableAsText.DumpAsText(TextWriter writer) { DumpAsText(writer); }
        protected virtual void DumpAsText(TextWriter writer)
        {
            var algo = new GraphvizAlgorithm<ControlFlowBlock, ControlFlowEdge>(this);
            algo.FormatVertex += (o, e) => FormatVertex(e.Vertex, e.VertexFormatter);
            algo.FormatEdge += (o, e) => FormatEdge(e.Edge, e.EdgeFormatter);

            var dotFile = Path.GetTempFileName() + ".dot";
            algo.Generate(new FileDotEngine(), dotFile);
            writer.Write(File.ReadAllText(dotFile));
        }

        protected virtual void FormatVertex(ControlFlowBlock cfb, GraphvizVertex fmt)
        {
            if (cfb.Name == "start")
            {
                fmt.Label = "start";
            }
            else if (cfb.Name == "finish")
            {
                fmt.Label = "finish";
            }
            else
            {
                var match = Regex.Match(cfb.DumpAsText(), @"^(?<header>.*?)?(\r\n(?<code>.*))?$", RegexOptions.Singleline);
                match.Success.AssertTrue();

                var header = match.Result("${header}");
                if (header.IsNeitherNullNorEmpty())
                {
                    var cfi = Cflow().IndexOf(cfb);
                    header = header + String.Format(", cfi={0}", cfi == -1 ? "*" : cfi.ToString());
                }

                var code = match.Result("${code}");
                fmt.Label = new[] { header, code }.Where(s => s.IsNeitherNullNorEmpty()).StringJoin(Environment.NewLine);
            }
        }

        protected virtual void FormatEdge(ControlFlowEdge e, GraphvizEdge fmt)
        {
            fmt.Label = new GraphvizEdgeLabel { Value = e.IsConditional ? e.Tag.ToSymbol() : null };
            fmt.Style = BackEdges().Contains(e) ? GraphvizEdgeStyle.Dotted : GraphvizEdgeStyle.Solid;
        }

        #endregion

        #region Control flow API

        private void HookClearControlFlowCaches()
        {
            Action clearCaches = () =>
            {
                _backEdgesCache = null;
                _treeEdgesCache = null;
                _dagCache = null;
                _dagSortedCache = null;
                _cflowCache = null;
            };

            VertexAdded += e => clearCaches();
            VertexRemoved += e => clearCaches();
            EdgeAdded += e => clearCaches();
            EdgeRemoved += e => clearCaches();
        }

        private BaseControlFlowGraph _dagCache;
        private ReadOnlyDictionary<ControlFlowBlock, int> _dagSortedCache;
        public BaseControlFlowGraph DAG
        {
            get
            {
                if (_dagCache == null || _dagSortedCache == null)
                {
                    var view = CreateView(TreeEdges()).InheritStartAndFinish();
                    _dagCache = view.ShallowClone().SetName(Name + ".DAG");

                    var topo = _dagCache.TopologicalSort();
                    _dagSortedCache = topo.ToDictionary(v => v, v => topo.IndexOf(v)).ToReadOnly();
                }

                return _dagCache;
            }
        }

        private Dictionary<ControlFlowBlock, ReadOnlyCollection<ControlFlowBlock>> _cflowCache;
        public ReadOnlyCollection<ControlFlowBlock> Cflow() { return Cflow(Start); }
        public ReadOnlyCollection<ControlFlowBlock> Cflow(ControlFlowBlock cfb)
        {
            _cflowCache = _cflowCache ?? new Dictionary<ControlFlowBlock, ReadOnlyCollection<ControlFlowBlock>>();
            if (!_cflowCache.ContainsKey(cfb))
            {
                // todo. this works in assumption that removing all back-edges won't break connectivity
                var cflow = new List<ControlFlowBlock>();
                var dfs = new DepthFirstSearchAlgorithm<ControlFlowBlock, ControlFlowEdge>(DAG);
                dfs.DiscoverVertex += cflow.Add;
                dfs.Compute(cfb);

                // todo. check how this works with disconnected cfgs
                var ordered = cflow.OrderBy(v => _dagSortedCache[v]);
                _cflowCache[cfb] = ordered.ToReadOnly();
            }

            return _cflowCache[cfb];
        }

        public ReadOnlyCollection<ControlFlowBlock> Cflow(ControlFlowBlock vs, ControlFlowBlock ve)
        {
            if (vs == ve)
            {
                return vs.MkArray().ToReadOnly();
            }
            else
            {
                var cflow = Cflow(vs).Except(Cflow(ve)).Where(v => Cflow(v).Contains(ve)).ToList();
                if (cflow.IsNotEmpty()) cflow.Add(ve);
                return cflow.ToReadOnly();
            }
        }

        public ControlFlowBlock ConvStrict(ControlFlowBlock cfb)
        {
            var outEdges = TreeVedges(cfb, null);
            if (outEdges.Count() == 0)
            {
                return null;
            }
            else if (outEdges.Count() == 1)
            {
                return outEdges.AssertSingle().Target;
            }
            else
            {
                var v_out = outEdges.Select(e => e.Target);
                var common_cflow = v_out.Select(v => Cflow(v)).Intersect().ToReadOnly();
                return common_cflow.FirstOrDefault(conv =>
                {
                    var mid = Cflow(cfb).Except(Cflow(conv)).ToReadOnly();
                    var allCflowThruMid = Set.Equal(Edges(mid, Cflow(conv)), Vedges(mid, conv));
                    var noOffsprings = mid.All(v => Cflow(v).Contains(conv));
                    return allCflowThruMid && noOffsprings;
                });
            }
        }

        public ControlFlowBlock ConvNearest(ControlFlowBlock cfb)
        {
            var outEdges = TreeVedges(cfb, null);
            if (outEdges.Count() == 0)
            {
                return null;
            }
            else if (outEdges.Count() == 1)
            {
                return outEdges.AssertSingle().Target;
            }
            else
            {
                var v_out = outEdges.Select(e => e.Target);
                return v_out.Select(v => Cflow(v)).Intersect().FirstOrDefault();
            }
        }

        #endregion

        #region Implementation of IMutableVertexAndEdgeListGraph<ControlFlowBlock, ControlFlowEdge>

        bool IGraph<ControlFlowBlock, ControlFlowEdge>.AllowParallelEdges { get { return false; } }
        bool IGraph<ControlFlowBlock, ControlFlowEdge>.IsDirected { get { return true; } }

        bool IEdgeSet<ControlFlowBlock, ControlFlowEdge>.IsEdgesEmpty { get { return _edges.Count() == 0; } }
        int IEdgeSet<ControlFlowBlock, ControlFlowEdge>.EdgeCount { get { return _edges.Count(); } }
        IEnumerable<ControlFlowEdge> IEdgeSet<ControlFlowBlock, ControlFlowEdge>.Edges { get { return _edges; } }

        bool IVertexSet<ControlFlowBlock>.IsVerticesEmpty { get { return _vertices.Count == 0; } }
        int IVertexSet<ControlFlowBlock>.VertexCount { get { return _vertices.Count; } }
        IEnumerable<ControlFlowBlock> IVertexSet<ControlFlowBlock>.Vertices { get { return _vertices; } }

        protected void OnEdgeAdded(ControlFlowEdge args)
        {
            var edgeAdded = this.EdgeAdded;
            if (edgeAdded != null) edgeAdded(args);
        }

        protected void OnEdgeRemoved(ControlFlowEdge args)
        {
            var edgeRemoved = this.EdgeRemoved;
            if (edgeRemoved != null) edgeRemoved(args);
        }

        protected void OnVertexAdded(ControlFlowBlock args)
        {
            var vertexAdded = this.VertexAdded;
            if (vertexAdded != null) vertexAdded(args);
        }

        protected void OnVertexRemoved(ControlFlowBlock args)
        {
            var vertexRemoved = this.VertexRemoved;
            if (vertexRemoved != null) vertexRemoved(args);
        }

        bool IEdgeSet<ControlFlowBlock, ControlFlowEdge>.ContainsEdge(ControlFlowEdge edge)
        {
            return _edges.Contains(edge);
        }

        bool IImplicitVertexSet<ControlFlowBlock>.ContainsVertex(ControlFlowBlock v)
        {
            return _vertices.Contains(v);
        }

        bool IImplicitGraph<ControlFlowBlock, ControlFlowEdge>.IsOutEdgesEmpty(ControlFlowBlock v)
        {
            return Vedges(v, null).IsEmpty();
        }

        int IImplicitGraph<ControlFlowBlock, ControlFlowEdge>.OutDegree(ControlFlowBlock v)
        {
            return Vedges(v, null).Count();
        }

        IEnumerable<ControlFlowEdge> IImplicitGraph<ControlFlowBlock, ControlFlowEdge>.OutEdges(ControlFlowBlock v)
        {
            return Vedges(v, null);
        }

        bool IImplicitGraph<ControlFlowBlock, ControlFlowEdge>.TryGetOutEdges(ControlFlowBlock v, out IEnumerable<ControlFlowEdge> edges)
        {
            // there's no dictionary semantics, unlike in AdjancencyGraph implementation
            edges = Vedges(v, null);
            return true;
        }

        ControlFlowEdge IImplicitGraph<ControlFlowBlock, ControlFlowEdge>.OutEdge(ControlFlowBlock v, int index)
        {
            return Vedges(v, null).Nth(index);
        }

        bool IIncidenceGraph<ControlFlowBlock, ControlFlowEdge>.ContainsEdge(ControlFlowBlock source, ControlFlowBlock target)
        {
            return Vedge(source, target) != null;
        }

        bool IIncidenceGraph<ControlFlowBlock, ControlFlowEdge>.TryGetEdges(ControlFlowBlock source, ControlFlowBlock target, out IEnumerable<ControlFlowEdge> edges)
        {
            // there's no dictionary semantics, unlike in AdjancencyGraph implementation
            edges = Edges(source.MkArray(), target.MkArray());
            return true;
        }

        bool IIncidenceGraph<ControlFlowBlock, ControlFlowEdge>.TryGetEdge(ControlFlowBlock source, ControlFlowBlock target, out ControlFlowEdge edge)
        {
            // there's no dictionary semantics, unlike in AdjancencyGraph implementation
            edge = Vedge(source, target);
            return true;
        }

        int IMutableIncidenceGraph<ControlFlowBlock, ControlFlowEdge>.RemoveOutEdgeIf(ControlFlowBlock v, EdgePredicate<ControlFlowBlock, ControlFlowEdge> predicate)
        {
            return RemoveEdgeIf(predicate);
        }

        void IMutableIncidenceGraph<ControlFlowBlock, ControlFlowEdge>.ClearOutEdges(ControlFlowBlock v)
        {
            Vedges(v, null).ForEach(e => RemoveEdge(e));
        }

        void IMutableIncidenceGraph<ControlFlowBlock, ControlFlowEdge>.TrimEdgeExcess()
        {
            // do nothing
        }

        #endregion
    }
}