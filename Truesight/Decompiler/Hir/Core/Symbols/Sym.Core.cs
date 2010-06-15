using System;
using System.Diagnostics;
using System.IO;
using XenoGears.Assertions;
using XenoGears.Traits.Cloneable;
using XenoGears.Traits.Dumpable;
using XenoGears.Traits.Freezable;

namespace Truesight.Decompiler.Hir.Core.Symbols
{
    [DebuggerNonUserCode]
    public abstract partial class Sym : Freezable, IDumpableAsText, ICloneable2, IEquatable<Sym>
    {
        protected abstract String DumpImpl();
        public Sym DeepClone() { return ((ICloneable2)this).DeepClone<Sym>(); }

        private readonly Guid _uniqueId = Guid.NewGuid();
        public Guid UniqueId { get { return _uniqueId; } }
        public Guid ProtoId { get { return _protoId; } }
        private String _name; public String Name { get { return _name; } set { IsFrozen.AssertFalse(); _name = value; } }
        private Type _type; public Type Type { get { return _type; } set { IsFrozen.AssertFalse(); _type = value; } }

        protected Sym(String name, Type type)
        {
            Name = name;
            Type = type;

            // todo. before uncommenting this consider the following point:
            // to improve performance Domain caches types of HIR nodes
            // certainly, cache gets flushed whenever something gets changed
            //
            // however, currently I don't track changes to locals' Type property
            // that's why I'm disabling these changes for the moment
            //
            // so, to make Syms (i.e. Locals and Params) mutable you need to:
            // 1) implement INPC* interfaces in the same fashion Node does
            // 2) within all Scope implementors listen to changes of Local types
            // 3) within Lambdas listen to changes of Param types
            // 4) don't forget to unbind from INPC* events when Sym gets deleted
            FreezeForever();
        }

        public sealed override String ToString() { return this.DumpAsText(); }
        void IDumpableAsText.DumpAsText(TextWriter writer) { writer.Write(DumpImpl()); }

        private Guid _protoId = Guid.NewGuid();
        Guid ICloneable2.ProtoId { get { return _protoId; } }
        T ICloneable2.ShallowClone<T>() { throw AssertionHelper.Fail(); }
        T ICloneable2.DeepClone<T>()
        {
            Sym clone;
            if (this is Param)
            {
                clone = new Param(Name, Type);
            }
            else if (this is Local)
            {
                clone = new Local(Name, Type);
            }
            else
            {
                throw AssertionHelper.Fail();
            }

            clone._protoId = _protoId;
            return clone.AssertCast<T>();
        }

        public bool Equals(Sym other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._name, _name) && Equals(other._type, _type) && other._protoId.Equals(_protoId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (this.GetType() != obj.GetType()) return false;
            return Equals((Sym)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (_name != null ? _name.GetHashCode() : 0);
                result = (result * 397) ^ (_type != null ? _type.GetHashCode() : 0);
                result = (result * 397) ^ _protoId.GetHashCode();
                return result;
            }
        }

        public static bool operator ==(Sym left, Sym right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Sym left, Sym right)
        {
            return !Equals(left, right);
        }
    }
}