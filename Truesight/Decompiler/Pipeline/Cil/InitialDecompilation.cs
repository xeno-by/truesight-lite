using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.SymbolStore;
using System.Reflection;
using Truesight.Decompiler.Hir;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Core.Functional;
using Truesight.Decompiler.Pipeline.Flow.Cfg;
using System.Linq;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Reflection;
using XenoGears.Reflection.Generics;
using Truesight.Parser.Api;
using Truesight.Parser.Api.Ops;
using XenoGears.Reflection.Shortcuts;
using XenoGears.Traits.Equivatable;
using CilOperator = Truesight.Parser.Api.Ops.Operator;
using CilCast = Truesight.Parser.Api.Ops.Cast;
using CilBreak = Truesight.Parser.Api.Ops.Break;
using CilSizeOf = Truesight.Parser.Api.Ops.Sizeof;
using CilThrow = Truesight.Parser.Api.Ops.Throw;
using CilRethrow = Truesight.Parser.Api.Ops.Rethrow;
using CilOperatorType = Truesight.Parser.Api.Ops.OperatorType;
using HirOperator = Truesight.Decompiler.Hir.Core.Expressions.Operator;
using HirBreak = Truesight.Decompiler.Hir.Core.ControlFlow.Break;
using HirSizeOf = Truesight.Decompiler.Hir.Core.Expressions.SizeOf;
using HirThrow = Truesight.Decompiler.Hir.Core.ControlFlow.Throw;
using HirOperatorType = Truesight.Decompiler.Hir.Core.Expressions.OperatorType;
using Convert=Truesight.Decompiler.Hir.Core.Expressions.Convert;
using Truesight.Decompiler.Hir.Traversal;
using Truesight.Decompiler.Hir.TypeInference;
using Truesight.Parser.Impl.PdbReader;

namespace Truesight.Decompiler.Pipeline.Cil
{
    // initial decompilation of CIL
    // during this step we might get so-called imbalanced blocks
    // i.e. ones that require non-empty stack on begin or leave non-empty stack after end
    // note. here we only deal with continuous control flow
    // branches and such are processed elsewhere (e.g. in CreateCarcass)
    internal class InitialDecompilation
    {
        private readonly ControlFlowBlock _block;
        private readonly ReadOnlyCollection<IILOp> _cil;
        private readonly Symbols _symbols;
        public static void DoPrimaryDecompilation(ControlFlowBlock block, ReadOnlyCollection<IILOp> cil, Symbols symbols) { new InitialDecompilation(block, cil, symbols).DoPrimaryDecompilation(); }
        private InitialDecompilation(ControlFlowBlock block, ReadOnlyCollection<IILOp> cil, Symbols symbols)
        {
            _block = block;
            _cil = cil;
            _symbols = symbols;
        }

        private void DoPrimaryDecompilation()
        {
            var loopholes = new List<Loophole>();
            Func<Loophole> emit_loophole = () =>
            {
                var loophole = new Loophole();
                loopholes.Add(loophole);
                return loophole;
            };

            _stack = new Stack<Expression>();
            foreach (var op in _cil)
            {
                _currentOp = op;
                _args = op.OpSpec.Pops.Times(_ =>
                    _stack.PopOrDefault() ?? emit_loophole()
                ).Reverse().ToReadOnly();

                Dispatch(op);

                if (_stack.IsEmpty() && _qualified.IsNotEmpty())
                {
                    var fixt = FixupEvaluationOrder(_qualified.ToReadOnly());
                    fixt.ForEach(q => _block.BalancedCode.Add(q));
                    _qualified.Clear();
                }
            }

            var pending = _qualified.Concat(_stack.Reverse().Cast<Node>()).ToReadOnly();
            if (pending.IsNotEmpty())
            {
                var p_fixt = FixupEvaluationOrder(pending);
                Func<Node, int> stackBalance = n => 
                    !(n is Expression) ? 0 :
                    n is Assign ? 0 :
                    n is Eval ? (n.InvokedMethod().Ret() == typeof(void) ? 0 : 1) :
                    /* otherwise */ 1;;
                var delim = 0.UpTo(p_fixt.Count() - 1).Single(i =>
                    p_fixt.Take(i).All(n => stackBalance(n) == 0) &&
                    p_fixt.Skip(i).All(n => stackBalance(n) == 1));
                p_fixt.Take(delim).ForEach(p => _block.BalancedCode.Add(p));
                p_fixt.Skip(delim).ForEach(p => _block.Residue.Add(p.AssertCast<Expression>()));
                _qualified.Clear();
            }

            loopholes.Reverse();
            loopholes.ForEach((l, i) => l.Tag = i);
        }

