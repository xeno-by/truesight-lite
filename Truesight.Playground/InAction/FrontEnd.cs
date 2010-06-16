using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Truesight.Decompiler;
using Truesight.Decompiler.Hir;
using Truesight.Decompiler.Hir.Core.ControlFlow;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Core.Functional;
using Truesight.Decompiler.Hir.Core.Scopes;
using Truesight.Decompiler.Hir.Core.Symbols;
using Truesight.Decompiler.Hir.Traversal;
using Truesight.Decompiler.Hir.Traversal.Transformers;
using Truesight.Playground.InAction.Domain;
using Truesight.Playground.InAction.Runtimes;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Reflection;
using XenoGears.Reflection.Emit;
using XenoGears.Reflection.Shortcuts;

namespace Truesight.Playground.InAction
{
    internal partial class Crosscompiler<T1, T2, T3>
    {
        private readonly Type _t_kernel;
        private readonly MethodInfo _m_kernel;
        private readonly TypeBuilder _t_xformed;
        private readonly MethodBuilder _m_xformed;

        private readonly Block _hir;
        private const int allocGlobal = 2, allocLocal = 1, allocPrivate = 0;
        private readonly ReadOnlyCollection<Param> _params;
        private readonly ReadOnlyCollection<Local> _locals;
        private Dictionary<Local, int> _allocs;
        private int Alloc(Sym sym) { return _allocs[sym.AssertCast<Local>()]; }
        private int Alloc(Ref @ref) { return _allocs[@ref.Sym.AssertCast<Local>()]; }
        private readonly ReadOnlyCollection<Eval> _callsToSyncThreads;

        private readonly Block _xhir;
        private readonly Param _this = new Param("x_this", typeof(IKernel<T1, T2, T3>));
        private readonly Param _blockIdx = new Param("blockIdx", typeof(int3));
        private readonly Dictionary<String, Local> _tids = new Dictionary<String, Local>();
        private readonly Dictionary<Local, Local> _replicatedLocals = new Dictionary<Local, Local>();
        private readonly Dictionary<Local, Assign> _replicatedInits = new Dictionary<Local, Assign>();
        private readonly Dictionary<Local, bool> _needsReplication = new Dictionary<Local, bool>();
        private readonly List<Local> _liftedLocals = new List<Local>();
        private int _lastIndex = 0;

        public static void DoCrosscompile(Type t_kernel, TypeBuilder t_xformed) { new Crosscompiler<T1, T2, T3>(t_kernel, t_xformed); }
        private Crosscompiler(Type t_kernel, TypeBuilder t_xformed)
        {
            _t_kernel = t_kernel;
            _m_kernel = _t_kernel.GetMethod("RunKernel", BF.All);
            ValidateInputParameters();

            // todo. think how to support multiple methods: *Dims, *Idxes, synchronization points
            // todo. I'd even say that we should embed constants instead of Dims and cache such bodies between calls
            // todo. if some dimension is equal to 1, then strip off corresponding loop
            var lam = _m_kernel.Decompile();
            _hir = lam.Body;
            _params = lam.Sig.Syms;
            _locals = lam.Body.LocalsRecursive();
            _xhir = new Block();
            _tids.Add("x", new Local("tid_x", typeof(int)));
            _tids.Add("y", new Local("tid_y", typeof(int)));
            _tids.Add("z", new Local("tid_z", typeof(int)));
            _xhir.Locals.AddElements(_tids.Values);
            InferLocalAllocationHints();
            ReplicatePrivatelyAllocatedLocals();
            LiftLocallyAllocatedLocals();
            HoistGloballyAllocatedLocals();
            _callsToSyncThreads = _hir.Family().OfType<Eval>().Where(eval =>
            {
                var m1 = eval.InvokedMethod();
                var syncapi_syncThreads = typeof(ISyncApi).GetMethod("SyncThreads");
                return m1 != null && m1 is MethodInfo && m1.Hierarchy().Contains(syncapi_syncThreads);
            }).ToReadOnly(); 
            TransformBlock(_hir, _xhir, false);

            // todo. currently we don't support debuggable IL
            // the latter implies correct PDB-mappings and working watch for replicated vars

            _t_xformed = t_xformed;
            _t_xformed.AddInterfaceImplementation(typeof(IBlockRunner<T1, T2, T3>));
            _m_xformed = _t_xformed.DefineMethod("RunBlock", MA.Public, typeof(void), new []{typeof(int3), typeof(T1), typeof(T2), typeof(T3)});
            _m_xformed.DefineParameter(1, ParmA.None, "blockIdx");
            _m_xformed.DefineParameter(2, ParmA.None, _m_kernel.GetParameters()[0].Name);
            _m_xformed.DefineParameter(3, ParmA.None, _m_kernel.GetParameters()[1].Name);
            _m_xformed.DefineParameter(4, ParmA.None, _m_kernel.GetParameters()[2].Name);
            CompileTransformedHir();
        }

