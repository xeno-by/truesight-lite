using System;
using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework;
using XenoGears.Assertions;
using XenoGears.Reflection.Emit;
using XenoGears.Reflection.Shortcuts;
using XenoGears.Reflection.Simple;

namespace Truesight.Playground.Parser
{
    [TestFixture]
    public class DotNetFrameworkTests
    {
        [Test]
        public void GetBodyForMethod()
        {
            var mb = MethodBase.GetCurrentMethod();
            var body = mb.GetMethodBody();

            body.ExceptionHandlingClauses.AssertEmpty();
            body.LocalVariables.AssertCount(3);
            var ilBytes = body.GetILAsByteArray();
            ilBytes.AssertNotNull();
        }

        [Test]
        public void GetBodyForDynamicMethod()
        {
            var dyna = new DynamicMethod("DynamicMethod", typeof(void), Type.EmptyTypes);
            dyna.il().ret();
            var compiled = dyna.CreateDelegate(typeof(Action));

            var method = compiled.Method;
            var owner = method.Get("m_owner").AssertCast<DynamicMethod>();
            var ilgen = owner.GetILGenerator();
            var ilBytes = ilgen.Get("m_ILStream").AssertCast<byte[]>();
            ilBytes.AssertNotNull();
        }

        [Test]
        public void GetBodyForMethodBuilder()
        {
            var asm = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("DynamicAssembly"), AssemblyBuilderAccess.Run);
            var mod = asm.DefineDynamicModule(asm.GetName().Name + ".dll");
            var d_t = mod.DefineType("DynamicType");
            var d_meth = d_t.DefineMethod("DynamicMethod", MA.Public, typeof(void), Type.EmptyTypes);
            d_meth.il().ret();

            var ilgen = d_meth.GetILGenerator();
            var ilBytes1 = ilgen.Get("m_ILStream").AssertCast<byte[]>();
            ilBytes1.AssertNotNull();

            var t = d_t.CreateType();
            var meth = t.GetMethod("DynamicMethod");

            var body = meth.GetMethodBody();
            body.ExceptionHandlingClauses.AssertEmpty();
            body.LocalVariables.AssertCount(0);
            var ilBytes2 = body.GetILAsByteArray();
            ilBytes2.AssertNotNull();
        }
    }
}
