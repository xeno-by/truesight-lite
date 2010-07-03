using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Truesight.Decompiler.Hir.Core.ControlFlow;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Core.Functional;
using Truesight.Decompiler.Hir.Core.Symbols;
using Truesight.Decompiler.Hir.Traversal;
using Truesight.Decompiler.Hir.Traversal.Traversers;
using Truesight.Decompiler.Hir.TypeInference;
using XenoGears.Traits.Dumpable;
using XenoGears.Logging;
using XenoGears.Reflection.Emit;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Traits.Hierarchy;
using XenoGears.Reflection;
using Convert = Truesight.Decompiler.Hir.Core.Expressions.Convert;
using HirLabel = Truesight.Decompiler.Hir.Core.ControlFlow.Label;
using EmitLabel = System.Reflection.Emit.Label;

namespace Truesight.Playground.InAction
{
    // todo. emit Addr/Deref for some cases of calling methods on structs
    // todo. correctly process [op=] and inc/dec with non-trivial roots
    // todo. emit *_Un instructions where appropriate
    // todo. same stuff for *_Ovf and other non-trivial suffixes

    internal partial class Crosscompiler<T1, T2, T3> : AbstractHirTraverser
    {
        private void CompileTransformedHir()
        {
            Log.TraceLine(_xhir.DumpAsText());
            Traverse(_xhir);
            il.ret();
        }

        private ILGenerator il { get { return _m_xformed.il(); } }
        private Dictionary<Local, LocalBuilder> locals = new Dictionary<Local, LocalBuilder>();
        private Dictionary<Loop, EmitLabel> continues = new Dictionary<Loop, EmitLabel>();
        private Dictionary<Loop, EmitLabel> breaks = new Dictionary<Loop, EmitLabel>();

        protected internal override void TraverseBlock(Block block)
        {
            Func<Type, Type> safeType = t => t.IsRectMdArray() ? typeof(Object) : t;
            block.Locals.ForEach(l => locals.Add(l, il.DeclareLocal(safeType(l.Type.AssertNotNull()))));
            block.ForEach(Traverse);
        }

        protected internal override void TraverseIf(If @if)
        {
            var @break = il.DefineLabel();
            var iffalse = il.DefineLabel();

            Traverse(@if.Test);
            il.brfalse(iffalse);
            Traverse(@if.IfTrue);
            il.br(@break);
            il.label(iffalse);
            Traverse(@if.IfFalse);
            il.label(@break);
        }

        protected internal override void TraverseLoop(Loop loop)
        {
            Func<Type, Type> safeType = t => t.IsRectMdArray() ? typeof(Object) : t;
            loop.Locals.ForEach(l => locals.Add(l, il.DeclareLocal(safeType(l.Type.AssertNotNull()))));

            var test = il.DefineLabel();
            var @continue = il.DefineLabel();
            var body = il.DefineLabel();
            var @break = il.DefineLabel();
            continues.Add(loop, @continue);
            breaks.Add(loop, @break);

            Traverse(loop.Init);
            if (loop.IsDoWhile) il.br(body);
            il.label(test);
            Traverse(loop.Test);
            il.brfalse(@break);
            il.label(body);
            Traverse(loop.Body);
            il.label(@continue);
            Traverse(loop.Iter);
            il.br(test);
            il.label(@break);
        }

        protected internal override void TraverseBreak(Break @break)
        {
            var loop = IHierarchyExtensions.Parents(@break).OfType<Loop>().AssertFirst();
            il.br(breaks[loop]);
        }

        protected internal override void TraverseContinue(Continue @continue)
        {
            var loop = IHierarchyExtensions.Parents(@continue).OfType<Loop>().AssertFirst();
            il.br(continues[loop]);
        }

        protected internal override void TraverseReturn(Return @return)
        {
            if (@return.Value != null) Traverse(@return.Value);
            il.ret();
        }

