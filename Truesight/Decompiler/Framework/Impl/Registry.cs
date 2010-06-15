using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using XenoGears.Assertions;
using XenoGears.Functional;
using Truesight.Decompiler.Framework.Annotations;
using XenoGears.Reflection.Attributes;
using XenoGears.Reflection.Shortcuts;

namespace Truesight.Decompiler.Framework.Impl
{
    [DebuggerNonUserCode]
    internal static class Registry
    {
        private static Object _cacheLock = new Object();
        private static Dictionary<PipelineAttribute, ReadOnlyCollection<PipelineStep>> _cache =
            new Dictionary<PipelineAttribute, ReadOnlyCollection<PipelineStep>>();

        static Registry()
        {
            AppDomain.CurrentDomain.AssemblyLoad += (o, e) =>
            {
                lock (_cacheLock) { _cache = new Dictionary<PipelineAttribute, ReadOnlyCollection<PipelineStep>>(); }
            };
        }

        public static ReadOnlyCollection<PipelineStep> For(PipelineAttribute annotation)
        {
            // temporary variable is introduced in order
            // not to get a crash when the cache is invalidated upon new assembly load
            var cache = _cache;

            if (!cache.ContainsKey(annotation))
            {
                lock (_cacheLock)
                {
                    if (!cache.ContainsKey(annotation))
                    {
                        var t_a_method = annotation.StepMarker;
                        var t_a_type = annotation.WorkshopMarker;
                        var t_a_asm = annotation.PlantMarker;

                        // todo. cache this and invalidate cache on appdomain changes
                        _cache.Add(annotation, AppDomain.CurrentDomain.GetAssemblies().SelectMany(asm =>
                        {
                            var a_asm = asm.AttrOrNull(t_a_asm).AssertCast<PipelinePlantAttribute>();
                            if (a_asm == null)
                            {
                                return Enumerable.Empty<PipelineStep>();
                            }
                            else
                            {
                                return asm.GetTypes().SelectMany(t =>
                                {
                                    var a_type = t.AttrOrNull(t_a_type).AssertCast<PipelineWorkshopAttribute>();
                                    if (a_type == null)
                                    {
                                        return Enumerable.Empty<PipelineStep>();
                                    }
                                    else
                                    {
                                        return t.GetMethods(BF.All).SelectMany(m =>
                                        {
                                            var a_method = m.AttrOrNull(t_a_method).AssertCast<PipelineStepAttribute>();
                                            if (a_method == null)
                                            {
                                                return Enumerable.Empty<PipelineStep>();
                                            }
                                            else
                                            {
                                                return new PipelineStep(a_asm, a_type, a_method, m).MkArray();
                                            }
                                        });
                                    }
                                });
                            }
                        }).Order().ToReadOnly());
                    }
                }
            }

            // temporary variable is used in order
            // not to get a crash when the cache is invalidated upon new assembly load
            return cache[annotation];
        }
    }
}