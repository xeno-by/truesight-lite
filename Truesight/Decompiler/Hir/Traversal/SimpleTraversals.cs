using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Truesight.Decompiler.Hir.Core.ControlFlow;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Core.Functional;
using XenoGears.Functional;
using XenoGears.Traits.Hierarchy;
using XenoGears.Assertions;
using XenoGears.Reflection;
using Truesight.Decompiler.Hir.Traversal;

namespace Truesight.Decompiler.Hir.Traversal
{
    [DebuggerNonUserCode]
    public static class SimpleTraversals
    {
        public static ReadOnlyCollection<Node> Parents(this Node node)
        {
            if (node == null) return Seq.Empty<Node>().ToReadOnly();
            return ((Hierarchy<Node>)node).Parents();
        }

        public static ReadOnlyCollection<Node> Hierarchy(this Node node)
        {
            if (node == null) return Seq.Empty<Node>().ToReadOnly();
            return ((Hierarchy<Node>)node).Hierarchy();
        }

        public static ReadOnlyCollection<Node> ChildrenRecursive(this Node node)
        {
            if (node == null) return Seq.Empty<Node>().ToReadOnly();
            return ((Hierarchy<Node>)node).ChildrenRecursive();
        }

        public static ReadOnlyCollection<Node> Family(this Node node)
        {
            if (node == null) return Seq.Empty<Node>().ToReadOnly();
            return ((Hierarchy<Node>)node).Family();
        }

        public static Node Stmt(this Node n)
        {
            if (n == null) return null;
            var e = n.Hierarchy().TakeWhile(p => p is Expression).LastOrDefault().AssertCast<Expression>();

            if (e == null) return null;
            var take_parent = e.Parent is Throw || e.Parent is Return;
            return take_parent ? (Node)e.Parent : e;
        }

        public static Lambda InvokedLambda(this Node n)
        {
            var ci = n as CollectionInit;
            if (ci != null) return ci.Ctor.InvokedLambda();

            var oi = n as ObjectInit;
            if (oi != null) return oi.Ctor.InvokedLambda();

            if (n == null) return null;
            if (n is Apply)
            {
                var app = n.AssertCast<Apply>();
                return app == null ? null : app.Callee as Lambda;
            }
            else if (n is Eval)
            {
                var eval = n.AssertCast<Eval>();
                return eval.Callee.InvokedLambda();
            }
            else
            {
                return null;
            }
        }

        public static MethodBase InvokedMethod(this Node n)
        {
            var ci = n as CollectionInit;
            if (ci != null) return ci.Ctor.InvokedMethod();

            var oi = n as ObjectInit;
            if (oi != null) return oi.Ctor.InvokedMethod();

            var lam = n.InvokedLambda();
            return lam == null ? null : lam.Method;
        }

        public static ConstructorInfo InvokedCtor(this Node n)
        {
            var ci = n as CollectionInit;
            if (ci != null) return ci.Ctor.InvokedCtor();

            var oi = n as ObjectInit;
            if (oi != null) return oi.Ctor.InvokedCtor();

            var lam = n.InvokedLambda();
            if (lam == null) return null;
            else
            {
                var ctor = n.InvokedMethod() as ConstructorInfo;
                return lam.InvokedAsCtor ? ctor : null;
            }
        }

        public static IList<Expression> InvocationArgs(this Node n)
        {
            var ci = n as CollectionInit;
            if (ci != null) return ci.Ctor.InvocationArgs();

            var oi = n as ObjectInit;
            if (oi != null) return oi.Ctor.InvocationArgs();

            if (n == null) return null;
            if (n is Apply)
            {
                var app = n.AssertCast<Apply>();
                return app.Args;
            }
            else if (n is Eval)
            {
                var eval = n.AssertCast<Eval>();
                return eval.Callee.InvocationArgs();
            }
            else
            {
                return null;
            }
        }

        public static Prop InvokedProp(this Node n)
        {
            if (n == null) return null;
            if (n is Assign)
            {
                var ass = (Assign)n;
                return ass.Lhs.InvokedProp();
            }
            else if (n is Prop)
            {
                var prop = (Prop)n;
                return prop;
            }
            else if (n is Apply)
            {
                var app = n.AssertCast<Apply>();
                var prop = app == null ? null : app.Callee as Prop;
                return prop;
            }
            else
            {
                return null;
            }
        }

        public static PropertyInfo InvokedProperty(this Node n)
        {
            var prop = n.InvokedProp();
            if (prop != null) return prop.Property;

            var m = n.InvokedMethod();
            var p_m = m.EnclosingProperty();
            if (p_m != null) return p_m;

            return null;
        }

        public static IList<Expression> InvocationIndexers(this Node n)
        {
            var prop = n.InvokedProp();
            if (prop == null) return null;

            var app = prop.Parent as Apply;
            if (app == null) return null;

            return app.Args;
        }

        public static Prop ReadProp(this Node n)
        {
            var prop = n.InvokedProp();
            var is_read = prop != null && !(prop.Parent is Assign);
            return is_read ? prop : null;
        }

        public static PropertyInfo ReadProperty(this Node n)
        {
            var prop = n.ReadProp();
            if (prop != null) return prop.Property;

            var m = n.InvokedMethod();
            var p_m = m.EnclosingProperty();
            if (m.IsGetter()) return p_m;

            return null;
        }

        public static Prop WrittenProp(this Node n)
        {
            var prop = n.InvokedProp();
            var is_written = prop != null && prop.Parent is Assign;
            return is_written ? prop : null;
        }

        public static PropertyInfo WrittenProperty(this Node n)
        {
            var prop = n.WrittenProp();
            if (prop != null) return prop.Property;

            var m = n.InvokedMethod();
            var p_m = m.EnclosingProperty();
            if (m.IsSetter()) return p_m;

            return null;
        }
    }
}