using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.ObjectModel;
using XenoGears.Functional;
using XenoGears.Reflection;
using XenoGears.Reflection.Shortcuts;

namespace Truesight.Decompiler.Framework.Impl
{
    [DebuggerNonUserCode]
    internal static class SlotHelpers
    {
        public static ReadOnlyCollection<Slot> AllSlots(this Type t)
        {
            return Seq.Concat(
                t.GetFields(BF.All).Where(f => !f.IsCompilerGenerated()).Select(f => new Slot(f)),
                t.GetProperties(BF.All).Where(p => !p.IsCompilerGenerated()).Select(p => new Slot(p))).ToReadOnly();
        }
    }
}