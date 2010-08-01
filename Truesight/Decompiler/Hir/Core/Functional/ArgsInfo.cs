using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Truesight.Decompiler.Hir.Core.Expressions;
using XenoGears.Assertions;
using XenoGears.Functional;
using XenoGears.Reflection;
using Truesight.Decompiler.Hir.Traversal;
using XenoGears.Reflection.Generics;
using XenoGears.Strings;
using XenoGears.Traits.Dumpable;
using Truesight.Decompiler.Hir.TypeInference;

namespace Truesight.Decompiler.Hir.Core.Functional
{
    [DebuggerDisplay("{ToDebugString(), nq}{\"\", nq}")]
    [DebuggerTypeProxy(typeof(ArgsInfoDebugView))]
    [DebuggerNonUserCode]
    public class ArgsInfo : IEnumerable<Tuple<Expression, ParamInfo>>, IDumpableAsText
    {
        private ReadOnlyCollection<Tuple<Expression, ParamInfo>> ExtractData(Apply app)
        {
            var callee = app.Callee;
            var args = app.Args;

            if (callee is Lambda)
            {
                var lam = callee.AssertCast<Lambda>();
                return args.Zip(lam.Sig.Params).ToReadOnly();
            }
            else if (callee is Prop)
            {
                var prop = ((Prop)callee).Property;
                if (prop != null)
                {
                    var setter = app.Parent is Assign && ((Assign)app.Parent).Lhs == app;
                    var m = setter ? prop.GetSetMethod(true) : prop.GetGetMethod(true);
                    var lam = m.Decompile(app.Domain);

                    var @params = lam.Sig.Params;
                    if (prop.IsInstance()) @params = @params.Skip(1).ToReadOnly();
                    if (setter) @params = @params.SkipLast(1).ToReadOnly();
                    return args.Zip(@params).ToReadOnly();
                }
            }
            else
            {
                // todo. return to this when lambdas decompilation is implemented
                // currently we just use default logic 
                //
                // note that the callee might be whatever you can imagine
                // not only curried Apply, but also e.g. Field of lambda type
                // i.e. when implementing this you must not ignore callees that are different from Apply
            }

            return args.Zip(Seq.Infinite(null as ParamInfo)).ToReadOnly();
        }

        #region Implementation of boilerplate stuff

        private readonly Apply _app; public Apply Apply { get { return _app; } }
        private ReadOnlyCollection<Tuple<Expression, ParamInfo>> _data;
        public ArgsInfo(Apply app) { _app = app; _data = ExtractData(app); }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        public IEnumerator<Tuple<Expression, ParamInfo>> GetEnumerator() { return _data.GetEnumerator(); }

        public ReadOnlyCollection<ParamInfo> Params { get { return _data.Select(t => t.Item2).ToReadOnly(); } }
        public ReadOnlyCollection<ParameterInfo> Metadata { get { return _data.Select(t => t.Item2.Metadata).ToReadOnly(); } }
        public ReadOnlyCollection<Expression> Args { get { return _data.Select(t => t.Item1).ToReadOnly(); } }
        public bool Contains(ParamInfo param) { return Params.Contains(param); }
        public bool Contains(ParameterInfo param) { return Metadata.Contains(param); }
        public bool Contains(Expression arg) { return Args.Contains(arg); }

        public Expression this[ParamInfo param] { get { var entry = _data.SingleOrDefault(t => t.Item2 == param); return entry == null ? null : entry.Item1; } } 
        public Expression this[ParameterInfo param] { get { var entry = _data.SingleOrDefault(t => t.Item2.Metadata == param); return entry == null ? null : entry.Item1; } }
        public ParamInfo this[Expression arg] { get { var entry = _data.SingleOrDefault(t => t.Item1 == arg); return entry == null ? null : entry.Item2; } }

        public sealed override String ToString() { return this.DumpAsText(); }
        private String ToDebugString() { return this.DumpAsText(); }
        void IDumpableAsText.DumpAsText(TextWriter writer)
        {
            var m = Apply.InvokedMethod();
            if (m != null)
            {
                if (Apply.InvokedAsVirtual()) writer.Write("virtual ");
                writer.Write(m.DeclaringType.GetCSharpRef(ToCSharpOptions.Informative) + ".");
                writer.Write(m.Name);
                writer.Write(" :: ");
            }

            var s_params = Params.Select(p =>
            {
                var buf = new StringBuilder();
                buf.Append(p.Type.GetCSharpRef(ToCSharpOptions.Informative));
                if (p.Name.IsNeitherNullNorEmpty()) buf.Append(" " + p.Name);
                return buf.ToString();
            }).StringJoin(" -> ");
            if (s_params.IsEmpty()) s_params = "()";
            writer.Write(s_params);

            var ret = Apply.Type().Ret();
            if (Apply.InvokedAsCtor()) ret = m.DeclaringType;
            writer.Write(" -> " + ret.GetCSharpRef(ToCSharpOptions.Informative));
        }

        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        internal class ArgsInfoDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly ArgsInfo _obj;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly String _name;
            public ArgsInfoDebugView(ArgsInfo obj) : this(obj, null) { }
            public ArgsInfoDebugView(ArgsInfo obj, String name) { _obj = obj; _name = name; }
            public override String ToString() { return _obj == null ? null : _obj.ToDebugString(); }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Object zArgs
            {
                get
                {
                    var names = _obj.Zip((e, pi, i) => pi != null ? pi.Name : ("arg" + i)).ToReadOnly();
                    return names.Zip(_obj.Args, (name, node) => node.CreateDebugProxy(_obj._app.CreateDebugProxy(null), name)).ToArray();
                }
            }
        }

        #endregion
    }
}