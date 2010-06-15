using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Collections.Virtual;
using XenoGears.Strings;

namespace Truesight.Decompiler.Pipeline.Flow.Cfg
{
    // todo. currently VoCFG doesn't necessarily preserve order of vertices
    // if that is important to you, replace hardcodes and cache with a hybrid of list and set
    // currently, I'm cba to implement and test this data structure, sorry

    internal class ViewOfControlFlowGraph : BaseControlFlowGraph
    {
        public BaseControlFlowGraph Source { get; private set; }

        #region Loads of constructor overloads

        // sequences

        public ViewOfControlFlowGraph(BaseControlFlowGraph source, IEnumerable<ControlFlowBlock> vertices) { Initialize(source, vertices); }
        protected void Initialize(BaseControlFlowGraph source, IEnumerable<ControlFlowBlock> vertices)
        {
            Initialize(source, vertices, (IEnumerable<ControlFlowEdge>)null);
        }

        public ViewOfControlFlowGraph(BaseControlFlowGraph source, IEnumerable<ControlFlowBlock> vertices, Action<ControlFlowEdge, ViewOfControlFlowGraph> onAlienEdge) { Initialize(source, vertices, onAlienEdge); }
        protected void Initialize(BaseControlFlowGraph source, IEnumerable<ControlFlowBlock> vertices, Action<ControlFlowEdge, ViewOfControlFlowGraph> onAlienEdge)
        {
            Initialize(source, vertices, (IEnumerable<ControlFlowEdge>)null, onAlienEdge);
        }

        public ViewOfControlFlowGraph(BaseControlFlowGraph source, IEnumerable<ControlFlowEdge> edges) { Initialize(source, edges); }
        protected void Initialize(BaseControlFlowGraph source, IEnumerable<ControlFlowEdge> edges)
        {
            Initialize(source, (IEnumerable<ControlFlowBlock>)null, edges);
        }

        public ViewOfControlFlowGraph(BaseControlFlowGraph source, IEnumerable<ControlFlowEdge> edges, Action<ControlFlowEdge, ViewOfControlFlowGraph> onAlienEdge) { Initialize(source, edges, onAlienEdge); }
        protected void Initialize(BaseControlFlowGraph source, IEnumerable<ControlFlowEdge> edges, Action<ControlFlowEdge, ViewOfControlFlowGraph> onAlienEdge)
        {
            Initialize(source, (IEnumerable<ControlFlowBlock>)null, edges, onAlienEdge);
        }

        public ViewOfControlFlowGraph(BaseControlFlowGraph source, IEnumerable<ControlFlowBlock> vertices, IEnumerable<ControlFlowEdge> edges) { Initialize(source, vertices, edges); }
        protected void Initialize(BaseControlFlowGraph source, IEnumerable<ControlFlowBlock> vertices, IEnumerable<ControlFlowEdge> edges)
        {
            Initialize(source, vertices, edges, (e, _) => { throw AssertionHelper.Fail(); });
        }

        public ViewOfControlFlowGraph(BaseControlFlowGraph source, IEnumerable<ControlFlowBlock> vertices, IEnumerable<ControlFlowEdge> edges, Action<ControlFlowEdge, ViewOfControlFlowGraph> onAlienEdge) { Initialize(source, vertices, edges, onAlienEdge); }
        protected void Initialize(BaseControlFlowGraph source, IEnumerable<ControlFlowBlock> vertices, IEnumerable<ControlFlowEdge> edges, Action<ControlFlowEdge, ViewOfControlFlowGraph> onAlienEdge)
        {
            _hardcodedVertices = vertices != null ? vertices.ToHashSet() :
                (edges != null ? edges.SelectMany(e => new []{e.Source, e.Target}).ToHashSet() : null);
            var vertexFilter = _hardcodedVertices == null ? (cfb => true) : (Func<ControlFlowBlock, bool>)(cfb => _hardcodedVertices.Contains(cfb));
            _hardcodedEdges = edges == null ? null : edges.ToHashSet();
            var edgeFilter = edges == null ? (e => true) : ((Func<ControlFlowEdge, bool>)(e => _hardcodedEdges.Contains(e)));
            InitializeCore(source, vertexFilter, edgeFilter, onAlienEdge);
        }

        // functions

        public ViewOfControlFlowGraph(BaseControlFlowGraph source, Func<ControlFlowBlock, bool> vertexFilter) { Initialize(source, vertexFilter); }
        protected void Initialize(BaseControlFlowGraph source, Func<ControlFlowBlock, bool> vertexFilter)
        {
            Initialize(source, vertexFilter, (IEnumerable<ControlFlowEdge>)null);
        }

