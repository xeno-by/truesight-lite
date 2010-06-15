using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using XenoGears.Functional;
using XenoGears.Assertions;
using Truesight.Parser.Api;
using Truesight.Parser.Api.DebugInfo;
using Truesight.Parser.Impl.DebugInfo;
using Truesight.Parser.Impl.PdbReader;
using Truesight.Parser.Impl.Reader;
using XenoGears.Reflection.Simple;

namespace Truesight.Parser.Impl
{
    // todo. if necessary, this model is quite easy to remake in mutable style
    // all offsets/lengths are hidden from the user, same for guarded blocks
    // so it *should* be significantly easier to use than even XenoGears.Reflection.Emit
    // note. keep in mind that despite of muxing some il instructions into a single il op
    // the parses loses none of the info (e.g. see the Cast or Ldc or Branch classes)

    [DebuggerNonUserCode]
    internal class MethodBody : IMethodBody, IEquatable<MethodBody>
    {
        public Module Module { get; private set; }
        public Type Type { get; private set; }
        public MethodBase Method { get; private set; }
        IMethodDebugInfo IMethodBody.DebugInfo { get { return DebugInfo; } }
        public MethodDebugInfo DebugInfo { get; private set; }

        // todo. also read pdb and reveal names for args
        public ReadOnlyCollection<ParameterInfo> Args { get; private set; }
        public ParameterInfo Ret { get; private set; }
        public ReadOnlyCollection<LocalVar> Locals { get; private set; }
        ReadOnlyCollection<ILocalVar> IMethodBody.Locals { get { return Locals == null ? null : Locals.Cast<ILocalVar>().ToReadOnly(); } }

        IMethodBody IPatch.Source { get { return this; } }
        IPatch IPatch.Parent { get { return null; } }

        public ReadOnlyCollection<byte> RawIL { get; private set; }
        public ReadOnlyCollection<ExceptionHandlingClause> RawEHC { get; private set; }

        public ReadOnlyCollection<ILOp> Ops { get; private set; }
        ReadOnlyCollection<IILOp> IPatch.Ops { get { return Ops.Cast<IILOp>().ToReadOnly(); } }
        IEnumerator IEnumerable.GetEnumerator() { return ((IEnumerable<IILOp>)this).GetEnumerator(); }
        IEnumerator<IILOp> IEnumerable<IILOp>.GetEnumerator() { return ((IPatch)this).Ops.GetEnumerator(); }

        public ReadOnlyCollection<Guard> Guards { get; private set; }
        ReadOnlyCollection<IGuard> IPatch.Guards { get { return Guards.Cast<IGuard>().ToReadOnly(); } }

        #region Construction

        public MethodBody(MethodBase method)
            : this(method, false)
        {
        }

        public MethodBody(MethodBase method, bool loadDebugInfo)
        {
            method.AssertNotNull();

            if (method is MethodBuilder)
            {
                Initialize(method.AssertCast<MethodBuilder>(), loadDebugInfo);
            }
            else if (method is ConstructorBuilder)
            {
                Initialize(method.AssertCast<ConstructorBuilder>(), loadDebugInfo);
            }
            else if (method is DynamicMethod)
            {
                // todo. implement reading dynamic IL
                throw AssertionHelper.Fail();
            }
            else if (method is MethodInfo)
            {
                Initialize(method.AssertCast<MethodInfo>(), loadDebugInfo);
            }
            else if (method is ConstructorInfo)
            {
                Initialize(method.AssertCast<ConstructorInfo>(), loadDebugInfo);
            }
            else
            {
                throw AssertionHelper.Fail();
            }
        }

        public MethodBody(System.Reflection.MethodBody body)
            : this(body, false)
        {
            
        }

        public MethodBody(System.Reflection.MethodBody body, bool loadDebugInfo)
            : this(body.AssertNotNull().Get("m_methodBase").AssertCast<MethodBase>(), loadDebugInfo)
        {
        }

        public MethodBody(byte[] rawIL)
            : this(rawIL, null, null)
        {
        }

        public MethodBody(byte[] rawIL, ParserContext ctx)
            : this(rawIL, null, ctx)
        {
        }

        public MethodBody(byte[] rawIL, IEnumerable<ExceptionHandlingClause> rawEhc)
            : this(rawIL, rawEhc, null)
        {
        }

        public MethodBody(byte[] rawIL, IEnumerable<ExceptionHandlingClause> rawEhc, ParserContext ctx)
        {
            Module = ctx == null ? null : ctx.Module;
            Type = ctx == null ? null : ctx.Type;
            Method = ctx == null ? null : ctx.Method;
            ParseIL(rawIL, rawEhc);
        }

        private void Initialize(MethodInfo mi, bool loadDebugInfo)
        {
            Module = mi.Module;
            Type = mi.DeclaringType;
            Method = mi;

            // todo. do we need to dispose the reader?
            var pdb = loadDebugInfo ? mi.GetSymReader() : null;
            if (pdb != null) DebugInfo = new MethodDebugInfo(this, pdb);

            Locals = mi.GetMethodBody().LocalVariables.Select(lv => new LocalVar(this, lv)).ToReadOnly();
            Args = mi.GetParameters().ToReadOnly();
            Ret = mi.ReturnParameter;

            var body = mi.GetMethodBody();
            ParseIL(body.GetILAsByteArray(), body.ExceptionHandlingClauses);
        }