        private void ValidateInputParameters()
        {
            // todo. currently we don't support sync points located outside of the main RunKernel method
            // todo. same for thread parameters (various *Idx) located outside of the main RunKernel method
            var forbiddenInAlgo = new HashSet<MethodBase>();
            forbiddenInAlgo.AddElements(typeof(ISyncApi).GetMethods().Cast<MethodBase>());
            forbiddenInAlgo.AddElements(typeof(IGridApi).GetProperties().Where(p => p.Name.EndsWith("Idx")).SelectMany(p => p.GetAccessors(true)).Cast<MethodBase>());
            foreach (var m in forbiddenInAlgo.ToArray())
            {
                var t_kernel = _m_kernel.DeclaringType;
                var imap = t_kernel.GetInterfaceMap(m.DeclaringType).MapInterfaceToImpl();
                forbiddenInAlgo.Add(imap[m]);
            }

            var algoMethods = _m_kernel.DeclaringType.GetMethods(BF.AllInstance | BF.DeclOnly)
                .Where(m => !m.Hierarchy().Contains(typeof(IKernel<T1, T2, T3>).GetMethod("RunKernel").AssertNotNull()))
                .Where(m => !forbiddenInAlgo.Contains(m)).ToReadOnly();
            algoMethods.ForEach(am => am.Decompile().Body.AssertThat(b => b.Family().None(c =>
            {
                var app = c as Apply;
                var lam = app == null ? null : app.Callee as Lambda;
                var m = lam == null ? null : lam.Method as MethodInfo;
                var h = m.Hierarchy().Cast<MethodBase>();
                return h != null && Set.Intersect(h, forbiddenInAlgo).IsNotEmpty();
            })));
        }

        private void InferLocalAllocationHints()
        {
            _allocs = _locals.ToDictionary(l => l, l => -1);
            _hir.Family().OfType<Eval>().ForEach(eval =>
            {
                var m = eval.InvokedMethod();
                var t = m == null ? null : m.DeclaringType;
                if (t != null && t.DeclaringType == typeof(Hints))
                {
                    // todo. support other hints as well
                    (t == typeof(Hints.SharingHint)).AssertTrue();

                    var args = eval.Callee.Args.AsEnumerable();
                    args = args.Skip(1.AssertThat(_ => m.IsInstance()));
                    m.IsVarargs().AssertTrue();
                    args = args.AssertSingle().AssertCast<CollectionInit>().Elements;

                    var alloc = m.Name == "Global" ? allocGlobal :
                        m.Name == "Local" ? allocLocal :
                        m.Name == "Private" ? allocPrivate :
                        ((Func<int>)(() => { throw AssertionHelper.Fail(); }))();
                    var locals = args.Select(e => e.AssertCast<Ref>().Sym.AssertCast<Local>());
                    locals.ForEach(l =>
                    {
                        (_allocs[l] == -1).AssertTrue();
                        _allocs[l] = alloc;
                    });
                }
            });
            _allocs.Where(kvp => kvp.Value == -1).ForEach(kvp => _allocs[kvp.Key] = allocPrivate);
        }

