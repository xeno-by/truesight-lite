using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Reflection;
using Truesight.Decompiler.Hir.Core.ControlFlow;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Core.Functional;
using Truesight.Decompiler.Hir.Core.Special;
using Truesight.Decompiler.Hir.Traversal.Traversers;
using XenoGears.Reflection.Generics;
using Convert=Truesight.Decompiler.Hir.Core.Expressions.Convert;

namespace Truesight.Decompiler.Hir.TypeInference
{
    public class TypeInferenceTraverser : AbstractHirTraverser
    {
        private readonly Dictionary<Node, Type> _types = new Dictionary<Node, Type>();
        public Dictionary<Node, Type> Types { get { return _types; } }

        [DebuggerNonUserCode]
        protected override void TraverseNode(Node node)
        {
            if (node == null) base.TraverseNode(node);
            else
            {
                var cache = node.Domain == null ? null : node.Domain.TypeInferenceCache;
                var is_cached = cache != null && cache.ContainsKey(node);
                if (is_cached)
                {
                    node.Children.ForEach(Traverse);
                    Types.Add(node, cache[node]);
                }
                else
                {
                    base.TraverseNode(node);
                    if (cache != null && node != null) cache[node] = Types[node];
                }
            }
        }

        protected internal override void TraverseOperator(Operator op)
        {
            op.Args.ForEach(Traverse);

            // todo. also support nullables and lifted versions of operators
            op.Args.Any(arg => Types[arg].IsNullable()).AssertFalse();

            var impArgs = op.Args;
            var anyBool = impArgs.Any(arg => Types[arg] == typeof(bool));
            if (anyBool) { Types.Add(op, typeof(bool)); return; }

            var opt = op.OperatorType;
            if (opt.IsAssign() || opt == OperatorType.Coalesce || 
                opt == OperatorType.LeftShift || opt == OperatorType.RightShift)
            {
                Types.Add(op, Types[op.Args.First()]);
            }
            else if (opt == OperatorType.AndAlso || opt == OperatorType.OrElse || opt == OperatorType.Not ||
                opt == OperatorType.GreaterThan || opt == OperatorType.GreaterThanOrEqual ||
                opt == OperatorType.Equal || opt == OperatorType.NotEqual ||
                opt == OperatorType.LessThan || opt == OperatorType.LessThanOrEqual)
            {
                Types.Add(op, typeof(bool));
            }
            else
            {
                if (op.IsUnary())
                {
                    var t_target = Types[op.Unary().Target];
                    if (t_target == null) Types.Add(op, null);
                    else
                    {
                        (t_target.IsInteger() || t_target.IsFloatingPoint()).AssertTrue();
                        Types.Add(op, t_target);
                    }
                }
                else if (op.IsBinary())
                {
                    var t_lhs = Types[op.Binary().Lhs];
                    var t_rhs = Types[op.Binary().Rhs];
                    if (t_lhs == null || t_rhs == null) Types.Add(op, null);
                    else
                    {
                        if (t_lhs.IsInteger() && t_rhs.IsInteger())
                        {
                            Func<Type, int> bitness = t => 
                                t == typeof(sbyte) || t == typeof(byte) ? 8 :
                                t == typeof(short) || t == typeof(ushort) ? 16 :
                                t == typeof(int) || t == typeof(uint) ? 32 :
                                t == typeof(long) || t == typeof(ulong) ? 64 :
                                ((Func<int>)(() => { throw AssertionHelper.Fail(); }))();
                            Func<Type, bool> signed = t => 
                                t == typeof(sbyte) || t == typeof(short) ||
                                t == typeof(byte) || t == typeof(ushort) ? true :
                                t == typeof(int) || t == typeof(long) ||
                                t == typeof(uint) || t == typeof(ulong) ? false :
                                ((Func<bool>)(() => { throw AssertionHelper.Fail(); }))();
                            Func<int, bool, Type> mk_int = (n_bits, is_signed) =>
                            {
                                if (n_bits == 8) return is_signed ? typeof(sbyte) : typeof(byte);
                                if (n_bits == 16) return is_signed ? typeof(short) : typeof(ushort);
                                if (n_bits == 32) return is_signed ? typeof(int) : typeof(uint);
                                if (n_bits == 64) return is_signed ? typeof(long) : typeof(ulong);
                                throw AssertionHelper.Fail();
                            };

                            var bits = Math.Max(bitness(t_lhs), bitness(t_rhs));
                            var sign = bitness(t_lhs) > bitness(t_rhs) ? signed(t_lhs) :
                                bitness(t_lhs) > bitness(t_rhs) ? signed(t_lhs) : 
                                signed(t_lhs) || signed(t_rhs);

                            var t_result = mk_int(bits, sign);
                            Types.Add(op, t_result);
                        }
                        else
                        {
                            (t_lhs.IsFloatingPoint() && t_rhs.IsFloatingPoint()).AssertTrue();
                            if (t_lhs == typeof(double) || t_rhs == typeof(double)) Types.Add(op, typeof(double));
                            else if (t_lhs == typeof(float) || t_rhs == typeof(float)) Types.Add(op, typeof(float));
                            else throw AssertionHelper.Fail();
                        }
                    }
                }
                else
                {
                    throw AssertionHelper.Fail();
                }
            }
        }