        // note. so what do we do here?
        //
        // shortly put, we ensure semantic correctness 
        // of transforming stack assembler to register assembler
        // note. okay, most likely you want more details... here they are:
        //
        // our naive approach to decompilation (see logic of this class)
        // i.e. sequential execution of instructions, keeping stack in sync
        // and then qualifying stack top when assignments take place
        // is conceptually flawed
        //
        // note. the point is that in presence of "dups" (upd. or "pops")
        // our algorithm sometimes produces ASTs that violate execution order of IL instructions
        //
        // note. e.g. take a look at the following IL snippet:
        // L_0001: ldc.i4.0 
        // L_0002: ldarg.1 
        // L_0003: dup 
        // L_0004: ldfld Int32 Playground.Truesight.Decompiler.Snippets/B::f
        // L_0009: dup 
        // L_000a: stloc.1 
        // L_000b: ldc.i4.1 
        // L_000c: add 
        // L_000d: stfld Int32 Playground.Truesight.Decompiler.Snippets/B::f
        // L_0012: ldloc.1 
        // L_0013: cgt 
        // which is the result of compilation of the following C# code:
        // * if (0 > arg1.f++) ...
        //
        // the naive approach will work as follows:
        // (btw, here I intentionally simplify handling of non-sequential control flow)
        // ldc.i4.0     => stack: Const(0)
        // ldarg.1      => stack: Const(0), Ref(arg1)
        // dup          => stack: Const(0), Ref($0), Ref($0)                => qualified: Assign(Ref($0), Ref(arg1))
        // ldfld f      => stack: Const(0), Ref($0), Fld(Ref($0), f)
        // dup          => stack: Const(0), Ref($0), Ref($1), Ref($1)       => qualified: Assign(Ref($1), Fld(Ref($0), f))
        // stloc.1      => stack: Const(0), Ref($0), Ref($1)                => qualified: Assign(Ref(loc1), Ref($1))
        // ldc.i4.1     => stack: Const(0), Ref($0), Ref($1), Const(1)
        // add          => stack: Const(0), Ref($0), Add(Ref($1), Const(1))
        // stfld f      => stack: Const(0)                                  => qualified: Assign(Fld(Ref($0), f), Add(Ref($1), Const(1)))
        // ldloc.1      => stack: Const(0), Ref(loc1)
        // cgt          => stack: Gt(Const(0), Ref(loc1))
        //
        // so, as the result we'll have:
        // * Assign(Ref($0), Ref(arg1))
        // * Assign(Ref($1), Fld(Ref($0), f))
        // * Assign(Ref(loc1), Ref($1))
        // * Assign(Fld(Ref($0), f), Add(Ref($1), Const(1)))
        // * Gt(Const(0), Ref(loc1))
        //
        // however, if we compile these 5 expressions back into IL, we'll have:
        // * ldarg 1
        // * stloc $0
        // * ldloc $0
        // * ldfld f
        // * stloc $1
        // * ldloc $1
        // * stloc 1
        // * ldloc $0
        // * ldloc $1
        // * ldc.i4 1
        // * stfld f
        // * ldc.i4 0
        // * ldloc 1
        // * cgt
        //
        // note. did you see it?!
        // "ldc.i4 0" instruction that was at L_0001 now became second last
        // so we've got evaluation order violation
        // it's okay when it concerns ops that don't spawn side-effects outside stack
        // e.g. aforementioned ldc, stuff like adds, converts and so on
        // however, if you go and check samples from Snippets
        // you'll face loads of cases when such reorderings are fatal for semantics
        // e.g. when "ldarg X" is reordered to be executed after "starg X"
        //
        // note. "okay" - you might think - "so let's just check for violations and fix them"
        // unfortunately, it seems that there doesn't exist an algorithm
        // that could robustly find out whether one IL instruction depend on another
        // e.g. how do you know that you can reorder "ldloc 0" past "call foo"
        // maybe loc0 is an object that gets mutated by a side-effect produced by foo?
        // 
        // note. but if you prohibit such reorderings that are innocent in 99.99% cases
        // then loads of code produced by C# compiler,
        // i.e. a significant percent of [op=]'s, ++'s and such
        // will fail to be compliant to this rule
        //
        // todo. so here we just tie ourselved to a practically useful particular case
        // i.e. to the code that is produced by MSVC# 2008 compiler, namely:
        // the "Microsoft (R) Visual C# 2008 Compiler version 3.5.30729.1" one
        //
        // note. here's a brief description of the algorithm that's currently used:
        // 1) we only allow ldlocs, ldargs, ldflds and propgets to be reordered
        // 2) if any of those is reordered we look through dependency violations
        // 3) and seek for complementing instructions (e.g. starg 0 for ldarg 0)
        // 4) if none of those are found, everything is considered to be okay
        // 5) otherwise, we detect the place where the reordered instruction was originally located
        // 6) and insert there a corresponding read into a temporary local
        // 7) after that we replace reordered instruction with a read from that temporary local
        private IILOp _currentOp;
        private Queue<Node> _qualified = new Queue<Node>();
        private Dictionary<Node, IILOp> _map = new Dictionary<Node, IILOp>();
        private Node _mapOp(IILOp op) { return _map.AssertSingle(kvp => ReferenceEquals(kvp.Value, op)).Key; }
        private ReadOnlyCollection<Node> FixupEvaluationOrder(ReadOnlyCollection<Node> seq)
        {
            var q_evalOrders = new Dictionary<Node, ReadOnlyCollection<IILOp>>();
            seq.ForEach(q => q_evalOrders[q] = q.CSharpEvaluationOrder().Where(n => n != null)
                .Select(n => _map.GetOrDefault(n)).Where(op => op != null).ToReadOnly());
            var evalOrder = q_evalOrders.SelectMany(kvp => kvp.Value).ToReadOnly();

            var violatedDeps = new Dictionary<IILOp, ReadOnlyCollection<IILOp>>();
            evalOrder.ForEach((op, i_op) => violatedDeps.Add(op, evalOrder.Where(
                (oop, i_oop) => i_op > i_oop && op.Offset < oop.Offset).ToReadOnly()));
            violatedDeps.RemoveElements(kvp => kvp.Value.IsEmpty());

            if (violatedDeps.IsEmpty())
            {
                return seq;
            }
            else
            {
                var fixt = seq.ToList();

                foreach (var op in violatedDeps.Keys)
                {
                    if (op is Call)
                    {
                        var call = op.AssertCast<Call>();
                        if (call.Method.IsGetter()) /* implemented */ {}
                        else if (call.Method.IsSetter()) throw AssertionHelper.Fail();
                        else throw AssertionHelper.Fail();
                    }
                    else if (op is New) /* presume that ctors are stateless */ {}
                    else if (op is Ldloc) /* implemented */ {}
                    else if (op is Ldarg) /* implemented */ {}
                    else if (op is Ldelem) throw AssertionHelper.Fail();
                    else if (op is Ldfld) /* implemented */ {}
                    else if (op is Ldloca) /* implemented */ {}
                    else if (op is Ldarga) /* implemented */ {}
                    else if (op is Ldelema) throw AssertionHelper.Fail();
                    else if (op is Ldflda) /* implemented */ {}
                    else if (op is Ldind) throw AssertionHelper.Fail();
                    else if (op is Ldftn) throw AssertionHelper.Fail();
                    else if (op is Stloc) throw AssertionHelper.Fail();
                    else if (op is Starg) throw AssertionHelper.Fail();
                    else if (op is Stelem) throw AssertionHelper.Fail();
                    else if (op is Stfld) throw AssertionHelper.Fail();
                    else if (op is Stind) throw AssertionHelper.Fail();
                    // ops that neither read nor write from anywhere except stack
                    // can be freely skipped so we have to consider only a dozen of ops
                    else continue;

                    if (op is Ldloc || op is Ldloca)
                    {
                        var ldloc = op as Ldloc;
                        var ldloca = op as Ldloca;
                        var loc_il = ldloc != null ? ldloc.Loc :
                            ldloca != null ? ldloca.Loc : ((Func<ILocalVar>)(() => { throw AssertionHelper.Fail(); }))();

                        var violations = violatedDeps[op];
                        if (violations.OfType<Stloc>().Any(stloc => stloc.Index == loc_il.Index))
                        {
                            var loc_sym = _symbols.ResolveLocal(loc_il.Index);
                            var expr_ldloc = _mapOp(op).AssertCast<Ref>();
                            (expr_ldloc.Sym == loc_sym).AssertTrue();

                            var locName = loc_il.Source.DebugInfo == null ? ("loc" + loc_il.Index) :
                                !loc_il.Source.DebugInfo.LocalNames.ContainsKey(loc_il.Index) ? ("loc" + loc_il.Index) :
                                loc_il.Source.DebugInfo.LocalNames[loc_il.Index];
                            var bufLocName = Seq.Nats.Select(i => locName + "__CF$" + i.ToString("0000")).First(name1 => _symbols.Locals.None(loc1 => loc1.Name == name1));
                            var bufLoc = _symbols.IntroduceLocal(bufLocName, loc_il.Type);

                            var startOfViolations = q_evalOrders.Keys.First(q => Set.Intersect(q_evalOrders[q], violations).IsNotEmpty());
                            q_evalOrders[startOfViolations].Except(violations).AssertEmpty();
                            var insertionPoint = fixt.IndexOf(startOfViolations);
                            fixt.Insert(insertionPoint, new Assign(new Ref(bufLoc), new Ref(loc_sym)));
                            expr_ldloc.Parent.Children.ReplaceElements(expr_ldloc, new Ref(bufLoc));
                        }
                    }

                    if (op is Ldarg || op is Ldarga)
                    {
                        var ldarg = op as Ldarg;
                        var ldarga = op as Ldarga;
                        var arg_il = ldarg != null ? ldarg.Arg :
                            ldarga != null ? ldarga.Arg : ((Func<ParameterInfo>)(() => { throw AssertionHelper.Fail(); }))();
                        var arg_index = ldarg != null ? ldarg.Index :
                            ldarga != null ? ldarga.Index : ((Func<int>)(() => { throw AssertionHelper.Fail(); }))();

                        var violations = violatedDeps[op];
                        if (violations.OfType<Starg>().Any(starg => starg.Index == arg_index))
                        {
                            var arg_sym = _symbols.ResolveParam(arg_index);
                            var expr_ldarg = _mapOp(op).AssertCast<Ref>();
                            (expr_ldarg.Sym == arg_sym).AssertTrue();

                            var argName = arg_sym.Name;
                            var bufLocName = Seq.Nats.Select(i => argName + "__CF$" + i.ToString("0000")).First(name1 => _symbols.Locals.None(loc => loc.Name == name1));
                            var bufLoc = _symbols.IntroduceLocal(bufLocName, null);

                            var startOfViolations = q_evalOrders.Keys.First(q => Set.Intersect(q_evalOrders[q], violations).IsNotEmpty());
                            q_evalOrders[startOfViolations].Except(violations).AssertEmpty();
                            var insertionPoint = fixt.IndexOf(startOfViolations);
                            fixt.Insert(insertionPoint, new Assign(new Ref(bufLoc), new Ref(arg_sym)));
                            expr_ldarg.Children.ReplaceElements(expr_ldarg, new Ref(bufLoc));
                        }
                    }

                    if (op is Ldfld || op is Ldflda)
                    {
                        var ldfld = op as Ldfld;
                        var ldflda = op as Ldflda;
                        var fld = ldfld != null ? ldfld.Field :
                            ldflda != null ? ldflda.Field : ((Func<FieldInfo>)(() => { throw AssertionHelper.Fail(); }))();

                        var violations = violatedDeps[op];
                        if (violations.OfType<Stfld>().Any(stfld => stfld.Field == fld &&
                            ((Func<bool>)(() =>
                            {
                                var stfld_fld = _mapOp(stfld).AssertCast<Assign>().Lhs.AssertCast<Fld>();
                                var ldfld_fld = _mapOp(op).AssertCast<Fld>();
                                return stfld_fld.This.Equiv(ldfld_fld.This);
                            }))()))
                        {
                            var expr_ldfld = _mapOp(op).AssertCast<Fld>();
                            (expr_ldfld.Field == fld).AssertTrue();

                            var fldName = fld.Name;
                            var bufLocName = Seq.Nats.Select(i => fldName + "__CF$" + i.ToString("0000")).First(name1 => _symbols.Locals.None(loc => loc.Name == name1));
                            var bufLoc = _symbols.IntroduceLocal(bufLocName, null);

                            var startOfViolations = q_evalOrders.Keys.First(q => Set.Intersect(q_evalOrders[q], violations).IsNotEmpty());
                            q_evalOrders[startOfViolations].Except(violations).AssertEmpty();
                            var insertionPoint = fixt.IndexOf(startOfViolations);
                            fixt.Insert(insertionPoint, new Assign(new Ref(bufLoc), new Fld(fld, expr_ldfld.This)));
                            expr_ldfld.Children.ReplaceElements(expr_ldfld, new Ref(bufLoc));
                        }
                    }

                    if (op is Call)
                    {
                        var callGet = op.AssertCast<Call>();
                        callGet.Method.IsGetter().AssertTrue();
                        var violations = violatedDeps[callGet];
                        if (violations.OfType<Call>().Any(callSet =>
                        {
                            if (!callSet.Method.IsSetter()) return false;
                            if (!(callSet.Method.EnclosingProperty() == callGet.Method.EnclosingProperty())) return false;

                            // todo. verify that both calls reference the same property
                            // and have the same This (use Dump comparison and add a todo)
                            throw new NotImplementedException();
                        }))
                        {
                            var prop = callGet.Method.EnclosingProperty().AssertNotNull();
                            var expr_callGet = _mapOp(callGet);
                            // todo. verify that it references the "prop" property

                            var rawName = callGet.Method.Name;
                            var bufLocName = Seq.Nats.Select(i => rawName + "__CF$" + i.ToString("0000")).First(name1 => _symbols.Locals.None(loc => loc.Name == name1));
                            var bufLoc = _symbols.IntroduceLocal(bufLocName, null);

                            var startOfViolations = q_evalOrders.Keys.First(q => Set.Intersect(q_evalOrders[q], violations).IsNotEmpty());
                            q_evalOrders[startOfViolations].Except(violations).AssertEmpty();
                            var insertionPoint = fixt.IndexOf(startOfViolations);
                            fixt.Insert(insertionPoint, ((Func<Expression>)(() =>
                            {
                                throw new NotImplementedException();
                            }))());
                            expr_callGet.Children.ReplaceElements(expr_callGet, new Ref(bufLoc));
                        }
                    }
                }

                return fixt.ToReadOnly();
            }
        }