        public ViewOfControlFlowGraph(BaseControlFlowGraph source, Func<ControlFlowBlock, bool> vertexFilter, Action<ControlFlowEdge, ViewOfControlFlowGraph> onAlienEdge) { Initialize(source, vertexFilter, onAlienEdge); }
        protected void Initialize(BaseControlFlowGraph source, Func<ControlFlowBlock, bool> vertexFilter, Action<ControlFlowEdge, ViewOfControlFlowGraph> onAlienEdge)
        {
            Initialize(source, vertexFilter, (IEnumerable<ControlFlowEdge>)null, onAlienEdge);
        }

        public ViewOfControlFlowGraph(BaseControlFlowGraph source, Func<ControlFlowEdge, bool> edgeFilter) { Initialize(source, edgeFilter); }
        protected void Initialize(BaseControlFlowGraph source, Func<ControlFlowEdge, bool> edgeFilter)
        {
            Initialize(source, (IEnumerable<ControlFlowBlock>)null, edgeFilter);
        }

        public ViewOfControlFlowGraph(BaseControlFlowGraph source, Func<ControlFlowEdge, bool> edgeFilter, Action<ControlFlowEdge, ViewOfControlFlowGraph> onAlienEdge) { Initialize(source, edgeFilter, onAlienEdge); }
        protected void Initialize(BaseControlFlowGraph source, Func<ControlFlowEdge, bool> edgeFilter, Action<ControlFlowEdge, ViewOfControlFlowGraph> onAlienEdge)
        {
            Initialize(source, (IEnumerable<ControlFlowBlock>)null, edgeFilter, onAlienEdge);
        }

        public ViewOfControlFlowGraph(BaseControlFlowGraph source, Func<ControlFlowBlock, bool> vertexFilter, Func<ControlFlowEdge, bool> edgeFilter) { Initialize(source, vertexFilter, edgeFilter); }
        protected void Initialize(BaseControlFlowGraph source, Func<ControlFlowBlock, bool> vertexFilter, Func<ControlFlowEdge, bool> edgeFilter)
        {
            Initialize(source, vertexFilter, edgeFilter, (e, _) => { throw AssertionHelper.Fail(); });
        }

        public ViewOfControlFlowGraph(BaseControlFlowGraph source, Func<ControlFlowBlock, bool> vertexFilter, Func<ControlFlowEdge, bool> edgeFilter, Action<ControlFlowEdge, ViewOfControlFlowGraph> onAlienEdge) { Initialize(source, vertexFilter, edgeFilter, onAlienEdge); }
        protected void Initialize(BaseControlFlowGraph source, Func<ControlFlowBlock, bool> vertexFilter, Func<ControlFlowEdge, bool> edgeFilter, Action<ControlFlowEdge, ViewOfControlFlowGraph> onAlienEdge)
        {
            vertexFilter = vertexFilter ?? (cfb => true);
            edgeFilter = edgeFilter ?? (e => true);
            InitializeCore(source, vertexFilter, edgeFilter, onAlienEdge);
        }

        // hybrids

        public ViewOfControlFlowGraph(BaseControlFlowGraph source, Func<ControlFlowBlock, bool> vertexFilter, IEnumerable<ControlFlowEdge> edges) { Initialize(source, vertexFilter, edges); }
        protected void Initialize(BaseControlFlowGraph source, Func<ControlFlowBlock, bool> vertexFilter, IEnumerable<ControlFlowEdge> edges)
        {
            Initialize(source, vertexFilter, edges, (e, _) => { throw AssertionHelper.Fail(); });
        }

        public ViewOfControlFlowGraph(BaseControlFlowGraph source, Func<ControlFlowBlock, bool> vertexFilter, IEnumerable<ControlFlowEdge> edges, Action<ControlFlowEdge, ViewOfControlFlowGraph> onAlienEdge) { Initialize(source, vertexFilter, edges, onAlienEdge); }
        protected void Initialize(BaseControlFlowGraph source, Func<ControlFlowBlock, bool> vertexFilter, IEnumerable<ControlFlowEdge> edges, Action<ControlFlowEdge, ViewOfControlFlowGraph> onAlienEdge)
        {
            _hardcodedEdges = edges == null ? null : edges.ToHashSet();
            var edgeFilter = edges == null ? (e => true) : ((Func<ControlFlowEdge, bool>)(e => _hardcodedEdges.Contains(e)));
            InitializeCore(source, vertexFilter, edgeFilter, onAlienEdge);
        }