        protected internal override void TraverseConditional(Conditional cond)
        {
            cond.Children.ForEach(Traverse);
            var t_iftrue = Types[cond.IfTrue];
            var t_iffalse = Types[cond.IfFalse];

            if (t_iftrue == null || t_iffalse == null) Types.Add(cond, null);
            else
            {
                (t_iftrue == t_iffalse).AssertTrue();
                Types.Add(cond, t_iftrue);
            }
        }

        protected internal override void TraverseApply(Apply app)
        {
            Traverse(app.Callee);
            app.Args.ForEach(Traverse);

            var callee = app.Callee;
            if (callee is Lambda || callee is Apply)
            {
                var t_lam = Types[callee];
                if (t_lam == null) Types.Add(app, null);
                else
                {
                    var rest = t_lam.DasmFType().Skip(app.Args.Count()).ToReadOnly();
                    Types.Add(app, rest.AsmFType());
                }
            }
            else if (callee is Prop)
            {
                var t_prop = Types[callee];
                Types.Add(app, t_prop);
            }
            else
            {
                (app.Callee == null).AssertTrue();
                Types.Add(app, null);
            }
        }

        protected internal override void TraverseEval(Eval eval)
        {
            Traverse(eval.Callee);

            var t_app = Types[eval.Callee];
            if (t_app == null) Types.Add(eval, null);
            else
            {
                t_app.Params().AssertEmpty();
                Types.Add(eval, t_app.Ret());
            }
        }

        protected internal override void TraverseLambda(Lambda lambda)
        {
            var sig = lambda.Sig.Params.Select(p => p.Type).Concat(lambda.Sig.Ret).ToReadOnly();
            if (sig.Any(t => t == null)) Types.Add(lambda, null);
            else Types.Add(lambda, sig.AsmFType());
        }

        #region Trivial cases

        protected internal override void TraverseNull(Null @null)
        {
            // do nothing
        }

        protected internal override void TraverseAddr(Addr addr)
        {
            Traverse(addr.Target);

            var t = Types[addr.Target];
            var t_ref = null as Type;
            if (t != null) t_ref = t.MakePointerType();
            Types.Add(addr, t_ref);
        }

        protected internal override void TraverseAssign(Assign ass)
        {
            Traverse(ass.Lhs);
            Traverse(ass.Rhs);
            Types.Add(ass, Types[ass.Rhs]);
        }

        protected internal override void TraverseConst(Const @const)
        {
            Types.Add(@const, @const.Type);
        }

        protected internal override void TraverseCollectionInit(CollectionInit ci)
        {
            Traverse(ci.Ctor);
            ci.Elements.ForEach(Traverse);
            Types.Add(ci, Types[ci.Ctor]);
        }

        protected internal override void TraverseConvert(Convert cvt)
        {
            Traverse(cvt.Source);
            Types.Add(cvt, cvt.Type);
        }