        private void Initialize(ConstructorInfo ci, bool loadDebugInfo)
        {
            Module = ci.Module;
            Type = ci.DeclaringType;
            Method = ci;

            // todo. do we need to dispose the reader?
            var pdb = loadDebugInfo ? ci.GetSymReader() : null;
            if (pdb != null) DebugInfo = new MethodDebugInfo(this, pdb);

            Locals = ci.GetMethodBody().LocalVariables.Select(lv => new LocalVar(this, lv)).ToReadOnly();
            Args = ci.GetParameters().ToReadOnly();
            Ret = null;

            var body = ci.GetMethodBody();
            ParseIL(body.GetILAsByteArray(), body.ExceptionHandlingClauses);
        }

        private void Initialize(MethodBuilder builder, bool loadDebugInfo)
        {
            Module = builder.GetModule();
            Type = builder.DeclaringType;
            Method = builder;

            // todo. also support acquiring debug info here

            // todo. we can retrieve this info after some work
            Locals = null;
            Args = null;
            Ret = null;

            var il = builder.Get("m_ilGenerator").AssertCast<ILGenerator>();
            var m_ILStream = il.Get("m_ILStream").AssertCast<byte[]>();
            var m_length = il.Get("m_length").AssertCast<int>();
            var rawIL = m_ILStream.Take(m_length).ToArray();
            // todo. also extract EHC info from ILGenerator
            ParseIL(rawIL, null);
        }

        private void Initialize(ConstructorBuilder builder, bool loadDebugInfo)
        {
            var methodBuilder = builder.Get("m_methodBuilder").AssertCast<MethodBuilder>();
            Initialize(methodBuilder, loadDebugInfo);
            Method = builder;
        }

        #endregion

        #region Equality members

        public bool Equals(IPatch other)
        {
            return Equals(other as MethodBody);
        }

        public bool Equals(IMethodBody other)
        {
            return Equals(other as MethodBody);
        }

        public bool Equals(MethodBody other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            if (Method != null)
            {
                return Equals(other.Method, Method);
            }
            else
            {
                return Equals(other.Module, Module) &&
                    Equals(other.Type, Type) &&
                    Equals(other.Method, Method) &&
                    Seq.Equal(other.RawIL, RawIL) &&
                    Seq.Equal(other.RawEHC, RawEHC);
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(MethodBody)) return false;
            return Equals((MethodBody)obj);
        }

        public override int GetHashCode()
        {
            if (Method != null)
            {
                return (Method != null ? Method.GetHashCode() : 0);
            }
            else
            {
                return (Module != null ? Module.GetHashCode() : 0) * 397 ^
                    (Type != null ? Type.GetHashCode() : 0) * 397 ^
                    (Method != null ? Method.GetHashCode() : 0) * 397 ^
                    (RawIL != null ? RawIL.GetHashCode() : 0) * 397 ^
                    (RawEHC != null ? RawEHC.GetHashCode() : 0) * 397;
            }
        }

        public static bool operator ==(MethodBody left, MethodBody right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MethodBody left, MethodBody right)
        {
            return !Equals(left, right);
        }

        #endregion

        private void ParseIL(byte[] rawIL, IEnumerable<ExceptionHandlingClause> rawEhc)
        {
            rawIL = rawIL ?? new byte[0];
            rawEhc = rawEhc ?? Enumerable.Empty<ExceptionHandlingClause>();

            // first pass just gets all the instructions from the stream
            // they're still plain and guard exc and fin code is intertwined with regular code
            RawIL = rawIL.ToReadOnly();
            var flat = new ILOpStream(this, rawIL).ToReadOnly();

            // todo. implement this
            // this also includes ToString for guard-related classes and an update of this tostring
            RawEHC = rawEhc.ToReadOnly();
            rawEhc.AssertEmpty();
//            // second pass separates guard handlers and regular code and links every op with
//            // patch - a continuous code snippet that belongs to a hierarchy composed from the plain source
//            var rootPatch = InflatePatch(flat, ehClauses.ToReadOnly());
//            Ops = rootPatch.Ops;
//            Guards = rootPatch.Guards;

            Ops = flat;
            Ops.ForEach(ilop => ilop.Patch = this);
            Guards = Enumerable.Empty<Guard>().ToReadOnly();
        }

        private Patch InflatePatch(
            ReadOnlyCollection<ILOp> flatPatch,
            ReadOnlyCollection<ExceptionHandlingClause> bodyClauses)
        {
            // todo. implement support for filters and faults
            // todo. this doesn't tolerate mixed flags
            // investigate whether this is a logical error and update accordingly
            bodyClauses.ForEach(ehc => (
                ehc.Flags == ExceptionHandlingClauseOptions.Clause ||
                ehc.Flags == ExceptionHandlingClauseOptions.Finally).AssertTrue());

            // todo.
            // when building guards make sure that there are no IL instructions in-between handlers/finallies/filters
            // i.e. when we remove all the guard apparel, there won't be new isolated blocks of code

            // todo.
            // inflation might require fixups for endfinally and endfilter
            // i.e. removing those from the patch and rewriting references to them if any
            // thanks to our architecture we don't need to fixup offsets!

            throw new NotImplementedException();
        }
    }
}