        // control panel of primary decompilation
        private Stack<Expression> _stack;
        private void Ignore() { /* do nothing */ }
        private void Push(Expression expr)
        {
            var pp_expr = Filter(expr);
            _map.Add(pp_expr, _currentOp);
            var dbg = _currentOp.Source.DebugInfo;
            if (dbg != null) pp_expr.Family().Where(n => n != null).ForEach(n => n.Src = n.Src ?? dbg[_currentOp.Offset]);
            _stack.Push(pp_expr);
        }
        private void Qualify(Node node)
        {
            var pp_node = Filter(node);
            _map.Add(pp_node, _currentOp);
            var dbg = _currentOp.Source.DebugInfo;
            if (dbg != null) pp_node.Family().Where(n => n != null).ForEach(n => n.Src = n.Src ?? dbg[_currentOp.Offset]);
            _qualified.Enqueue(pp_node);
        }

        // node postprocessors
        private Expression Filter(Expression expr) { return Filter((Node)expr).AssertCast<Expression>(); }
        private Node Filter(Node node) { return node; }

        // context of current instruction processing
        private ReadOnlyCollection<Expression> _args;
        private Expression _firstArg { get { return _args.First(); } }
        private Expression _secondArg { get { return _args.Second(); } }
        private Expression _thirdArg { get { return _args.Third(); } }