        protected internal override void TraverseDeref(Deref deref)
        {
            Traverse(deref.Target);

            var t = Types[deref.Target];
            var t_deref = null as Type;
            if (t != null)
            {
                (t.IsByRef || t.IsPointer).AssertTrue();
                t_deref = t.GetElementType();
            }

            Types.Add(deref, t_deref);
        }

        protected internal override void TraverseFld(Fld fld)
        {
            Traverse(fld.This);
            Types.Add(fld, fld.Field == null ? null : fld.Field.FieldType);
        }

        protected internal override void TraverseLoophole(Loophole loophole)
        {
            Types.Add(loophole, null);
        }

        protected internal override void TraverseObjectInit(ObjectInit oi)
        {
            Traverse(oi.Ctor);
            oi.MemberInits.Values.ForEach(Traverse);
            Types.Add(oi, Types[oi.Ctor]);
        }

        protected internal override void TraverseProp(Prop prop)
        {
            Traverse(prop.This);
            Types.Add(prop, prop.Property == null ? null : prop.Property.PropertyType);
        }

        protected internal override void TraverseRef(Ref @ref)
        {
            var t_ref = @ref.Sym == null ? null : (@ref.Sym.Type ?? typeof(Object));
            if (t_ref != null && t_ref.IsByRef)
            {
                // todo. later think about other cases when we need to type byrefs as T&
                // as far as I can imagine, all of those involve pointers:
                // 1) assignment to a pointer, 2) assignment from a pointer, 3) passing as a pointer parameter
                var used_as_ptr = @ref.Parent is Deref;
                if (!used_as_ptr) t_ref = t_ref.GetElementType();
            }

            Types.Add(@ref, t_ref);
        }

        protected internal override void TraverseSizeof(SizeOf @sizeof)
        {
            Types.Add(@sizeof, typeof(uint));
        }

        protected internal override void TraverseTypeIs(TypeIs typeIs)
        {
            Types.Add(typeIs, typeof(bool));
        }

        protected internal override void TraverseTypeAs(TypeAs typeAs)
        {
            Types.Add(typeAs, typeAs.Type);
        }

        protected internal override void TraverseDefault(Default @default)
        {
            Types.Add(@default, @default.Type);
        }

        protected internal override void TraverseBlock(Block block)
        {
            block.ForEach(Traverse);
            Types.Add(block, null);
        }

        protected internal override void TraverseBreak(Break @break)
        {
            Types.Add(@break, null);
        }

        protected internal override void TraverseContinue(Continue @continue)
        {
            Types.Add(@continue, null);
        }

        protected internal override void TraverseCatch(Catch @catch)
        {
            Traverse(@catch.Filter);
            @catch.ForEach(Traverse);
            Types.Add(@catch, null);
        }

        protected internal override void TraverseGoto(Goto @goto)
        {
            Types.Add(@goto, null);
        }

        protected internal override void TraverseLabel(Label label)
        {
            Types.Add(label, null);
        }

        protected internal override void TraverseIf(If @if)
        {
            Traverse(@if.Test);
            Traverse(@if.IfTrue);
            Traverse(@if.IfFalse);
            Types.Add(@if, null);
        }

        protected internal override void TraverseLoop(Loop loop)
        {
            Traverse(loop.Init);
            Traverse(loop.Test);
            Traverse(loop.Body);
            Traverse(loop.Iter);
            Types.Add(loop, null);
        }

        protected internal override void TraverseReturn(Return @return)
        {
            Traverse(@return.Value);
            Types.Add(@return, null);
        }

        protected internal override void TraverseThrow(Throw @throw)
        {
            Traverse(@throw.Exception);
            Types.Add(@throw, null);
        }

        protected internal override void TraverseTry(Try @try)
        {
            Traverse(@try.Body);
            @try.Clauses.ForEach(Traverse);
            Types.Add(@try, null);
        }

        protected internal override void TraverseUsing(Using @using)
        {
            Traverse(@using.Init);
            Traverse(@using.Body);
            Types.Add(@using, null);
        }

        protected internal override void TraverseIter(Iter iter)
        {
            Traverse(iter.Seq);
            Traverse(iter.Body);
            Types.Add(iter, null);
        }

        #endregion
    }
}
