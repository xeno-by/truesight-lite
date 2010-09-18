using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using XenoGears;
using XenoGears.Assertions;
using XenoGears.Functional;
using Truesight.Decompiler.Framework.Annotations;
using XenoGears.Reflection;
using XenoGears.Reflection.Attributes;
using XenoGears.Reflection.Emit;
using XenoGears.Reflection.Generics;
using XenoGears.Reflection.Shortcuts;
using XenoGears.Traits.Dumpable;

namespace Truesight.Decompiler.Framework.Impl
{
    [DebuggerNonUserCode]
    internal static class Codegen
    {
        private static Dictionary<Tuple<PipelineAttribute, Type>, Delegate> _cache =
            new Dictionary<Tuple<PipelineAttribute, Type>, Delegate>();

        public static Func<T, T> CompilePipeline<T>(this Pipeline pipeline)
        {
            var uncurried = (Func<Pipeline, T, T>)_cache.GetOrDefault(Tuple.Create(pipeline.Metadata, typeof(T)), () =>
            {
                var asm = XenoGears.Reflection.Emit2.Codegen.Units["Truesight.Decompiler.Pipeline"];
//                var typeName = "\"" + pipeline.Metadata.Name + "\"";
                var typeName = pipeline.Metadata.Name;

                var t_generated = asm.Module.GetType(typeName);
                if (t_generated == null)
                {
                    var t = asm.Module.DefineType(typeName);
                    GenerateProcessMethod<T>(pipeline, t);
                    t_generated = t.CreateType();
                }

                var m_process_created = t_generated.GetMethod("Process", BF.All);
                var func_pipeline_t_t = typeof(Func<,,>).XMakeGenericType(typeof(Pipeline), typeof(T), typeof(T));
                return (Func<Pipeline, T, T>)Delegate.CreateDelegate(func_pipeline_t_t, m_process_created);
            });

            return ctx => uncurried(pipeline, ctx);
        }

        private static MethodBuilder GenerateProcessMethod<T>(this Pipeline pipeline, TypeBuilder t)
        {
            var m_dump = GenerateDumpMethod(pipeline, t);
            var m = t.DefineMethod("Process", MA.PublicStatic, typeof(T), new[] { typeof(Pipeline), typeof(T) });
            m.DefineParameter(1, ParmA.None, "pipeline");
            m.DefineParameter(2, ParmA.None, "ctx");

            var instances = m.il().DeclareLocal(typeof(Dictionary<Type, Object>));
            foreach (var step in pipeline.Steps)
            {
                // pre-bind input and output arguments 
                // since we need them both for logs and for the call itself
                var boundArgs = step.Code.GetParameters().ToDictionary(pi => pi,
                    pi => pi.ParameterType == typeof(T) ? null : pi.BindRead(typeof(T)));
                var boundRet = step.Code.ReturnParameter.BindWrite(typeof(T));

                // prepare logging infrastructure
                var step1 = step; // so that Resharper doesn't whine
                Action<bool, Slot> emitLogger = (beforeOrAfter, slot) =>
                {
                    var parentsAllowLogging =
                        step1.PlantMetadata.LogEnabled &&
                        step1.WorkshopMetadata.LogEnabled;
                    var cfgAllowsLogging = beforeOrAfter ?
                        step1.StepMetadata.LogBefore :
                        step1.StepMetadata.LogAfter;
                    var slotIsDumpable = slot == null ?
                        typeof(IDumpableAsText).IsAssignableFrom(typeof(T)) :
                        typeof(IDumpableAsText).IsAssignableFrom(slot.Type);

                    if (parentsAllowLogging && cfgAllowsLogging && slotIsDumpable)
                    {
                        Action<ILGenerator> emitNameOfMoment = il =>
                        {
                            var fileNameTemplate = beforeOrAfter ? "Before" : "After";
                            fileNameTemplate += " " + (step1.StepMetadata.Name.IsNeitherNullNorEmpty() ?
                                step1.StepMetadata.Name :
                                (step1.Code.Name.StartsWith("Do") ? step1.Code.Name.Substring(2) : step1.Code.Name));
                            if (slot != null) fileNameTemplate += " - " + slot.Name;
                            m.il().ldstr(fileNameTemplate);
                        };

                        Action<ILGenerator> emitSubject = il =>
                        {
                            m.il().ldarg(1);
                            if (slot != null) slot.EmitGetValue(m.il());
                        };

                        emitSubject(m.il());
                        m.il().ldarg(0).call(typeof(Pipeline).GetProperty("Name").GetGetMethod());
                        emitNameOfMoment(m.il());
                        var targ = slot == null ? typeof(T) : slot.Type;
                        m.il().call(m_dump.XMakeGenericMethod(targ));
                    }
                };

                // perform log before if necessary
                step1.Code.GetParameters().ForEach(pi => emitLogger(true, boundArgs[pi]));

                // prepare to use the return value if necessary
                if (boundRet != null) m.il().ldarg(1);

                // load call target onto the stack
                if (!m.IsStatic)
                {
                    // todo. correctly handle situation of inherited instance method
                    var ctor = m.DeclaringType.GetConstructor(Type.EmptyTypes).AssertNotNull();
                    var m_getOrDefault = typeof(EnumerableExtensions).GetMethods(BF.All).Single(m1 =>
                        m1.Name == "GetOrDefault" &&
                        m1.Params().Count() == 3 &&
                        m1.Params().First().SameMetadataToken(typeof(IDictionary<,>)) &&
                        m1.Params().Second() == m1.Params().First().XGetGenericArguments()[0] &&
                        m1.Params().Third() == m1.Params().First().XGetGenericArguments()[1]);
                    m_getOrDefault = m_getOrDefault.XMakeGenericMethod(typeof(Type), typeof(Object));
                    m.il().ldloc(instances).ld_type_info(m.DeclaringType).newobj(ctor).call(m_getOrDefault);
                }

                // load all args onto the stack
                step.Code.GetParameters().ForEach(pi =>
                {
                    m.il().ldarg(1);
                    if (pi.ParameterType == typeof(T))
                    {
                        // do nothing -> bind to the entire context
                    }
                    else
                    {
                        var argSlot = pi.BindRead(typeof(T)).AssertNotNull();
                        argSlot.EmitGetValue(m.il());
                    }
                });

                // lololo, I've forgotten this step when first implementing this logic
                m.il().call(step.Code);

                // use the return value if necessary
                if (boundRet != null) boundRet.EmitSetValue(m.il());
                if (step.Code.ReturnType != typeof(void) && boundRet == null) m.il().pop();

                // perform log after if necessary
                // note. here we also log parameters since they might've been changed
                step1.Code.GetParameters().ForEach(pi => emitLogger(false, boundArgs[pi]));
                if (boundRet != null) emitLogger(false, boundRet);
            }

            m.il().ldarg(1).ret();


            return m;
        }

