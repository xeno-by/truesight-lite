using System;
using System.Collections.Generic;
using System.Linq;
using Truesight.Decompiler.Hir.Core.ControlFlow;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Core.Scopes;
using Truesight.Decompiler.Hir.Core.Symbols;
using Truesight.Decompiler.Pipeline.Hir;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Strings;
using XenoGears.Traits.Hierarchy;
using Truesight.Decompiler.Pipeline.Cil;

namespace Truesight.Decompiler.Hir.Prettyprint
{
    public partial class CSharpPrettyprinter
    {
        private bool IsMultiliner(Node node) { return !IsOneliner(node); }

        protected internal override void TraverseBlock(Block block)
        {
            var prefixes = block.ToDictionary(s => s, s => new List<Action<IndentedTextWriter>>());
            var evalOrder = block.CSharpEvaluationOrder();
            var allRefs = evalOrder.OfType<Ref>().Where(
                @ref => @ref.Sym != null && @ref.Sym.IsLocal()).ToReadOnly();
            var r_scopes = allRefs.ToDictionary(@ref => @ref, @ref => @ref.Scope());
            block.Locals.ForEach(loc =>
            {
                var refs = allRefs.Where(@ref => @ref.Sym == loc).ToReadOnly();
                if (refs.IsEmpty()) return;

                var firstUsage = refs.First();
                var firstAss = firstUsage.Parent.AssertCast<Assign>().AssertNotNull();
                (firstAss.Lhs is Ref && ((Ref)firstAss.Lhs).Sym == loc).AssertTrue();

                if (ReferenceEquals(r_scopes[firstUsage], block))
                {
                    prefixes[firstAss].Add(w => w.Write("var "));
                }
                else
                {
                    var stmt = firstUsage.Hierarchy().SkipWhile(n => n.Parent != block).First();
                    prefixes[stmt].Add(w => w.WriteLine(String.Format("{0} {1};",
                        loc.Type == null ? "?" : loc.Type.GetCSharpRef(ToCSharpOptions.Informative), loc.Name)));
                }
            });

            _writer.WriteLine("{");
            _writer.Indent++;
            block.ForEach(c =>
            {
                prefixes[c].ForEach(action => action(_writer));
                Traverse(c);
                if (IsOneliner(c))
                {
                    if (c is Label) _writer.WriteLine(":");
                    else _writer.WriteLine(";");
                }
            });
            _writer.Indent--;
            _writer.WriteLine("}");
        }

        protected internal override void TraverseCatch(Catch @catch)
        {
            if (@catch.Filter != null)
            {
                _writer.WriteLine("filter");
                Traverse(@catch.Filter);
            }

            String s_exn = null as String;
            if (@catch.ExceptionType == null) s_exn = null;
            else if (@catch.ExceptionType == typeof(Object)) s_exn = null;
            else s_exn = String.Format("({0})", @catch.ExceptionType.GetCSharpRef(ToCSharpOptions.Informative));

            var head = @catch.ExceptionType != null ? "catch" : "fault";
            if (s_exn.IsNeitherNullNorEmpty()) head += ("" + s_exn);
            _writer.WriteLine(head);
            base.TraverseBlock(@catch);
        }

        protected internal override void TraverseFinally(Finally @finally)
        {
            _writer.WriteLine("finally");
            TraverseClause(@finally);
        }

        protected internal override void TraverseTry(Try @try)
        {
            _writer.WriteLine("try");
            Traverse(@try.Body);
            @try.Clauses.ForEach(Traverse);
        }

        protected internal override void TraverseIf(If @if)
        {
            var worthReversing = @if.IfTrue.IsNullOrEmpty() && @if.IfFalse.IsNeitherNullNorEmpty();
            var test = worthReversing ? Operator.Not(@if.Test).SimplifyConditions() : @if.Test;
            var block1 = worthReversing ? @if.IfFalse : @if.IfTrue;
            var block2 = worthReversing ? @if.IfTrue : @if.IfFalse;

            _writer.Write("if (");
            Traverse(test);
            _writer.WriteLine(")");

            Traverse(block1);
            if (block2.IsNeitherNullNorEmpty())
            {
                _writer.WriteLine("else");
                Traverse(block2);
            }
        }

        protected internal override void TraverseLoop(Loop loop)
        {
            Action dumpHeader = () =>
            {
                var initIsNotEmpty = loop.Init.IsNeitherNullNorEmpty();
                var iterIsNotEmpty = loop.Iter.IsNeitherNullNorEmpty(); ;
                if (initIsNotEmpty || iterIsNotEmpty) _writer.Write("for (");
                else _writer.Write("while (");

                if (initIsNotEmpty || iterIsNotEmpty)
                {
                    (loop.Init ?? new Block()).ForEach((c, i) =>
                    {
                        var ass = c as Assign;
                        var lhs = ass == null ? null : ass.Lhs as Ref;
                        if (lhs != null && loop.Locals.Contains(lhs.Sym as Local)) _writer.Write("var ");

                        Traverse(c.AssertCast<Expression>());
                        if (i != loop.Init.Count() - 1) _writer.Write(", ");
                    });

                    _writer.Write(";");
                    if (initIsNotEmpty) _writer.Write(" ");
                }

                Traverse(loop.Test);

                if (initIsNotEmpty || iterIsNotEmpty)
                {
                    _writer.Write(";");

                    if (iterIsNotEmpty) _writer.Write(" ");
                    (loop.Iter ?? new Block()).ForEach((c, i) =>
                    {
                        Traverse(c.AssertCast<Expression>());
                        if (i != loop.Iter.Count() - 1) _writer.Write(", ");
                    });
                }

                _writer.WriteLine(")");
            };

            if (loop.IsWhileDo)
            {
                dumpHeader();
                Traverse(loop.Body);
            }
            else
            {
                _writer.WriteLine("do");
                Traverse(loop.Body);
                dumpHeader();
            }
        }

        protected internal override void TraverseUsing(Using @using)
        {
            _writer.Write("foreach (var {0} in ", @using.Resource);
            Traverse(@using.Init);
            _writer.WriteLine(")");
            Traverse(@using.Body);
        }

        protected internal override void TraverseIter(Iter iter)
        {
            _writer.Write("foreach (var {0} in ", iter.Element);
            Traverse(iter.Seq);
            _writer.WriteLine(")");
            Traverse(iter.Body);
        }
    }
}
