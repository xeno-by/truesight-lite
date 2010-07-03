using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Truesight.Decompiler.Hir.Core.Expressions;
using XenoGears.Assertions;
using XenoGears.Functional;
using XenoGears.Reflection;

namespace Truesight.Decompiler.Hir.Core.Functional
{
    [DebuggerDisplay("{ToDebugString_WithParentInfo(), nq}{\"\", nq}")]
    [DebuggerTypeProxy(typeof(ArgsInfoDebugView))]
    [DebuggerNonUserCode]
    public class ArgsInfo : IEnumerable<Tuple<Expression, ParamInfo>>
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

        private ReadOnlyCollection<Tuple<Expression, ParamInfo>> _data;
        public ArgsInfo(Apply app) { _data = ExtractData(app); }

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

        private String ToDebugString() { return String.Format("Count = {0}", this.Count()); }
        [DebuggerDisplay("{ToString(), nq}{\"\", nq}", Name = "{_name, nq}{\"\", nq}")]
        [DebuggerNonUserCode]
        protected internal class ArgsInfoDebugView
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
                    return names.Zip(_obj.Args, (name, node) => node.CreateDebugProxy(this, name)).ToArray();
                }
            }
        }

        #endregion
    }
}