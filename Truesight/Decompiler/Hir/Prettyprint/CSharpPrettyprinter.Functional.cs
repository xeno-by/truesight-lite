using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Core.Functional;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Reflection;
using XenoGears.Reflection.Attributes;
using XenoGears.Reflection.Generics;
using XenoGears.Reflection.Shortcuts;
using XenoGears.Strings;
using Truesight.Decompiler.Hir.Traversal;
using Truesight.Decompiler.Hir.TypeInference;
using Convert = Truesight.Decompiler.Hir.Core.Expressions.Convert;

namespace Truesight.Decompiler.Hir.Prettyprint
{
    public partial class CSharpPrettyprinter
    {
        // todo. prettyprint and tests for it:
        //.1) show type args if the invocation is polymorphic
        // 2) do not show type args if they can be inferred
        protected internal override void TraverseEval(Eval eval)
        {
            var apply = eval.Callee;
            if (apply == null) _writer.Write("?()");
            else
            {
                if (apply.Callee is Lambda)
                {
                    var lam = (Lambda)apply.Callee;
                    var m = lam.Method;

                    if (m == null)
                    {
                        // todo. return to this when lambdas decompilation is implemented
                        // currently we do nothing and thusly fall back to the (m == null) case
                        //
                        // note. absence of lam.Method means that we face an ad-hoc lambda
                        // i.e. the one that's crafted manually rather than decompiled
                        // or the one that arose from decompiling c#'s anonymous delegate/lambda
                        Traverse(apply.Callee);
                        _writer.Write("(");
                        TraverseArgs(apply.ArgsInfo);
                        _writer.Write(")");
                    }
                    else if (m is ConstructorInfo)
                    {
                        if (m.DeclaringType.IsArray)
                        {
                            lam.InvokedAsCtor.AssertTrue();
                            _writer.Write("new ");
                            var t_el = m.DeclaringType.GetElementType().AssertNotNull();
                            _writer.Write(t_el.GetCSharpRef(ToCSharpOptions.Informative));
                            _writer.Write("[");
                            TraverseArgs(apply.ArgsInfo);
                            _writer.Write("]");
                        }
                        else
                        {
                            if (lam.InvokedAsCtor)
                            {
                                _writer.Write("new ");
                                _writer.Write(m.DeclaringType.GetCSharpRef(ToCSharpOptions.Informative));
                                _writer.Write("(");
                                TraverseArgs(apply.ArgsInfo);
                                _writer.Write(")");
                            }
                            else
                            {
                                _writer.Write("ctor");
                                _writer.Write("(");
                                TraverseArgs(apply.ArgsInfo.Skip(1));
                                _writer.Write(")");
                            }
                        }
                    }
                    else if (m is MethodInfo)
                    {
                        lam.InvokedAsCtor.AssertFalse();
                        if (m.IsUserDefinedOperator())
                        {
                            // todo. add support for op_UnaryPlus
                            var op_type = (OperatorType)Enum.Parse(typeof(OperatorType), m.UserDefinedOperatorType());
                            Traverse(Operator.Create(op_type, apply.Args));
                        }
                        else if (m.IsUserDefinedCast())
                        {
                            var targetType = m.Ret();
                            var source = apply.Args.Single();

                            if (m.IsExplicitCast())
                            {
                                Traverse(new Convert(targetType, source));
                            }
                            else if (m.IsImplicitCast())
                            {
                                // todo. think about cases when we can omit the cast
                                Traverse(new Convert(targetType, source));
                            }
                            else
                            {
                                throw AssertionHelper.Fail();
                            }
                        }
                        else
                        {
                            var isArrayAddress = m.DeclaringType.IsArray && m.Name == "Address";
                            if (isArrayAddress)
                            {
                                var getter = m.DeclaringType.ArrayGetter();
                                Traverse(new Addr(new Eval(new Apply(new Lambda(getter), apply.Args))));
                            }
                            else
                            {
                                // classify
                                var isArrayGetter = m.IsArrayGetter();
                                var isArraySetter = m.IsArraySetter();
                                var isArrayIndexer = isArrayGetter || isArraySetter;
                                var isRegularMethod = !isArrayIndexer;
                                var hasThis = m.IsInstance() || m.IsConstructor() || m.IsExtension();
                                var displayName = isArrayIndexer ? null : m.Name;

                                // qualifier
                                // todo. support explicit interface implementation
                                if (hasThis)
                                {
                                    Traverse(apply.Args.First());
                                }
                                else
                                {
                                    _writer.Write(m.DeclaringType.GetCSharpRef(ToCSharpOptions.Informative));
                                }

                                // member access
                                if (!isArrayIndexer)
                                {
                                    var @this = hasThis ? apply.Args.First() : null;
                                    var t_this = @this.Type();
                                    var is_ptr = t_this != null && t_this.IsPointer;
                                    _writer.Write(is_ptr ? "->" : ".");
                                }

                                // invocation
                                _writer.Write(displayName);
                                _writer.Write(m.GetCSharpTypeArgsClause(ToCSharpOptions.Informative));
                                _writer.Write(isArrayIndexer ? "[" : "(");
                                if (isRegularMethod) TraverseArgs(apply.ArgsInfo.Skip(hasThis ? 1 : 0));
                                else { isArrayIndexer.AssertTrue(); TraverseArgs(apply.ArgsInfo.Skip(hasThis ? 1 : 0).SkipLast(isArraySetter ? 1 : 0)); }
                                _writer.Write(isArrayIndexer ? "]" : ")");
                                if (isArraySetter) { _writer.Write(" = "); Traverse(apply.Args.Last()); }
                            }
                        }
                    }
                    else
                    {
                        throw AssertionHelper.Fail();
                    }
                }
                else
                {
                    // todo. return to this when lambdas decompilation is implemented
                    // currently we do nothing and thusly fall back to the (m == null) case
                    //
                    // note that the callee might be whatever you can imagine
                    // not only curried Apply, but also e.g. Field of lambda type
                    // i.e. when implementing this you must not ignore callees that are different from Apply
                    _writer.Write(apply.Callee);
                    _writer.Write("(");
                    TraverseArgs(apply.ArgsInfo);
                    _writer.Write(")");
                }
            }
        }