        public ViewOfControlFlowGraph(BaseControlFlowGraph source, IEnumerable<ControlFlowBlock> vertices, Func<ControlFlowEdge, bool> edgeFilter) { Initialize(source, vertices, edgeFilter); }
        protected void Initialize(BaseControlFlowGraph source, IEnumerable<ControlFlowBlock> vertices, Func<ControlFlowEdge, bool> edgeFilter)
        {
            Initialize(source, vertices, edgeFilter, (v, _) => { throw AssertionHelper.Fail(); });
        }

        public ViewOfControlFlowGraph(BaseControlFlowGraph source, IEnumerable<ControlFlowBlock> vertices, Func<ControlFlowEdge, bool> edgeFilter, Action<ControlFlowEdge, ViewOfControlFlowGraph> onAlienEdge) { Initialize(source, vertices, edgeFilter, onAlienEdge); }
        protected void Initialize(BaseControlFlowGraph source, IEnumerable<ControlFlowBlock> vertices, Func<ControlFlowEdge, bool> edgeFilter, Action<ControlFlowEdge, ViewOfControlFlowGraph> onAlienEdge)
        {
            _hardcodedVertices = vertices == null ? null : vertices.ToHashSet();
            var vertexFilter = _hardcodedVertices == null ? (cfb => true) : (Func<ControlFlowBlock, bool>)(cfb => _hardcodedVertices.Contains(cfb));
            InitializeCore(source, vertexFilter, edgeFilter, onAlienEdge);
        }

        // for descendants who want to call Initialize by themselves
        // i.e. using parameters calculated in their constructor body

        protected ViewOfControlFlowGraph()
        {
        }

        #endregion

        #region Inherited vertices and edges

        private Func<ControlFlowBlock, bool> _vertexFilter;
        private Func<ControlFlowEdge, bool> _edgeFilter;
        private HashSet<ControlFlowBlock> _hardcodedVertices;
        private HashSet<ControlFlowEdge> _hardcodedEdges;
        private List<ControlFlowBlock> _cachedVertices;
        private List<ControlFlowEdge> _cachedEdges;
        private IList<ControlFlowBlock> __vertices;
        protected override IList<ControlFlowBlock> _vertices { get { return __vertices; } }
        private IList<ControlFlowEdge> __edges;
        protected override IList<ControlFlowEdge> _edges { get { return __edges; } }