        protected internal override void TraverseAssign(Assign ass)
        {
            if (ass.Parent is Expression)
            {
                throw new NotImplementedException();
            }
            else
            {
                if (ass.Lhs is Ref)
                {
                    var sym = ((Ref)ass.Lhs).Sym;
                    if (sym.IsLocal())
                    {
                        Traverse(ass.Rhs);
                        il.stloc(locals[(Local)sym]);
                    }
                    else if (sym.IsParam())
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        throw AssertionHelper.Fail();
                    }
                }
                else if (ass.Lhs is Fld)
                {
                    var fld = (Fld)ass.Lhs;
                    if (fld.This != null) Traverse(fld.This);
                    Traverse(ass.Rhs);
                    il.stfld(fld.Field);
                }
                else
                {
                    var p = ass.InvokedProperty();
                    if (p != null)
                    {
                        var prop = ass.InvokedProp();
                        var setter = prop.Property.GetSetMethod(true);
                        var @this = prop.Property.IsInstance() ? prop.This.MkArray() : Seq.Empty<Expression>();
                        var args = (ass.InvocationIndexers() ?? Seq.Empty<Expression>()).Concat(ass.Rhs);
                        var style = prop.InvokedAsVirtual ? InvocationStyle.Virtual : InvocationStyle.NonVirtual;
                        var equiv = new Eval(new Apply(new Lambda(setter, style), @this.Concat(args)));
                        Traverse(equiv);
                    }
                    else
                    {
                        // todo. we've really got to consider array-getter an lvalue
                        // currently I don't have time for that
                        // but this needs to be propagated through the entire code

                        var m = ass.Lhs.InvokedMethod();
                        if (m.IsArrayGetter())
                        {
                            var setter = m.DeclaringType.ArraySetter();
                            var args = ass.Lhs.InvocationArgs().Concat(ass.Rhs);
                            var equiv = new Eval(new Apply(new Lambda(setter), args));
                            Traverse(equiv);
                        }
                        else
                        {
                            throw AssertionHelper.Fail();
                        }
                    }
                }
            }
        }

        protected internal override void TraverseConditional(Conditional cond)
        {
            var @break = il.DefineLabel();
            var iffalse = il.DefineLabel();

            Traverse(cond.Test);
            il.brfalse(iffalse);
            Traverse(cond.IfTrue);
            il.br(@break);
            il.label(iffalse);
            Traverse(cond.IfFalse);
            il.label(@break);
        }