        private static MethodBuilder GenerateDumpMethod(this Pipeline pipeline, TypeBuilder t)
        {
            var m = t.DefineMethod("DumpAsText", MA.PrivateStatic);
            var gargs = m.DefineGenericParameters("T");
            gargs[0].SetInterfaceConstraints(typeof(IDumpableAsText));
            m.SetReturnType(typeof(void));
            m.SetParameters(gargs[0], typeof(String), typeof(String));
            m.DefineParameter(1, ParmA.None, "subject");
            m.DefineParameter(2, ParmA.None, "nameOfPipeline");
            m.DefineParameter(3, ParmA.None, "nameOfMoment");

            LocalBuilder fileDir, fileName, fileExt, fullPath;
            m.il()
             .def_local(typeof(String), out fileDir)
             .def_local(typeof(String), out fileName)
             .def_local(typeof(String), out fileExt)
             .def_local(typeof(String), out fullPath);
            // todo. find out why this doesn't work
            fileDir.SetLocalSymInfo("fileDir");
            fileDir.SetLocalSymInfo("fileName");
            fileDir.SetLocalSymInfo("fileExt");
            fileDir.SetLocalSymInfo("fullPath");

            // path to directory
            var path = ".\\" + pipeline.Metadata.Name + "\\";
            m.il().ldstr(path).stloc(fileDir);

            // part of filename that's bound to pipeline's name
            var parts = new List<Action>();
            parts.Add(() =>
            {
                Label ifTrue2, after2;
                m.il().ldarg(1)
                  .call(typeof(String).GetMethod("IsNullOrEmpty"))
                  .def_label(out ifTrue2)
                  .brtrue(ifTrue2)
                    // pipeline.Name is not empty
                  .ldarg(1)
                  .def_label(out after2)
                  .br(after2)
                  .label(ifTrue2)
                    // pipeline.Name is empty
                  .call(typeof(Guid).GetMethod("NewGuid"))
                  .box(typeof(Guid))
                  .callvirt(typeof(Object).GetMethod("ToString"))
                  .label(after2);
            });
            parts.Add(() => m.il().ldstr(" - "));

            // part of filename that's bound to static data: beforeOrAfter and slot
            parts.Add(() => m.il().ldarg(2));

            // part of filename that's bound to current unit test
            parts.Add(() =>
            {
                Label ifTrue2, after2;
                m.il().call(typeof(UnitTest).GetProperty("PersistentId").GetGetMethod())
                  .call(typeof(String).GetMethod("IsNullOrEmpty"))
                  .def_label(out ifTrue2)
                  .brtrue(ifTrue2)
                    // UnitTest.PersistentId is not empty
                  .ldstr(", ")
                  .call(typeof(UnitTest).GetProperty("PersistentId").GetGetMethod())
                  .call(typeof(String).GetMethod("Concat", new[] { typeof(String), typeof(String) }))
                  .def_label(out after2)
                  .br(after2)
                  .label(ifTrue2)
                    // UnitTest.PersistentId is empty
                  .ldnull()
                  .label(after2);
            });

            // concat all parts of filename
            parts.First()();
            parts.Skip(1).ForEach(part =>
            {
                part();
                m.il().call(typeof(String).GetMethod("Concat", new []{typeof(Object), typeof(Object)}));
            });
            m.il().stloc(fileName);

            // extension of the file
            var m_attrOrNull = typeof(AttributeHelper).GetMethods(BF.All).Single(m1 =>
                m1.Name == "AttrOrNull" &&
                m1.XGetGenericArguments().Count() == 1 &&
                m1.Ret() == m1.XGetGenericArguments().Single() &&
                m1.Params().Count() == 1 &&
                m1.Params().Single() == typeof(ICustomAttributeProvider));
            m_attrOrNull = m_attrOrNull.XMakeGenericMethod(typeof(DumpFormatAttribute));

            Label ifFalse, ifFalseFalse, after;
            m.il().ldarg(0)
              .def_label(out ifFalse)
              .brfalse(ifFalse)
              .def_label(out after)
              // subject is not null
              .ldarg(0)
              .callvirt(typeof(Object).GetMethod("GetType"))
              .call(m_attrOrNull)
              .def_label(out ifFalseFalse)
              .brfalse(ifFalseFalse)
              // subject.GetType().AttrOrNull<DumpFormatAttribute>() is not null
              .ldarg(0)
              .callvirt(typeof(Object).GetMethod("GetType"))
              .call(m_attrOrNull)
              .call(typeof(DumpFormatAttribute).GetProperty("DefaultExtension").GetGetMethod())
              .br(after)
              // subject.GetType().AttrOrNull<DumpFormatAttribute>() is null
              .label(ifFalseFalse)
              .ldnull()
              .br(after)
                // subject is null
              .label(ifFalse)
              .ldnull()
              .label(after)
              .stloc(fileExt);

            // compose fullpath
            m.il()
             .ldloc(fileDir)
             .ldloc(fileName)
             .ldstr(".")
             .ldloc(fileExt)
             .call(typeof(String).GetMethod("Concat", new[] { typeof(Object), typeof(Object), typeof(Object), typeof(Object) }))
             .stloc(fullPath);

            Label exists;
            var dump = typeof(IDumpableAsTextTrait).GetMethods(BF.All).Single(m1 =>
                m1.Name == "DumpAsText" &&
                m1.XGetGenericArguments().Count() == 1 &&
                m1.Ret() == typeof(String) &&
                m1.Params().Count() == 1 &&
                m1.Params().Single() == m1.XGetGenericArguments().Single());
            dump = dump.XMakeGenericMethod(gargs[0]);
            m.il()
             .ldloc(fileDir)
             .call(typeof(Directory).GetMethod("Exists"))
             .def_label(out exists)
             .brtrue(exists)
             .ldloc(fileDir)
             .call(typeof(Directory).GetMethod("CreateDirectory", new[] { typeof(String) }))
             .pop()
             .label(exists)
             .ldloc(fullPath)
             .ldarg(0)
             .call(dump)
             .call(typeof(File).GetMethod("WriteAllText", new[] { typeof(String), typeof(String) }));
            m.il().ret();

            return m;
        }

        private static Slot BindRead(this ParameterInfo pi, Type t_ctx)
        {
            var slotByName = t_ctx.AllSlots().SingleOrDefault2(s => String.Compare(pi.Name, s.Name, true) == 0);
            // todo. this is very crude check for compatibility of types
            var slotByType = t_ctx.AllSlots().SingleOrDefault2(s => pi.ParameterType.IsAssignableFrom(s.Type));
            return slotByName ?? slotByType;
        }

        private static Slot BindWrite(this ParameterInfo pi, Type t_ctx)
        {
            var slotByName = t_ctx.AllSlots().SingleOrDefault2(s => String.Compare(pi.Name, s.Name, true) == 0);
            // todo. this is very crude check for compatibility of types
            var slotByType = t_ctx.AllSlots().SingleOrDefault2(s => s.Type.IsAssignableFrom(pi.ParameterType));
            return slotByName ?? slotByType;
        }
    }
}
