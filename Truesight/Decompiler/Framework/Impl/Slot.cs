using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using XenoGears.Assertions;
using XenoGears.Strings;
using XenoGears.Reflection.Emit;

namespace Truesight.Decompiler.Framework.Impl
{
    [DebuggerNonUserCode]
    internal class Slot
    {
        private readonly FieldInfo _fi;
        private readonly PropertyInfo _pi;
        public Slot(FieldInfo fi) { _fi = fi; }
        public Slot(PropertyInfo pi) { _pi = pi; }

        public String Name
        {
            get
            {
                (_fi == null ^ _pi == null).AssertTrue();
                return _fi != null ? _fi.Name : _pi.Name;
            }
        }

        public Type Type
        {
            get
            {
                (_fi == null ^ _pi == null).AssertTrue();
                return _fi != null ? _fi.FieldType : _pi.PropertyType;
            }
        }

        public Object GetValue(Object target)
        {
            (_fi == null ^ _pi == null).AssertTrue();
            return _fi != null ? _fi.GetValue(target) : _pi.GetValue(target, new Object[0]);
        }

        public void EmitGetValue(ILGenerator il)
        {
            (_fi == null ^ _pi == null).AssertTrue();
            if (_fi != null) il.ldfld(_fi);
            else il.call(_pi.GetGetMethod(true));
        }

        public void SetValue(Object target, Object value)
        {
            (_fi == null ^ _pi == null).AssertTrue();
            if (_fi != null) _fi.SetValue(target, value);
            else _pi.SetValue(target, value, new Object[0]);
        }

        public void EmitSetValue(ILGenerator il)
        {
            (_fi == null ^ _pi == null).AssertTrue();
            if (_fi != null) il.stfld(_fi);
            else il.call(_pi.GetSetMethod(true));
        }

        public override String ToString()
        {
            return String.Format("{0} {1}", Type.GetCSharpRef(ToCSharpOptions.Informative), Name);
        }
    }
}