        protected internal override void TraverseConst(Const @const)
        {
            var v = @const.Value;
            var t = @const.Type;
            if (v == null)
            {
                il.ldnull();
            }
            else
            {
                if (t == typeof(bool))
                {
                    if ((bool)v) il.ldtrue();
                    else il.ldfalse();
                }
                else if (t == typeof(int) || t == typeof(uint))
                {
                    unchecked
                    {
                        il.ldc_i4((int)v);
                    }
                }
                else if (t == typeof(long) || t == typeof(long))
                {
                    unchecked
                    {
                        il.ldc_i8((long)v);
                    }
                }
                else if (t == typeof(float))
                {
                    il.ldc_r4((float)v);
                }
                else if (t == typeof(double))
                {
                    il.ldc_r8((double)v);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        protected internal override void TraverseConvert(Convert cvt)
        {
            var s = cvt.Type();
            var t = cvt.Type;

            Traverse(cvt.Source);
            if (s == typeof(Object) && t.IsValueType)
            {
                il.unbox_any(t);
            }
            else if (s.IsValueType && t == typeof(Object))
            {
                il.box(t);
            }
            else if (t == typeof(int))
            {
                il.Emit(OpCodes.Conv_I4);
            }
            else if (t == typeof(uint))
            {
                il.Emit(OpCodes.Conv_U4);
            }
            else if (t == typeof(long))
            {
                il.Emit(OpCodes.Conv_I8);
            }
            else if (t == typeof(ulong))
            {
                il.Emit(OpCodes.Conv_U8);
            }
            else if (t == typeof(float))
            {
                il.Emit(OpCodes.Conv_R4);
            }
            else if (t == typeof(double))
            {
                il.Emit(OpCodes.Conv_R8);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        protected internal override void TraverseFld(Fld fld)
        {
            var ass = fld.Parent as Assign;
            (ass != null && ass.Lhs == fld).AssertFalse();

            if (fld.This != null) Traverse(fld.This);
            il.ldfld(fld.Field);
        }

        protected internal override void TraverseProp(Prop prop)
        {
            var ass = prop.Parent as Assign;
            (ass != null && ass.Lhs == prop).AssertFalse();

            var getter = prop.Property.GetGetMethod(true);
            var @this = prop.Property.IsInstance() ? prop.This.MkArray() : Seq.Empty<Expression>();
            var args = prop.InvocationIndexers() ?? Seq.Empty<Expression>();
            var style = prop.InvokedAsVirtual ? InvocationStyle.Virtual : InvocationStyle.NonVirtual;
            var equiv = new Eval(new Apply(new Lambda(getter, style), @this.Concat(args)));
            Traverse(equiv);
        }

        protected internal override void TraverseApply(Apply apply)
        {
            var ass = apply.Parent as Assign;
            (ass != null && ass.Lhs == apply).AssertFalse();
            var prop = apply.InvokedProp().AssertNotNull();

            var getter = prop.Property.GetGetMethod(true);
            var @this = prop.Property.IsInstance() ? prop.This.MkArray() : Seq.Empty<Expression>();
            var args = prop.InvocationIndexers() ?? Seq.Empty<Expression>();
            var style = prop.InvokedAsVirtual ? InvocationStyle.Virtual : InvocationStyle.NonVirtual;
            var equiv = new Eval(new Apply(new Lambda(getter, style), @this.Concat(args)));
            Traverse(equiv);
        }

        protected internal override void TraverseRef(Ref @ref)
        {
            var sym = @ref.Sym;
            if (sym.IsLocal())
            {
                il.ldloc(locals[(Local)sym]);
            }
            else if (sym.IsParam())
            {
                var p = (Param)sym;
                var index = p == _this ? 0 : p == _blockIdx ? 1 : ((Func<int>)(() =>
                {
                    var i_param = _params.IndexOf(p1 => p1 == p);
                    (i_param > 0).AssertTrue();
                    return i_param + 1;
                }))();
                il.ldarg(index);
            }
            else
            {
                throw AssertionHelper.Fail();
            }
        }

        protected internal override void TraverseOperator(Operator op)
        {
            var lhs = op.Args.FirstOrDefault();
            var rhs = op.Args.SecondOrDefault();
            var targ = op.Args.FirstOrDefault();

            var opt = op.OperatorType;
            if (opt.IsAssign())
            {
                rhs = rhs ?? new Const(1); // hack for inc/decrements
                var equiv = new Assign(lhs, Operator.Create(opt.Unassign(), lhs, rhs));
                Traverse(equiv);
            }
            else if (opt == OperatorType.AndAlso)
            {
                var equiv = new Conditional(lhs, rhs, new Const(false));
                Traverse(equiv);
            }
            else if (opt == OperatorType.OrElse)
            {
                var equiv = new Conditional(lhs, new Const(true), rhs);
                Traverse(equiv);
            }
            else
            {
                op.Args.ForEach(Traverse);

                switch (opt)
                {
                    case OperatorType.Add:
                        il.add();
                        break;
                    case OperatorType.And:
                        il.and();
                        break;
                    case OperatorType.Divide:
                        il.div();
                        break;
                    case OperatorType.Equal:
                        il.ceq();
                        break;
                    case OperatorType.GreaterThan:
                        il.cgt();
                        break;
                    case OperatorType.GreaterThanOrEqual:
                        il.cge();
                        break;
                    case OperatorType.LeftShift:
                        il.shl();
                        break;
                    case OperatorType.LessThan:
                        il.clt();
                        break;
                    case OperatorType.LessThanOrEqual:
                        il.cle();
                        break;
                    case OperatorType.Modulo:
                        il.rem();
                        break;
                    case OperatorType.Multiply:
                        il.mul();
                        break;
                    case OperatorType.Negate:
                        il.neg();
                        break;
                    case OperatorType.Not:
                        il.ldc_i4(0).ceq();
                        break;
                    case OperatorType.NotEqual:
                        il.cne();
                        break;
                    case OperatorType.Or:
                        il.or();
                        break;
                    case OperatorType.RightShift:
                        il.shr();
                        break;
                    case OperatorType.Subtract:
                        il.sub();
                        break;
                    case OperatorType.Xor:
                        il.xor();
                        break;
                    default:
                        throw AssertionHelper.Fail();
                }
            }
        }

        protected internal override void TraverseEval(Eval eval)
        {
            var m = eval.InvokedMethod();
            eval.InvocationArgs().ForEach(Traverse);

            var lam = eval.InvokedLambda();
            if (lam.InvokedAsCtor) il.newobj(m);
            else if (lam.InvokedAsVirtual) il.callvirt(m);
            else il.call(m);

            var rets_smth = lam.Sig.Ret != typeof(void);
            var ret_aint_used = !(Stack.SecondOrDefault() is Expression);
            if (rets_smth && ret_aint_used) il.pop();
        }
    }
}