        protected internal override void TraverseApply(Apply apply)
        {
            if (apply.Callee is Prop)
            {
                var p = ((Prop)apply.Callee).Property;
                if (p != null)
                {
                    var m = (apply.Parent is Assign) ? p.GetSetMethod(true) : p.GetGetMethod(true);

                    // classify
                    var hasThis = p.IsInstance() || p.IsExtension();
                    var isIndexer = p.IsIndexer();
                    var isDefaultIndexer = p.IsDefaultIndexer();
                    var displayName = isDefaultIndexer ? null : isIndexer ?
                    ((Func<String>)(() =>
                    {
                        if (p.HasAttr<IndexerNameAttribute>())
                        {
                            var inaCtor = typeof(IndexerNameAttribute).GetConstructor(typeof(String).MkArray());
                            (inaCtor.GetParameters().AssertSingle().Name == "indexerName").AssertTrue();
                            var inaCad = CustomAttributeData.GetCustomAttributes(p).Single(cad => cad.Constructor == inaCtor);
                            var inaCarg = inaCad.ConstructorArguments.Single();
                            (inaCarg.ArgumentType == typeof(String)).AssertTrue();
                            return inaCarg.Value.AssertCast<String>();
                        }
                        else
                        {
                            return p.Name;
                        }
                    }))() : p.Name;

                    // qualifier
                    // todo. support explicit interface implementation
                    var target = apply.Callee.AssertCast<Prop>().This;
                    if (hasThis)
                    {
                        Traverse(target);
                    }
                    else
                    {
                        target.AssertNull();
                        _writer.Write(p.DeclaringType.GetCSharpRef(ToCSharpOptions.Informative));
                    }

                    // invocation
                    if (!isDefaultIndexer)
                    {
                        var @this = ((Prop)apply.Callee).This;
                        var t_this = @this.Type();
                        var is_ptr = t_this != null && t_this.IsPointer;
                        _writer.Write(is_ptr ? "->" : ".");
                    }

                    _writer.Write(displayName);
                    _writer.Write(m.GetCSharpTypeArgsClause(ToCSharpOptions.Informative));
                    _writer.Write("[");
                    TraverseArgs(apply.ArgsInfo);
                    _writer.Write("]");
                }
            }
            else
            {
                // todo. return to this when lambdas decompilation is implemented
                _writer.Write("λ");
            }

        }

        protected internal override void TraverseLambda(Lambda lambda)
        {
            // todo. return to this when lambdas decompilation is implemented
            _writer.Write("λ");
        }

        private void TraverseArgs(IEnumerable<Tuple<Expression, ParamInfo>> args)
        {
            args.Zip((arg, pi, i) =>
            {
                // this works correctly because ref/out and varargs are mutually exclusive
                var meta = pi == null ? null : pi.Metadata;
                if (meta != null && meta.ParameterType.IsByRef)
                {
                    var is_out = meta.ParameterType.HasAttr<OutAttribute>();
                    _writer.Write(is_out ? "out " : "ref ");
                }

                var aintFirst = i != 0;
                var aintLast = i != args.Count() - 1;
                if (aintLast)
                {
                    if (aintFirst) _writer.Write(", ");
                    Traverse(arg);
                }
                else
                {
                    if (meta.IsVarargs())
                    {
                        var ctor = arg.InvokedCtor();
                        if (ctor != null && ctor.DeclaringType.IsArray)
                        {
                            var eval = arg as Eval;
                            if (eval != null)
                            {
                                var rank = ctor.DeclaringType.GetArrayRank();
                                if (rank == 1)
                                {
                                    var sole_arg = arg.InvocationArgs().AssertSingle() as Const;
                                    if (sole_arg != null && 
                                        sole_arg.Value is int && (int)sole_arg.Value == 0)
                                    {
                                        return;
                                    }
                                }
                            }

                            var ci = arg as CollectionInit;
                            if (ci != null)
                            {
                                if (aintFirst && ci.Elements.IsNotEmpty()) _writer.Write(", ");
                                TraverseArgs(ci.Elements.Zip(Seq.Infinite(null as ParamInfo)));
                                return;
                            }
                        }
                    }

                    if (aintFirst) _writer.Write(", ");
                    Traverse(arg);
                }
            });
        }
    }
}