        private void ReplicatePrivatelyAllocatedLocals()
        {
            foreach (var local in _allocs.Where(kvp => kvp.Value == allocPrivate).Select(kvp => kvp.Key))
            {
                var replica = new Local(local.Name + "s", local.Type.MakeArrayType(3));
                var init = new Assign(new Ref(replica), new Eval(new Apply(
                    new Lambda(replica.Type.GetConstructor(3.Times(typeof(int)).ToArray())),
                    new Fld(typeof(int3).GetField("Z"), new Prop(typeof(IGridApi).GetProperty("BlockDim"), new Ref(_this))),
                    new Fld(typeof(int3).GetField("Y"), new Prop(typeof(IGridApi).GetProperty("BlockDim"), new Ref(_this))),
                    new Fld(typeof(int3).GetField("X"), new Prop(typeof(IGridApi).GetProperty("BlockDim"), new Ref(_this))))));
                _replicatedLocals.Add(local, replica);
                _replicatedInits.Add(local, init);
                _needsReplication.Add(local, false);
            }
        }

        private void LiftLocallyAllocatedLocals()
        {
            foreach (var local in _allocs.Where(kvp => kvp.Value == allocLocal).Select(kvp => kvp.Key))
            {
                var onlyAss = _hir.UsagesOfLocal(local).Select(u =>
                {
                    var ass = u.Parent as Assign;
                    var lhs = ass == null ? null : ass.Lhs as Ref;
                    var ok = lhs != null && lhs.Sym == local;
                    return ok ? ass.Rhs : null;
                }).Where(rhs => rhs != null).AssertSingle();

                // todo. currently we only support "this" symbols in locals' initializers
                onlyAss.UsedParams().AssertEach(_params.Contains);

                _liftedLocals.Add(local);
                _xhir.Locals.Add(local);
                _xhir.Insert(_lastIndex++, onlyAss.Parent.Transform((Ref @ref) => 
                    @ref.Sym == _params.First() ? new Ref(_this) : @ref.DefaultTransform()));
                onlyAss.Parent.RemoveSelf();
            }
        }

        private void HoistGloballyAllocatedLocals()
        {
            foreach (var local in _allocs.Where(kvp => kvp.Value == allocGlobal).Select(kvp => kvp.Key))
            {
                // todo. currently we don't support global locals since this requires extensive codegen
                throw new NotImplementedException();
            }
        }