        private void InitializeCore(
            BaseControlFlowGraph source,
            Func<ControlFlowBlock, bool> vertexFilter,
            Func<ControlFlowEdge, bool> edgeFilter,
            Action<ControlFlowEdge, ViewOfControlFlowGraph> onAlienEdge)
        {
            Source = source.AssertNotNull();
            HookUpVertexPostprocessors();

            _vertexFilter = vertexFilter.AssertNotNull();
            _edgeFilter = edgeFilter.AssertNotNull();

            _cachedVertices = _hardcodedVertices != null ? null : source.Vertices.Where(_vertexFilter).ToList();
            (_cachedVertices ?? _hardcodedVertices.AsEnumerable()).ForEach(OnVertexAdded);

            ReadOnlyCollection<ControlFlowEdge> alienEdges = null;
            if (_hardcodedEdges != null)
            {
                var edgeVertices = _hardcodedEdges.SelectMany(e => new []{e.Source, e.Target}).ToHashSet();
                edgeVertices.ExceptWith(_cachedVertices ?? _hardcodedVertices.AsEnumerable());
                edgeVertices.AssertEmpty();
            }
            else
            {
                _edgeFilter = e => edgeFilter(e) && _vertexFilter(e.Source) && _vertexFilter(e.Target);

                var relatedEdges = source.Edges(_vertexFilter, null).Concat(source.Edges(null, _vertexFilter)).Distinct();
                var parts = relatedEdges.GroupBy(e => _edgeFilter(e)).ToDictionary(g => g.Key, g => g.AsEnumerable());
                _cachedEdges = (parts.GetOrDefault(true, Seq.Empty<ControlFlowEdge>)).ToList();
                alienEdges = parts.GetOrDefault(false, Seq.Empty<ControlFlowEdge>).ToReadOnly();
            }
            (_cachedEdges ?? _hardcodedEdges.AsEnumerable()).ForEach(OnEdgeAdded);

            Action<ControlFlowBlock> cacheVertex = v => { if (_cachedVertices != null) _cachedVertices.Add(v); else throw AssertionHelper.Fail(); };
            Action<ControlFlowBlock> uncacheVertex = v => { if (_cachedVertices != null) _cachedVertices.Remove(v); else _hardcodedVertices.Remove(v); };
            source.VertexAdded += v => { if (_vertexFilter(v)) { cacheVertex(v); OnVertexAdded(v); } };
            source.VertexRemoved += v => { if (_vertexFilter(v)) { uncacheVertex(v); OnVertexRemoved(v); } };

            Action<ControlFlowEdge> cacheEdge = e => { if (_cachedEdges != null) _cachedEdges.Add(e); else throw AssertionHelper.Fail(); };
            Action<ControlFlowEdge> uncacheEdge = e => { if (_cachedEdges != null) _cachedEdges.Remove(e); else _hardcodedEdges.Remove(e); };
            source.EdgeAdded += e => { if (_edgeFilter(e)) { cacheEdge(e); OnEdgeAdded(e); } };
            source.EdgeRemoved += e => { if (_edgeFilter(e)) { uncacheEdge(e); OnEdgeRemoved(e); } };

            __vertices = new VirtualList<ControlFlowBlock>(
                () => (_hardcodedVertices ?? (IEnumerable<ControlFlowBlock>)_cachedVertices).Concat(_eigenVertices),
                (i, v) =>
                {
                    if (_eigenVertices.Contains(v))
                    {
                        // do nothing - the vertex has just been created by AddEigenVertex
                    }
                    else
                    {
                        _vertexFilter(v).AssertTrue();
                        (_cachedVertices != null && i == _cachedVertices.Count()).AssertTrue();
                        Source.AddVertex(v);
                    }
                },
                (i, v) => { _vertexFilter(v).AssertTrue(); throw AssertionHelper.Fail(); },
                i =>
                {
                    if (i < _cachedEdges.Count())
                    {
                        var v = _cachedVertices[i];
                        Source.RemoveVertex(v);
                    }
                    else
                    {
                        throw AssertionHelper.Fail();
                    }
                });

            __edges = new VirtualList<ControlFlowEdge>(
                () => (_hardcodedEdges ?? (IEnumerable<ControlFlowEdge>)_cachedEdges).Concat(_eigenEdges),
                (i, e) =>
                {
                    if (_eigenEdges.Contains(e))
                    {
                        // do nothing - the edge has just been created by AddEigenEdge
                    }
                    else
                    {
                        _edgeFilter(e).AssertTrue();
                        (_cachedEdges != null && i == _cachedEdges.Count()).AssertTrue();
                        Source.AddEdge(e);
                    }
                },
                (i, e) => { _edgeFilter(e).AssertTrue(); throw AssertionHelper.Fail(); },
                i =>
                {
                    if (i < _cachedEdges.Count())
                    {
                        var e = _cachedEdges[i];
                        Source.RemoveEdge(e);
                    }
                    else
                    {
                        var e = _eigenEdges[i - _cachedEdges.Count()];
                        _eigenEdges.Remove(e);
                    }
                });

            try { _allowAutoCreateStartAndFinish = true; alienEdges.ForEach(e => onAlienEdge(e, this)); }
            finally { _allowAutoCreateStartAndFinish = false; }
        }

        private void HookUpVertexPostprocessors()
        {
            VertexAdded += v =>
            {
                if (v.Name.IsNullOrEmpty())
                {
                    var genuine = Source;
                    while (genuine is ViewOfControlFlowGraph) genuine = ((ViewOfControlFlowGraph)genuine).Source;
                    var index = genuine.AssertCast<ControlFlowGraph>()._allTimeVertexCounter++;

                    (index >= 2).AssertTrue();
                    v.SetName((index - 2).SZtoAAA());
                }
            };

            VertexRemoved += e => (e != _start && e != _finish).AssertTrue();
        }

        #endregion

        #region Eigen vertices and edges

        private readonly List<ControlFlowBlock> _eigenVertices = new List<ControlFlowBlock>();
        private readonly List<ControlFlowEdge> _eigenEdges = new List<ControlFlowEdge>();

        public bool AddEigenVertex(ControlFlowBlock v)
        {
            if (_vertices.Contains(v))
            {
                _eigenVertices.Contains(v).AssertTrue();
                return false;
            }
            else
            {
                _eigenVertices.Add(v);
                OnVertexAdded(v);
                return true;
            }
        }

        public int AddEigenVertices(params ControlFlowBlock[] vertices)
        {
            return AddEigenVertices((IEnumerable<ControlFlowBlock>)vertices);
        }

