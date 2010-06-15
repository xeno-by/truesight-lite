using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Truesight.Decompiler.Hir.Core.Symbols;
using XenoGears;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Reflection;
using XenoGears.Reflection.Generics;
using XenoGears.Strings;
using XenoGears.Traits.Cloneable;
using XenoGears.Traits.Dumpable;
using XenoGears.Traits.Equivatable;

namespace Truesight.Decompiler.Hir.Core.Functional
{
    [DebuggerNonUserCode]
    public class Sig : IDumpableAsText, ICloneable2, IEquatable<Sig>, IEquivatable<Sig>
    {
        public MethodBase Method { get; private set; }
        private readonly InvocationStyle _invocationStyle = InvocationStyle.NonVirtual;
        public InvocationStyle InvocationStyle { get { return _lambda == null ? _invocationStyle : _lambda.InvocationStyle; } }

        public ReadOnlyCollection<ParamInfo> Params
        {
            get
            {
                if (Method != null)
                {
                    var parc = 0; var @params = new List<ParamInfo>();
                    var shouldIncludeThis = Method.IsInstance();
                    if (Method.IsConstructor && InvocationStyle != InvocationStyle.Ctor) shouldIncludeThis = true;
                    if (shouldIncludeThis) @params.Add(new ParamInfo(this, parc++, "this", Method.DeclaringType));
                    Method.GetParameters().ForEach((p, i) => @params.Add(new ParamInfo(this, parc++, p)));
                    return @params.ToReadOnly();
                }
                else
                {
                    return Syms.Select((p, i) => new ParamInfo(this, i)).ToReadOnly();
                }
            }
        }

        private ReadOnlyCollection<Param> _syms;
        public ReadOnlyCollection<Param> Syms
        {
            get
            {
                if (_lambda != null)
                {
                    _lambda.EnsureSigAndBody();
                }

                return _syms;
            }

            // todo. that's really poor implementation. how can we improve it?
            // we need to implement the following functionality:
            // 1) make Lambda's Body lazy so that we decompile stuff only when we need it
            // 2) make Lambda's Sig to have symbols that are in sync with lazy Body
            // 3) make both Lambda and Sig usable in disconnection with real MethodInfos
            internal set
            {
                _syms = value;

                // todo. before uncommenting this think twice
                // about how this will work with linked Method and with type inference cache
                // regarding the latter check the comments in Sym constructor (if there will remain any)
                if (_syms != null)
                {
                    _syms.ForEach(sym => sym.FreezeForever());
                }
            }
        }

        private Type _ret;
        public Type Ret
        {
            get
            {
                if (Method != null)
                {
                    var t_ret = Method.Ret();
                    if (_invocationStyle == InvocationStyle.Ctor) t_ret = Method.DeclaringType;
                    return t_ret;
                }
                else
                {
                    return _ret;
                }
            }

            private set
            {
                _ret = value;
            }
        }

        private readonly Lambda _lambda;
        internal Sig(Lambda lambda) 
            : this(lambda.Method.AssertNotNull(), lambda.InvocationStyle)
        {
            _lambda = lambda;
        }

        public Sig(MethodBase method)
            : this(method, method.IsConstructor() ? InvocationStyle.Ctor : InvocationStyle.NonVirtual)
        {
        }

        public Sig(MethodBase method, InvocationStyle invocationStyle)
        {
            Method = method;
            _invocationStyle = invocationStyle;

            var parc = 0; var @params = new List<ParamInfo>();
            if (method.IsInstance() || method.IsConstructor) @params.Add(new ParamInfo(this, parc++, "this", method.DeclaringType));
            method.GetParameters().ForEach((p, i) => @params.Add(new ParamInfo(this, parc++, p)));
            Syms = @params.Select((p, i) => new Param(p.Name, p.Type)).ToReadOnly();
        }

        internal Sig(IEnumerable<Type> @params, Type ret)
            : this(@params.Concat(ret).ToArray())
        {
        }

        internal Sig(params Type[] paramsAndRet)
            : this(paramsAndRet.SkipLast(1).Select((t, i) => new Param("$p" + i, t)), paramsAndRet.Last())
        {
        }

        internal Sig(IEnumerable<Param> @params, Type ret)
        {
            Syms = @params.ToReadOnly();
            Ret = ret;
        }

        public sealed override String ToString() { return this.DumpAsText(); }
        void IDumpableAsText.DumpAsText(TextWriter writer)
        {
            if (Method != null)
            {
                if (InvocationStyle == InvocationStyle.Virtual) writer.Write("virtual ");
                writer.Write(Method.DeclaringType.GetCSharpRef(ToCSharpOptions.Informative) + ".");
                writer.Write(Method.Name);
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

            var ret = Ret;
            if (InvocationStyle == InvocationStyle.Ctor) ret = Method.DeclaringType;
            writer.Write(" -> " + ret.GetCSharpRef(ToCSharpOptions.Informative));
        }

        private readonly Guid _uniqueId = Guid.NewGuid();
        public Guid UniqueId { get { return _uniqueId; } }
        private Guid _protoId = Guid.NewGuid();
        Guid ICloneable2.ProtoId { get { return _protoId; } }
        T ICloneable2.ShallowClone<T>() { throw AssertionHelper.Fail(); }
        public Sig DeepClone() { return ((ICloneable2)this).DeepClone<Sig>(); }
        T ICloneable2.DeepClone<T>()
        {
            var clone = Method != null ? new Sig(Method, InvocationStyle) : 
                new Sig(Syms.Select(p => p.DeepClone()), Ret);
            clone._protoId = _protoId;
            return (T)(Object)clone;
        }

        public bool Equals(Sig other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(this.InvocationStyle, other.InvocationStyle) &&
                Equals(this.Ret, other.Ret) && this.Syms.AllMatch(other.Syms, Equals);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Sig)) return false;
            return Equals((Sig)obj);
        }

        public override int GetHashCode()
        {
            if (_lambda != null) return _lambda.GetHashCode();
            return Syms.Fold(InvocationStyle.SafeHashCode() ^ Ret.SafeHashCode(), (h, p) => h ^ p.SafeHashCode());
        }

        public static bool operator ==(Sig left, Sig right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Sig left, Sig right)
        {
            return !Equals(left, right);
        }

        public bool Equiv(Sig other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(this.InvocationStyle, other.InvocationStyle) &&
                Equals(this.Ret, other.Ret) && this.Params.AllMatch(other.Params, Equals);
        }

        public int EquivHashCode()
        {
            return Params.Fold(InvocationStyle.SafeHashCode() ^ Ret.SafeHashCode(), (h, p) => h ^ p.SafeHashCode());
        }
    }
}