using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using XenoGears.Strings;
using XenoGears.Assertions;
using XenoGears.Reflection.Generics;

namespace Truesight.Parser.Api.Ops
{
    // note. see comments to ILOp::MemberFromToken
    // exactly the same applies here, so be sure to read the comments

    [DebuggerNonUserCode]
    public class RuntimeHandle
    {
        public Module Module { get; private set; }
        public Type Type { get; private set; }
        public MethodBase Method { get; private set; }
        public int MetadataToken { get; private set; }

        public RuntimeHandle(Module module, Type type, MethodBase method, int metadataToken)
        {
            Module = module;
            Type = type;
            Method = method;
            MetadataToken = metadataToken;
        }

        public Object ResolveHandle()
        {
            if (Module == null) return null;

            var module = Module.ModuleHandle;
            var t_handles = Type == null ? null : Type.XGetGenericArguments().Select(a => a.TypeHandle).ToArray();
            var m_handles = Method == null ? null : Method.XGetGenericArguments().Select(a => a.TypeHandle).ToArray();

            // todo. a robust approach to this problem would be
            // to analyze the highest byte of the token according to ECMA-335 spec
            try { return module.ResolveTypeHandle(MetadataToken, t_handles, m_handles); }
            catch
            {
                try { return module.ResolveMethodHandle(MetadataToken, t_handles, m_handles); }
                catch
                {
                    try { return module.ResolveFieldHandle(MetadataToken, t_handles, m_handles); }
                    catch
                    {
                        return null;
                    }
                }
            }
        }

        public Object ResolveMember()
        {
            var o_handle = ResolveHandle();
            if (o_handle == null) return null;

            if (o_handle is RuntimeTypeHandle)
            {
                var handle = (RuntimeTypeHandle)o_handle;
                return Type.GetTypeFromHandle(handle);
            }
            else if (o_handle is RuntimeMethodHandle)
            {
                var handle = (RuntimeMethodHandle)o_handle;
                return MethodBase.GetMethodFromHandle(handle);
            }
            else if (o_handle is RuntimeFieldHandle)
            {
                var handle = (RuntimeFieldHandle)o_handle;
                return FieldInfo.GetFieldFromHandle(handle);
            }
            else
            {
                throw AssertionHelper.Fail();
            }
        }

        public override String ToString()
        {
            try
            {
                // todo. verify that memberinfo covers all necessary cases
                // affirmed: http://msdn.microsoft.com/en-us/library/system.reflection.emit.opcodes.ldtoken.aspx
                var member = ResolveMember().AssertCast<MemberInfo>();
                return member.GetCSharpRef(ToCSharpOptions.Informative);
            }
            catch
            {
                return "runtime handle 0x" + MetadataToken.ToString("x8");
            }
        }
    }
}
