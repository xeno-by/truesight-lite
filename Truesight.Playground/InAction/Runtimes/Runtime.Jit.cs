using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Truesight.Playground.InAction.Domain;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Reflection;
using XenoGears.Reflection.Generics;
using XenoGears.Reflection.Shortcuts;
using XenoGears.Reflection.Emit;
using XenoGears.Reflection.Emit.Hackarounds;

namespace Truesight.Playground.InAction.Runtimes
{
    [DebuggerNonUserCode]
    internal partial class Runtime<T1, T2, T3>
    {
        private IKernel<T1, T2, T3> Jit(Type t_kernel)
        {
            var unit_name = "Truesight.Playground.InAction.Runtime";
            var unit = XenoGears.Reflection.Emit2.Codegen.Units[unit_name];
            return unit.Context.GetOrCreate(t_kernel, () =>
            {
                // Constructor
                var name = String.Format("{0}_{1}", t_kernel.FullName, this.GetType().Name);
                var t = unit.Module.DefineType(name, TA.Public, t_kernel);
                var f_runtime = t.DefineField("_runtime", typeof(Runtime<T1, T2, T3>), FA.Private);
                var ctor = t.DefineConstructor(MA.PublicCtor, CC.Std, typeof(Runtime<T1, T2, T3>).MkArray());
                ctor.DefineParameter(1, ParmA.None, "runtime");
                ctor.il().ldarg(0).ldarg(1).stfld(f_runtime).ret();

                // Redirect implementations of kernel APIs to the runtime
                var ifacesToImpls = new Dictionary<MethodBase, MethodBase>();
                ifacesToImpls.AddElements(t_kernel.GetInterfaceMap(typeof(IGridApi)).MapInterfaceToImpl());
                ifacesToImpls.AddElements(t_kernel.GetInterfaceMap(typeof(ISyncApi)).MapInterfaceToImpl());
                ifacesToImpls.ForEach(kvp =>
                {
                    var apiDecl = kvp.Key;
                    var baseImpl = kvp.Value;

                    var impl = t.DefineOverride(baseImpl);
                    baseImpl.GetParameters().ForEach((pi, i) => impl.DefineParameter(i + 1, ParmA.None, pi.Name));
                    impl.il()
                        .ldarg(0)
                        .ldfld(f_runtime)
                        .ld_args(1, apiDecl.Paramc())
                        .callvirt(apiDecl)
                        .ret();
                });

                // Redirect implementation of RunKernel to the runtime
                var orig_runkernel = t_kernel.GetMethod("RunKernel", BF.All);
                var m_runkernel = t.OverrideMethod(orig_runkernel);
                var m_rungrid = typeof(IGridRunner<T1, T2, T3>).GetMethod("RunGrid");
                m_runkernel.il()
                    .ldarg(0)
                    .ldfld(f_runtime)
                    .ldarg(1)
                    .ldarg(2)
                    .ldarg(3)
                    .call(m_rungrid)
                    .ret();

                // Invoke custom compilation logic if necessary
                Crosscompiler<T1, T2, T3>.DoCrosscompile(t_kernel, t);

                // false is necessary to ensure that ref.emit won't destroy our symbols
                return t.CreateType(false);
            }).AssertCast<Type>().CreateInstance(this).AssertCast<IKernel<T1, T2, T3>>();
        }
    }
}