        public int AddEigenVertices(IEnumerable<ControlFlowBlock> vertices)
        {
            return vertices.Select(v => AddEigenVertex(v) ? 1 : 0).Sum();
        }

        public bool AddEigenEdge(ControlFlowEdge e)
        {
            var existing = Vedge(e.Source, e.Target);
            if (existing != null)
            {
                _eigenEdges.Contains(existing).AssertFalse();
                return false;
            }
            else
            {
                _vertices.Contains(e.Source).AssertTrue();
                _vertices.Contains(e.Target).AssertTrue();
                _eigenEdges.Add(e);
                OnEdgeAdded(e);
                return true;
            }
        }

        public int AddEigenEdges(params ControlFlowEdge[] edges)
        {
            return AddEigenEdges((IEnumerable<ControlFlowEdge>)edges);
        }

        public int AddEigenEdges(IEnumerable<ControlFlowEdge> edges)
        {
            return edges.Select(e => AddEigenEdge(e) ? 1 : 0).Sum();
        }

        #endregion

        #region Start and finish vertices (might be eigenvertices)

        private ControlFlowBlock _start;
        private ControlFlowBlock _finish;
        private bool _allowAutoCreateStartAndFinish = false;

        public override ControlFlowBlock Start
        {
            get
            {
                if (_start == null && _allowAutoCreateStartAndFinish)
                    CreateEigenStart();

                return _start;
            }
        }

        public override ControlFlowBlock Finish
        {
            get
            {
                if (_finish == null && _allowAutoCreateStartAndFinish)
                    CreateEigenFinish();

                return _finish;
            }
        }

        public ViewOfControlFlowGraph CreateEigenStart()
        {
            _start.AssertNull();
            var eigenStart = new ControlFlowBlock().SetName("start");
            AddEigenVertex(eigenStart);
            _start = eigenStart;
            return this;
        }

        public ViewOfControlFlowGraph CreateEigenFinish()
        {
            _finish.AssertNull();
            var eigenFinish = new ControlFlowBlock().SetName("finish");
            AddEigenVertex(eigenFinish);
            _finish = eigenFinish;
            return this;
        }

        public ViewOfControlFlowGraph CreateEigenStartAndFinish()
        {
            CreateEigenStart();
            CreateEigenFinish();
            return this;
        }

        public ViewOfControlFlowGraph InheritStart()
        {
            return InheritStart(Source.Start);
        }

        public ViewOfControlFlowGraph InheritStart(ControlFlowBlock start)
        {
            _start.AssertNull();
            if (!_vertices.Contains(start)) AddEigenVertex(start);
            _start = start.AssertNotNull();
            return this;
        }

        public ViewOfControlFlowGraph InheritFinish()
        {
            return InheritFinish(Source.Finish);
        }

        public ViewOfControlFlowGraph InheritFinish(ControlFlowBlock finish)
        {
            _finish.AssertNull();
            if (!_vertices.Contains(finish)) AddEigenVertex(finish);
            _finish = finish.AssertNotNull();
            return this;
        }

        public ViewOfControlFlowGraph InheritStartAndFinish()
        {
            InheritStart();
            InheritFinish();
            return this;
        }

        public ViewOfControlFlowGraph InheritStartAndFinish(ControlFlowBlock start, ControlFlowBlock finish)
        {
            InheritStart(start);
            InheritFinish(finish);
            return this;
        }

        #endregion

        #region Overriden dump API

        public new ViewOfControlFlowGraph SetName(String name) { return (ViewOfControlFlowGraph)base.SetName(name); }
        public new ViewOfControlFlowGraph SetName(Func<String> name) { return (ViewOfControlFlowGraph)base.SetName(name); }

        // todo. show the entire parent graph for the VoCFG, but shade away unnecessary details
        protected override void DumpAsText(TextWriter writer) { base.DumpAsText(writer); }
        public override String ToString()
        {
            var v_inh = Vertices.Count() - _eigenVertices.Count();
            var v_eigen = _eigenVertices.Count();
            var e_inh = Edges().Count() - _eigenEdges.Count();
            var e_eigen = _eigenEdges.Count();
            var fmt = String.Format("{0}+{1} vertices, {2}+{3} edges", v_inh, v_eigen, e_inh, e_eigen);
            fmt = String.Format("{0} (view of {1})", fmt, Source);
            if (Name.IsNeitherNullNorEmpty()) fmt = Name + " (" + fmt + ")";
            return fmt;
        }

        #endregion
    }
}