        // instruction processors
        private void Process(Nop nop) { Ignore(); }
        private void Process(CilBreak @break) { Ignore(); }
        private void Process(Ldc ldc) { Push(new Const(ldc.Value, ldc.Type)); }
        private void Process(Ldarg ldarg) { Push(new Ref(_symbols.ResolveParam(ldarg.Index))); }
        private void Process(Ldarga ldarga) { Push(new Addr(new Ref(_symbols.ResolveParam(ldarga.Index)))); }
        private void Process(Ldloc ldloc) { Push(new Ref(_symbols.ResolveLocal(ldloc.Index))); }
        private void Process(Ldloca ldloca) { Push(new Addr(new Ref(_symbols.ResolveLocal(ldloca.Index)))); }
        private void Process(Ldfld ldfld) { Push(new Fld(ldfld.Field, ldfld.Field.IsStatic ? null : _firstArg)); }
        private void Process(Ldflda ldflda) { Push(new Addr(new Fld(ldflda.Field, ldflda.Field.IsStatic ? null : _firstArg))); }
        private void Process(Ldelem ldelem) { Push(new Eval(new Apply(new Lambda(typeof(Array).GetMethods(BF.All | BF.DeclOnly).Single(m => m.Name == "GetValue" && m.Params().SequenceEqual(typeof(int)))), _firstArg, _secondArg))); }
        private void Process(Ldelema ldelema) { Push(new Addr(new Eval(new Apply(new Lambda(typeof(Array).GetMethods(BF.All | BF.DeclOnly).AssertSingle(m => m.Name == "GetValue" && m.Params().SequenceEqual(typeof(int)))), _firstArg, _secondArg)))); }
        private void Process(Ldlen ldlen) { Push(new Eval(new Apply(new Lambda(typeof(Array).GetMethod("GetLength").AssertNotNull()), _firstArg, _secondArg))); }
        private void Process(Ldind ldind) { Push(new Deref(_firstArg)); }
        private void Process(Starg starg) { Qualify(new Assign(new Ref(_symbols.ResolveParam(starg.Index)), _firstArg)); }
        private void Process(Stloc stloc) { Qualify(new Assign(new Ref(_symbols.ResolveLocal(stloc.Index)), _firstArg)); }
        private void Process(Stfld stfld) { Qualify(new Assign(new Fld(stfld.Field, stfld.Field.IsStatic ? null : _firstArg), stfld.Field.IsStatic ? _firstArg : _secondArg)); }
        private void Process(Stelem stelem) { Qualify(new Eval(new Apply(new Lambda(typeof(Array).GetMethods(BF.All | BF.DeclOnly).AssertSingle(m => m.Name == "SetValue" && m.Params().SequenceEqual(new []{typeof(Object), typeof(int)}))), _firstArg, _secondArg, _thirdArg))); }
        private void Process(Stind stind) { Qualify(new Assign(new Deref(_firstArg), _secondArg)); }
        private void Process(Call call)
        {
            var m = call.Method;

            // the line below is necessary because
            // sometimes csc emits callvirt when calling non-virtual methods
            var virt = call.IsVirtual && m.IsVirtual();
            var style = virt ? InvocationStyle.Virtual : InvocationStyle.NonVirtual;

            var p = m.EnclosingProperty();
            if (p == null)
            {
                var getTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", new []{typeof(RuntimeTypeHandle)});
                if (m == getTypeFromHandle && _args.First() is Const)
                {
                    var typeHandle = _args.First().AssertCast<Const>().Value.AssertCast<RuntimeTypeHandle>();
                    Push(new Const(m.Invoke(null, new Object[]{typeHandle})));
                }

                var getMethodFromHandle1 = typeof(MethodBase).GetMethod("GetMethodFromHandle", new []{typeof(RuntimeMethodHandle)});
                if (m == getMethodFromHandle1 && _args.First() is Const)
                {
                    var methodHandle = _args.First().AssertCast<Const>().Value.AssertCast<RuntimeMethodHandle>();
                    Push(new Const(m.Invoke(null, new Object[]{methodHandle})));
                }

                var getMethodFromHandle2 = typeof(MethodBase).GetMethod("GetMethodFromHandle", new []{typeof(RuntimeMethodHandle), typeof(RuntimeTypeHandle)});
                if (m == getMethodFromHandle2 && _args.First() is Const && _args.Second() is Const)
                {
                    var methodHandle = _args.First().AssertCast<Const>().Value.AssertCast<RuntimeMethodHandle>();
                    var typeHandle = _args.Second().AssertCast<Const>().Value.AssertCast<RuntimeTypeHandle>();
                    Push(new Const(m.Invoke(null, new Object[]{methodHandle, typeHandle})));
                }

                var getFieldFromHandle1 = typeof(FieldInfo).GetMethod("GetFieldFromHandle", new []{typeof(RuntimeFieldHandle)});
                if (m == getFieldFromHandle1 && _args.First() is Const)
                {
                    var fieldHandle = _args.First().AssertCast<Const>().Value.AssertCast<RuntimeFieldHandle>();
                    Push(new Const(m.Invoke(null, new Object[]{fieldHandle})));
                }

                var getFieldFromHandle2 = typeof(FieldInfo).GetMethod("GetFieldFromHandle", new []{typeof(RuntimeFieldHandle), typeof(RuntimeTypeHandle)});
                if (m == getFieldFromHandle2 && _args.First() is Const && _args.Second() is Const)
                {
                    var fieldHandle = _args.First().AssertCast<Const>().Value.AssertCast<RuntimeFieldHandle>();
                    var typeHandle = _args.Second().AssertCast<Const>().Value.AssertCast<RuntimeTypeHandle>();
                    Push(new Const(m.Invoke(null, new Object[]{fieldHandle, typeHandle})));
                }

                if (m.DeclaringType.IsArray && m.Name == "Address")
                {
                    var getter = m.DeclaringType.ArrayGetter();
                    Push(new Addr(new Eval(new Apply(new Lambda(getter, style), _args))));
                }
                else if (m.DeclaringType.IsDelegate() && m.Name == "Invoke")
                {
                    var app = new Apply(_firstArg, _args.Skip(1));
                    if (m.Ret() != typeof(void))
                    {
                        Push(new Eval(app));
                    }
                    else
                    {
                        Qualify(new Eval(app));
                    }
                }
                else
                {
                    if (m.IsConstructor)
                    {
                        // note. ctor can be called via "call", but not via "newobj" in two cases:
                        // 1) base/this constructor invocation within a constructor
                        if (m.DeclaringType.IsClass)
                        {
                            Qualify(new Eval(new Apply(new Lambda(m, style), _args)));
                        }
                        // 2) constructor invocation for structs
                        else
                        {
                            var @this = _firstArg.AssertCast<Addr>().Target;
                            var ctor_call = new Eval(new Apply(new Lambda(m, InvocationStyle.Ctor), _args.Skip(1)));
                            Qualify(new Assign(@this, ctor_call));
                        }
                    }
                    else
                    {
                        var app = new Apply(new Lambda(m, style), _args);
                        if (m.Ret() != typeof(void))
                        {
                            Push(new Eval(app));
                        }
                        else
                        {
                            Qualify(new Eval(app));
                        }
                    }
                }
            }
            else
            {
                var prop = new Prop(p, p.IsStatic() ? null : _args.First(), virt);
                if (m == p.GetGetMethod(true))
                {
                    var index = p.IsStatic() ? _args : _args.Skip(1);
                    if (p.GetIndexParameters().IsEmpty())
                    {
                        index.AssertEmpty();
                        Push(prop);
                    }
                    else
                    {
                        Push(new Apply(prop, index));
                    }
                }
                else
                {
                    (m == p.GetSetMethod(true)).AssertTrue();

                    var value = _args.Last();
                    var index = p.IsStatic() ? _args.SkipLast(1) : _args.Skip(1).SkipLast(1);
                    if (p.GetIndexParameters().IsEmpty())
                    {
                        index.AssertEmpty();
                        Qualify(new Assign(prop, value));
                    }
                    else
                    {
                        Qualify(new Assign(new Apply(prop, index), value));
                    }
                }
            }
        }
        private void Process(Ldftn ldftn)
        {
            (ldftn.Next is New).AssertTrue();
            var style = ldftn.IsVirtual ? InvocationStyle.Virtual : InvocationStyle.NonVirtual;
            var lambda = new Lambda(ldftn.Method, style);
            Push(ldftn.IsVirtual ? new Apply(lambda, _firstArg) : (Expression)lambda);
        }
        private void Process(New @new)
        {
            if (@new.Prev is Ldftn)
            {
                var @this = _firstArg;
                var isFtnStatic = @this is Const && ((Const)@this).Value == null;

                _map.Remove(_secondArg);
                if (_secondArg is Lambda)
                {
                    var ftn = _secondArg.AssertCast<Lambda>().AssertThat(lam => !lam.InvokedAsVirtual);
                    ftn.Method.IsStatic.AssertEquiv(isFtnStatic);
                    Push(isFtnStatic ? (Expression)ftn : new Apply(ftn, @this));
                }
                else 
                {
                    var app = _secondArg.AssertCast<Apply>();
                    var app_this = app.Args.AssertSingle();
                    app_this.Equiv(@this).AssertTrue();
                    Push(app);
                }
            }
            else
            {
                Push(new Eval(new Apply(new Lambda(@new.Ctor, InvocationStyle.Ctor), _args)));
            }
        }
        private void Process(Initobj initobj)
        {
            var addr = _firstArg.AssertCast<Addr>();
            Qualify(new Assign(addr.Target, new Default(initobj.Type)));
        }
        private void Process(CilOperator op) { Push(HirOperator.Create(op.OperatorType, _args)); }
        private void Process(CilCast cast) { Push(new Convert(cast.Type, _firstArg)); }
        private void Process(Sizeof @sizeof) { Push(new SizeOf(@sizeof.Type)); }
        private void Process(Isinst isinst) { Push(new TypeAs(isinst.Type, _firstArg)); }
        private void Process(CilThrow @throw) { Qualify(new HirThrow(_firstArg)); }
        private void Process(CilRethrow rethrow) { Qualify(new HirThrow()); }
        // note. dup is used by csc to implement: 1) [op=] operators, 2) inc/dec operators, 3) ternaries
        // previously I simply cloned the _firstArg expression, but that's a failboat approach
        // since dupped expression might contain side-effects that mustn't be repeated twice
        private void Process(Dup dup)
        {
            var name = Seq.Nats.Select(i => "CF$" + i.ToString("0000")).First(name1 => _symbols.Locals.None(loc => loc.Name == name1));
            var temp = _symbols.IntroduceLocal(name, _firstArg.Type());
            Qualify(new Assign(new Ref(temp), _firstArg));
            var oldKeys = _map.Keys.ToReadOnly();
            Push(new Ref(temp)); 
            Push(new Ref(temp));
            _map.RemoveElements(_map.Keys.Except(oldKeys));
        }
        private void Process(Pop pop)
        {
            // note. omg another glitch with evaluation order
            var oldop = _map[_firstArg];
            _map.Remove(_firstArg);
            Qualify(_firstArg);
            _map[_firstArg] = oldop;
        }