        private void TransformBlock(Block hir, Block xhir, bool insideThreadLoop)
        {
            if (hir == null) return;

            var deepFission = new List<Node>();
            var regions = new List<ReadOnlyCollection<Node>>();
            var curr_region = new List<Node>();
            foreach (var stmt in hir)
            {
                if (_callsToSyncThreads.Contains(stmt as Eval))
                {
                    if (curr_region.IsNotEmpty()) regions.Add(curr_region.ToReadOnly());
                    curr_region = new List<Node>();
                }
                else if (_callsToSyncThreads.Any(c => Set.Intersect(c.Hierarchy(), stmt.MkArray()).IsNotEmpty()))
                {
                    if (curr_region.IsNotEmpty()) regions.Add(curr_region.ToReadOnly());
                    curr_region = new List<Node>();

                    deepFission.Add(stmt);
                    regions.Add(stmt.MkArray().ToReadOnly());
                }
                else
                {
                    curr_region.Add(stmt);
                }
            }
            if (curr_region.IsNotEmpty()) regions.Add(curr_region.ToReadOnly());
            curr_region = null;

            var privateLocals = hir.Locals.Where(l => Alloc(l) == allocPrivate).ToReadOnly();
            var privateUsages = privateLocals.ToDictionary(l => l, 
                l => regions.Where(r => r.UsagesOfLocal(l).IsNotEmpty()).ToReadOnly());
            privateUsages.ForEach(kvp =>
            {
                var needsReplication = kvp.Value.Count() > 1;
                _needsReplication[kvp.Key] = needsReplication;
                if (needsReplication)
                {
                    _xhir.Insert(_lastIndex++, _replicatedInits[kvp.Key]);
                    _xhir.Locals.Add(_replicatedLocals[kvp.Key]);
                }
            });

            foreach (var region in regions)
            {
                var xregion = new Block();

                var needsToBeWrapped = !deepFission.Contains(region.SingleOrDefault2());
                var regionIsInsideThreadLoop = insideThreadLoop || needsToBeWrapped;
                foreach (var stmt in region)
                {
                    if (stmt is Expression)
                    {
                        TransformExpression(((Expression)stmt), xregion, regionIsInsideThreadLoop);
                    }
                    else if (stmt is If)
                    {
                        TransformIf(((If)stmt), xregion, regionIsInsideThreadLoop);
                    }
                    else if (stmt is Loop)
                    {
                        TransformLoop(((Loop)stmt), xregion, regionIsInsideThreadLoop);
                    }
                    else
                    {
                        throw AssertionHelper.Fail();
                    }
                }

                if (needsToBeWrapped && !insideThreadLoop)
                {
                    xhir.Add(new Loop
                    {
                        Init = new Block(new Assign(new Ref(_tids["z"]), new Const(0))),
                        Test = Operator.LessThan(new Ref(_tids["z"]), new Fld(typeof(int3).GetField("Z"), new Prop(typeof(IGridApi).GetProperty("BlockDim"), new Ref(_this)))),
                        Body = new Block(new Loop
                        {
                            Init = new Block(new Assign(new Ref(_tids["y"]), new Const(0))),
                            Test = Operator.LessThan(new Ref(_tids["y"]), new Fld(typeof(int3).GetField("Y"), new Prop(typeof(IGridApi).GetProperty("BlockDim"), new Ref(_this)))),
                            Body = new Block(new Loop
                            {
                                Init = new Block(new Assign(new Ref(_tids["x"]), new Const(0))),
                                Test = Operator.LessThan(new Ref(_tids["x"]), new Fld(typeof(int3).GetField("X"), new Prop(typeof(IGridApi).GetProperty("BlockDim"), new Ref(_this)))),
                                Body = new Block(xregion.Children),
                                Iter = new Block(Operator.PreIncrement(new Ref(_tids["x"]))),
                            }),
                            Iter = new Block(Operator.PreIncrement(new Ref(_tids["y"]))),
                        }),
                        Iter = new Block(Operator.PreIncrement(new Ref(_tids["z"]))),
                    });
                }
                else
                {
                    xhir.AddElements(xregion.Children);
                }
            }

            var locals = hir.Locals.Except(_liftedLocals).ToReadOnly();
            locals.ForEach(l => xhir.Locals.Add(l.DeepClone()));
        }

        private void TransformExpression(Expression e, Block xregion, bool insideThreadLoop)
        {
            var xe = TransformExpression(e, insideThreadLoop);
            if (xe != null) xregion.Add(xe);
        }

        private void TransformIf(If @if, Block xregion, bool insideThreadLoop)
        {
            var x_test = TransformExpression(@if.Test, insideThreadLoop);
            var x_iftrue = new Block(); TransformBlock(@if.IfTrue, x_iftrue, insideThreadLoop);
            var x_iffalse = new Block(); TransformBlock(@if.IfFalse, x_iffalse, insideThreadLoop);
            xregion.Add(new If(x_test, x_iftrue, x_iffalse));
        }

        // todo. verify that init and iter can live outside the thread loop (when insideThreadLoop is false)
        private void TransformLoop(Loop loop, Block xregion, bool insideThreadLoop)
        {
            var x_init = new Block(); TransformBlock(loop.Init, x_init, true);
            var x_test = TransformExpression(loop.Test, insideThreadLoop);
            var x_body = new Block(); TransformBlock(loop.Body, x_body, insideThreadLoop);
            var x_iter = new Block(); TransformBlock(loop.Iter, x_iter, true);

            var x_loop = new Loop(x_test, x_body);
            x_loop.Locals.SetElements(loop.Locals);
            x_loop.Init = new Block(x_init.Children);
            x_loop.Iter = new Block(x_iter.Children);
            xregion.Add(x_loop);
        }

