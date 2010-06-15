using System;
using System.Collections.ObjectModel;
using System.Linq;
using Truesight.Decompiler.Hir.Core.ControlFlow;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Core.Functional;
using Truesight.Decompiler.Hir.TypeInference;
using Truesight.Decompiler.Pipeline.Attrs;
using Truesight.Decompiler.Hir.Traversal.Transformers;
using XenoGears.Functional;
using XenoGears.Reflection;
using XenoGears.Assertions;
using Truesight.Decompiler.Hir.Traversal;
using XenoGears.Reflection.Generics;

namespace Truesight.Decompiler.Pipeline.Hir
{
    [Decompiler(Weight = (int)Stages.PostprocessHir)]
    internal static class RestoreBooleans
    {
        [DecompilationStep(Weight = 2)]
        public static Block DoRestoreBooleans(Block hir)
        {
            // todo. also think about how to support nullables
            var types = hir.InferTypes();
            return (Block)hir.Transform(
                (If @if) =>
                {
                    var is_bool = types[@if.Test] == typeof(bool);
                    var ensuredTest = is_bool ? @if.Test.CurrentTransform() : @if.Test.EnsureBoolean(types);
                    var ifTrue = @if.IfTrue.CurrentTransform();
                    var ifFalse = @if.IfFalse.CurrentTransform();
                    return new If(ensuredTest, ifTrue, ifFalse);
                },
                (Assign ass) =>
                {
                    if (types[ass.Lhs] == typeof(bool))
                    {
                        var lhs = ass.Lhs.CurrentTransform();
                        var ensuredRhs = ass.Rhs.EnsureBoolean(types);
                        return new Assign(lhs, ensuredRhs);
                    }
                    else
                    {
                        return ass.DefaultTransform();
                    }
                },
                (Apply app) =>
                {
                    ReadOnlyCollection<Type> targs = null;
                    if (app.Callee is Prop)
                    {
                        var p = app.Callee.InvokedProperty();
                        if (p != null) targs = p.GetIndexParameters().Select(pi => pi.ParameterType).ToReadOnly();
                    }
                    else
                    {
                        var ftype = types[app.Callee];
                        if (ftype != null) targs = ftype.DasmFType().SkipLast(1).ToReadOnly();
                    }

                    if (targs == null) return app.DefaultTransform();
                    var ensuredArgs = targs.Select((targ, i) =>
                    {
                        var arg = app.Args[i];
                        var is_bool = targ == typeof(bool);
                        return is_bool ? arg.EnsureBoolean(types) : arg.CurrentTransform();
                    });

                    var callee = app.Callee.CurrentTransform();
                    return new Apply(callee, ensuredArgs);
                },
                (Conditional cond) =>
                {
                    var anyBool = types[cond.IfTrue] == typeof(bool) || types[cond.IfFalse] == typeof(bool);
                    var is_bool = types[cond.Test] == typeof(bool);
                    var ensuredTest = is_bool ? cond.Test.CurrentTransform() : cond.Test.EnsureBoolean(types);
                    var ifTrue = anyBool ? cond.IfTrue.EnsureBoolean(types) : cond.IfTrue.CurrentTransform();
                    var ifFalse = anyBool ? cond.IfFalse.EnsureBoolean(types) : cond.IfFalse.CurrentTransform();
                    return new Conditional(ensuredTest, ifTrue, ifFalse);
                },
                (Operator op) =>
                {
                    var opt = op.OperatorType;
                    var anyBool = op.Args.Any(arg => types[arg] == typeof(bool));
                    var alwaysBool = opt == OperatorType.AndAlso || 
                        opt == OperatorType.OrElse || opt == OperatorType.Not;
                    var ensureBool = anyBool || alwaysBool;
                    if (ensureBool)
                    {
                        if (opt == OperatorType.And) opt = OperatorType.AndAlso;
                        if (opt == OperatorType.Or) opt = OperatorType.OrElse;
                        if (opt == OperatorType.Negate) opt = OperatorType.Not;

                        var ensuredArgs = op.Args.Select(arg => arg.EnsureBoolean(types));
                        return Operator.Create(opt, ensuredArgs);
                    }
                    else
                    {
                        return op.DefaultTransform();
                    }
                });
        }

        private static Expression EnsureBoolean(this Expression e, TypeInferenceCache types)
        {
            var t = types[e];
            if (t == typeof(bool)) return e.CurrentTransform();
            else
            {
                if (t.IsValueType)
                {
                    t.IsInteger().AssertTrue();
                    var @const = e as Const;
                    if (@const != null)
                    {
                        var value = @const.Value.AssertCast<int>();
                        (value == 0 || value == 1).AssertTrue();
                        return new Const(value == 1);
                    }

                    return Operator.NotEqual(e.CurrentTransform(), new Const(0));
                }
                else
                {
                    return Operator.NotEqual(e.CurrentTransform(), new Const(null, t));
                }
            }
        }
    }
}