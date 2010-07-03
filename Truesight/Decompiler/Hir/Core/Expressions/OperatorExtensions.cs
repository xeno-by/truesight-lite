using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Truesight.Decompiler.Hir.Core.Functional;
using Truesight.Decompiler.Hir.Core.Symbols;
using Truesight.Decompiler.Hir.Traversal;
using XenoGears.Assertions;
using XenoGears.Functional;
using XenoGears.Reflection;
using Truesight.Decompiler.Hir.Traversal.Transformers;
using Truesight.Decompiler.Hir.TypeInference;

namespace Truesight.Decompiler.Hir.Core.Expressions
{
    [DebuggerNonUserCode]
    public static class OperatorExtensions
    {
        public static UnaryOperator Unary(this Operator op)
        {
            return op.AssertCast<UnaryOperator>();
        }

        public static bool IsUnary(this Operator op)
        {
            return op.Args.Count() == 1;
        }

        public static BinaryOperator Binary(this Operator op)
        {
            return op.AssertCast<BinaryOperator>();
        }

        public static bool IsBinary(this Operator op)
        {
            return op.Args.Count() == 2;
        }

        public static Expression UnsafeExpandOpAssign(this Expression root) { return (Expression)UnsafeExpandOpAssign((Node)root); }
        public static Node UnsafeExpandOpAssign(this Node root)
        {
            return root.Transform((Operator op) =>
            {
                var opt = op.OperatorType;
                if (!opt.IsAssign()) return (Expression)op.DefaultTransform();

                var lhs = op.Args.FirstOrDefault();
                var rhs = op.Args.SecondOrDefault() ?? new Const(1); // hack for inc/decrements
                return new Assign(lhs, Operator.Create(opt.Unassign(), lhs, rhs));
            }).AssertCast<Node>();
        }

        public static Expression SafeExpandOpAssign(this Expression root, out Dictionary<Operator, Assign> out_roots) { return (Expression)SafeExpandOpAssign((Node)root, out out_roots); }
        public static Node SafeExpandOpAssign(this Node root, out Dictionary<Operator, Assign> out_roots)
        {
            var roots = new Dictionary<Operator, Assign>();
            var x_root = root.Transform((Operator op) =>
            {
                var opt = op.OperatorType;
                if (!opt.IsAssign()) return (Expression)op.DefaultTransform();

                Func<Expression, Expression> mk_safe_lhs = lhs =>
                {
                    var @ref = lhs as Ref;
                    if (@ref != null) return lhs;

                    var slot = lhs as Slot;
                    if (slot != null)
                    {
                        var @this = slot.This;
                        if (@this == null || @this is Ref) return lhs;
                        else
                        {
                            var ass_root = @this.DeepClone();
                            var ref_root = new Ref(new Local(null, ass_root.Type()));
                            var ass = new Assign(ref_root, ass_root);
                            roots.Add(op, ass);

                            var fld = slot as Fld;
                            if (fld != null)
                            {
                                return new Fld(fld.Field, ref_root);
                            }

                            var prop = slot as Prop;
                            if (prop != null)
                            {
                                return new Prop(prop.Property, ref_root, prop.InvokedAsVirtual);
                            }

                            throw AssertionHelper.Fail();
                        }
                    }

                    var eval = lhs as Eval;
                    var m = eval == null ? null : eval.InvokedMethod();
                    if (m != null && m.IsArrayGetter())
                    {
                        var app = eval.Callee;
                        var @this = eval.Callee.Args.First();

                        if (@this == null || @this is Ref) return lhs;
                        else
                        {
                            var ass_root = @this.DeepClone();
                            var ref_root = new Ref(new Local(null, ass_root.Type()));
                            var ass = new Assign(ref_root, ass_root);
                            roots.Add(op, ass);

                            return new Eval(new Apply(new Lambda(m), ref_root.Concat(app.Args.Skip(1))));
                        }
                    }

                    throw AssertionHelper.Fail();
                };

                var safe_lhs = mk_safe_lhs(op.Args.FirstOrDefault());
                var rhs = op.Args.SecondOrDefault() ?? new Const(1); // hack for inc/decrements
                return new Assign(safe_lhs, Operator.Create(opt.Unassign(), safe_lhs, rhs));
            }).AssertCast<Node>();

            out_roots = roots;
            return x_root;
        }
    }
}