        private Expression TransformExpression(Expression e, bool insideThreadLoop)
        {
            var xe = e.Transform(
                (Eval eval) =>
                {
                    var m = eval.InvokedMethod();
                    var decl = m == null ? null : m.DeclaringType;
                    while (decl != null && decl.DeclaringType != null) decl = decl.DeclaringType;
                    return decl == typeof(Hints) ? null : eval.DefaultTransform();
                },
                (Fld fld) =>
                {
                    var isXyz = fld.Field.Name == "X" || fld.Field.Name == "Y" || fld.Field.Name == "Z";
                    if (isXyz)
                    {
                        var parent = fld.This as Prop;
                        var p_isGridApi = parent == null ? false :
                            parent.Property.Hierarchy().Any(p => p.DeclaringType == typeof(IGridApi));
                        if (p_isGridApi)
                        {
                            if (parent.Property.Name == "BlockIdx")
                            {
                                return new Fld(fld.Field, new Ref(_blockIdx));
                            }
                            else if (parent.Property.Name == "ThreadIdx")
                            {
                                return new Ref(_tids[fld.Field.Name.ToLower()]);
                            }
                            else
                            {
                                return fld.DefaultTransform();
                            }
                        }
                        else
                        {
                            return fld.DefaultTransform();
                        }
                    }
                    else
                    {
                        return (Node)fld.DefaultTransform();
                    }
                },
                (Prop prop) =>
                {
                    var isGridApi = prop.Property.Hierarchy().Any(p => p.DeclaringType == typeof(IGridApi));
                    if (isGridApi)
                    {
                        // todo. support raw queries for *Idx properties
                        return prop.DefaultTransform();
                    }
                    else
                    {
                        return prop.DefaultTransform();
                    }
                },
                (Assign ass) =>
                {
                    var @ref = ass.Lhs as Ref;
                    var local = @ref == null ? null : @ref.Sym as Local;
                    if (local != null)
                    {
                        if (Alloc(local) == allocPrivate &&
                            _needsReplication[local])
                        {
                            insideThreadLoop.AssertTrue();
                            var r_local = _replicatedLocals[local];
                            var replica = new Eval(new Apply(
                                new Lambda(r_local.Type.ArraySetter()),
                                new Ref(r_local), 
                                new Ref(_tids["z"]), 
                                new Ref(_tids["y"]), 
                                new Ref(_tids["x"]),
                                ass.Rhs.CurrentTransform()));
                            return (Node)replica;
                        }
                        else
                        {
                            return ass.DefaultTransform();
                        }
                    }
                    else
                    {
                        return ass.DefaultTransform();
                    }
                },
                (Ref @ref) =>
                {
                    var sym = @ref.Sym;
                    if (sym.IsParam())
                    {
                        var param = sym.AssertCast<Param>();
                        if (param == _params.First()) return new Ref(_this);
                        else
                        {
                            return @ref.DefaultTransform();
                        }
                    }
                    else
                    {
                        var local = sym.AssertCast<Local>();
                        if (local != null)
                        {
                            if (Alloc(local) == allocPrivate &&
                                _needsReplication[local])
                            {
                                insideThreadLoop.AssertTrue();
                                var r_local = _replicatedLocals[local];
                                var replica = new Eval(new Apply(
                                    new Lambda(r_local.Type.ArrayGetter()),
                                    new Ref(r_local),
                                    new Ref(_tids["z"]),
                                    new Ref(_tids["y"]),
                                    new Ref(_tids["x"])));
                                return (Node)replica;
                            }
                            else
                            {
                                return @ref.DefaultTransform();
                            }
                        }
                        else
                        {
                            return @ref.DefaultTransform();
                        }
                    }
                }).AssertCast<Expression>();
            return xe;
        }
    }
}
