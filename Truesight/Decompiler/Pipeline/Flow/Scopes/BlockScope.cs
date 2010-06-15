using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Truesight.Decompiler.Hir;
using Truesight.Decompiler.Hir.Core.ControlFlow;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Pipeline.Flow.Cfg;
using XenoGears.Functional;
using XenoGears.Assertions;

namespace Truesight.Decompiler.Pipeline.Flow.Scopes
{
    internal class BlockScope : IScope<Block>
    {
        private readonly IScope _parent;
        public IScope Parent { get { return _parent; } }

        private readonly Block _block = new Block();
        Node IScope.Hir { get { return Hir; } }
        public Block Hir { get { return _block; } }

        private readonly BaseControlFlowGraph _localCfg;
        public BaseControlFlowGraph LocalCfg { get { return _localCfg; } }
        public ReadOnlyCollection<Offspring> Offsprings { get { return Seq.Empty<Offspring>().ToReadOnly(); } }
        public ReadOnlyCollection<ControlFlowBlock> Pivots { get { return Seq.Empty<ControlFlowBlock>().ToReadOnly(); } }

        public static BlockScope Decompile(IScope parent, BaseControlFlowGraph cfg) { return new BlockScope(parent, cfg); }
        private BlockScope(IScope parent, BaseControlFlowGraph cfg)
        {
            _parent = parent.AssertNotNull();
            _localCfg = cfg;

            var offspringsRecursive = new Dictionary<ControlFlowBlock, Offspring>();
            this.Hierarchy().ForEach(s => s.Offsprings.ForEach(off => offspringsRecursive.Add(off.Root, off)));
            var pivotsRecursive = new Dictionary<ControlFlowBlock, IScope>();
            this.Parents().Reverse().ForEach(p => p.Pivots.ForEach(cfb => pivotsRecursive[cfb] = p));

            var cflow = _localCfg.Cflow().Except(cfg.Start).ToReadOnly();
            var todo = cflow.ToList();
            var expected = todo.First();
            while (todo.IsNotEmpty())
            {
                var curr = todo.First();
                (expected == curr).AssertTrue();
                todo.Remove(curr);

                var offspring = offspringsRecursive.GetOrDefault(curr);
                if (offspring != null)
                {
                    var parentCfg = offspring.Scope.Parent.LocalCfg;
                    (parentCfg.Vedges(curr, null).Count() == 2).AssertTrue();
                    (parentCfg.BackVedges(null, curr).Count() == 0).AssertTrue();
                    var localEdge = cfg.Vedges(curr, null).AssertSingle();
                    localEdge.IsConditional.AssertTrue();
                    expected = localEdge.Target;

                    _block.AddElements(curr.BalancedCode);
                    var test = curr.Residue.AssertSingle();
                    test = localEdge.Condition == PredicateType.IsTrue ? Operator.Not(test) :
                        localEdge.Condition == PredicateType.IsFalse ? test :
                        ((Func<Expression>)(() => { throw AssertionHelper.Fail(); }))();
                    _block.AddElements(new If(test, ComplexScope.Decompile(this, offspring).Hir));
                }
                else
                {
                    if (_localCfg.BackVedges(null, curr).IsNotEmpty())
                    {
                        var loop = LoopScope.Decompile(this, curr);
                        todo.RemoveElements(loop.Test, loop.Continue);
                        todo.RemoveElements(loop.Body.Vertices);
                        todo.RemoveElements(loop.Offsprings.SelectMany(off => off.Body));

                        _block.AddElements(loop.Hir);
                        expected = loop.Conv;
                    }
                    else if (_localCfg.Vedges(curr, null).Count() >= 2)
                    {
                        (_localCfg.TreeVedges(curr, null).Count() == 2).AssertTrue();
                        (_localCfg.BackVedges(curr, null).Count() == 0).AssertTrue();

                        var @if = IfScope.Decompile(this, curr);
                        todo.RemoveElements(@if.Test);
                        todo.RemoveElements(@if.IfTrue.Vertices);
                        todo.RemoveElements(@if.IfFalse.Vertices);
                        todo.RemoveElements(@if.Offsprings.SelectMany(off => off.Body));

                        _block.AddElements(@if.Hir);
                        expected = @if.Conv;
                    }
                    else
                    {
                        (_localCfg.TreeVedges(curr, null).Count() <= 1).AssertTrue();
                        (_localCfg.BackVedges(curr, null).Count() == 0).AssertTrue();
                        var e_next = _localCfg.TreeVedges(curr, null).SingleOrDefault();
                        (e_next == null).AssertEquiv(todo.IsEmpty());
                        expected = e_next == null ? null : e_next.Target;

                        var isPivot = pivotsRecursive.ContainsKey(curr);
                        isPivot.AssertImplies(e_next == null);
                        if (!isPivot)
                        {
                            _block.AddElements(curr.BalancedCode);
                            if (curr.Residue.IsNotEmpty())
                            {
                                var nextIsRetOf = pivotsRecursive.GetOrDefault(expected) as LambdaScope;
                                (nextIsRetOf != null && nextIsRetOf.Return == expected).AssertTrue();
                                _block.Add(curr.Residue.AssertSingle());
                            }
                        }
                        else
                        {
                            var scope = pivotsRecursive[curr];
                            if (scope is LambdaScope)
                            {
                                var lambda = scope.AssertCast<LambdaScope>();
                                if (curr == lambda.Return)
                                {
                                    var i_curr = cflow.IndexOf(curr).AssertThat(i => i != -1);
                                    var prev = cflow.NthOrDefault(i_curr - 1);
                                    var prevWasExpression = _block.LastOrDefault() is Expression;
                                    var prevHasResidue = prev != null && prev.Residue.IsNotEmpty();

                                    if (prevWasExpression && prevHasResidue)
                                    {
                                        var valueToRet = _block.Last().AssertCast<Expression>();
                                        _block.RemoveLast();
                                        _block.Add(new Return(valueToRet.DeepClone()));
                                    }
                                    else
                                    {
                                        var complex = parent as ComplexScope;
                                        var global = complex == null ? null : complex.Parent as LambdaScope;
                                        var canOmitReturn = global != null && global.Return == curr;
                                        if (!canOmitReturn) _block.Add(new Return());
                                    }
                                }
                                else
                                {
                                    throw AssertionHelper.Fail();
                                }
                            }
                            else if (scope is LoopScope)
                            {
                                var loop = scope.AssertCast<LoopScope>();
                                (loop == this.Parents().OfType<LoopScope>().First()).AssertTrue();
                                if (curr == loop.Continue)
                                {
                                    _block.Add(new Continue());
                                }
                                else if (curr == loop.Conv)
                                {
                                    _block.Add(new Break());
                                }
                                else
                                {
                                    throw AssertionHelper.Fail();
                                }
                            }
                            else
                            {
                                throw AssertionHelper.Fail();
                            }
                        }
                    }
                }
            }
        }

        public String Uri
        {
            get
            {
                String self;
                if (Parent is LambdaScope)
                {
                    self = "body";
                }
                else if (Parent is ComplexScope)
                {
                    self = "body";
                }
                else if (Parent is LoopScope)
                {
                    self = "body";
                }
                else if (Parent is IfScope)
                {
                    var @if = Parent.Hir.AssertCast<If>();
                    if (Hir == @if.IfTrue) self = "true";
                    else if (Hir == @if.IfFalse) self = "false";
                    else throw AssertionHelper.Fail();
                }
                else
                {
                    throw AssertionHelper.Fail();
                }

                return Parent.Uri + " :: " + self;
            }
        }

        public override String ToString()
        {
            var lite = Uri.Replace("complex :: ", "");
            lite = lite.Replace("body :: ", "");
            return "{" + lite + "}";
        }
    }
}