        // dispatch logic
        private void Dispatch(IILOp op)
        {
            switch (op.OpType)
            {
                case IILOpType.Nop:
                    Process(op.AssertCast<Nop>());
                    break;
                case IILOpType.Break:
                    Process(op.AssertCast<CilBreak>());
                    break;
                case IILOpType.Ldarg:
                    Process(op.AssertCast<Ldarg>());
                    break;
                case IILOpType.Ldloc:
                    Process(op.AssertCast<Ldloc>());
                    break;
                case IILOpType.Stloc:
                    Process(op.AssertCast<Stloc>());
                    break;
                case IILOpType.Ldarga:
                    Process(op.AssertCast<Ldarga>());
                    break;
                case IILOpType.Starg:
                    Process(op.AssertCast<Starg>());
                    break;
                case IILOpType.Ldloca:
                    Process(op.AssertCast<Ldloca>());
                    break;
                case IILOpType.Ldc:
                    Process(op.AssertCast<Ldc>());
                    break;
                case IILOpType.Dup:
                    Process(op.AssertCast<Dup>());
                    break;
                case IILOpType.Pop:
                    Process(op.AssertCast<Pop>());
                    break;
                case IILOpType.Call:
                    Process(op.AssertCast<Call>());
                    break;
                case IILOpType.Ldind:
                    Process(op.AssertCast<Ldind>());
                    break;
                case IILOpType.Stind:
                    Process(op.AssertCast<Stind>());
                    break;
                case IILOpType.Operator:
                    Process(op.AssertCast<CilOperator>());
                    break;
                case IILOpType.Cast:
                    Process(op.AssertCast<Cast>());
                    break;
                case IILOpType.New:
                    Process(op.AssertCast<New>());
                    break;
                case IILOpType.Initobj:
                    Process(op.AssertCast<Initobj>());
                    break;
                case IILOpType.Ldfld:
                    Process(op.AssertCast<Ldfld>());
                    break;
                case IILOpType.Ldflda:
                    Process(op.AssertCast<Ldflda>());
                    break;
                case IILOpType.Stfld:
                    Process(op.AssertCast<Stfld>());
                    break;
                case IILOpType.Ldlen:
                    Process(op.AssertCast<Ldlen>());
                    break;
                case IILOpType.Ldelema:
                    Process(op.AssertCast<Ldelema>());
                    break;
                case IILOpType.Ldelem:
                    Process(op.AssertCast<Ldelem>());
                    break;
                case IILOpType.Stelem:
                    Process(op.AssertCast<Stelem>());
                    break;
                case IILOpType.Ldftn:
                    Process(op.AssertCast<Ldftn>());
                    break;
                case IILOpType.Isinst:
                    Process(op.AssertCast<Isinst>());
                    break;
                case IILOpType.Sizeof:
                    Process(op.AssertCast<CilSizeOf>());
                    break;
                case IILOpType.Throw:
                    Process(op.AssertCast<CilThrow>());
                    break;
                case IILOpType.Rethrow:
                    Process(op.AssertCast<CilRethrow>());
                    break;
                case IILOpType.Localloc:
                    throw AssertionHelper.Fail();
                case IILOpType.Arglist:
                    throw AssertionHelper.Fail();
                case IILOpType.Mkrefany:
                    throw AssertionHelper.Fail();
                case IILOpType.Refanyval:
                    throw AssertionHelper.Fail();
                case IILOpType.Refanytype:
                    throw AssertionHelper.Fail();
                case IILOpType.Initblk:
                    throw AssertionHelper.Fail();
                case IILOpType.Cpblk:
                    throw AssertionHelper.Fail();
                case IILOpType.Cpobj:
                    throw AssertionHelper.Fail();
                case IILOpType.Jmp:
                    throw AssertionHelper.Fail();
                case IILOpType.Ckfinite:
                    throw AssertionHelper.Fail();
                default:
                    throw AssertionHelper.Fail();
            }
        }
    }
}