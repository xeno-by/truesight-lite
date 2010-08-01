using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Truesight.Decompiler.Hir.Core.Scopes;
using Truesight.Decompiler.Hir.Core.Symbols;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Collections.Observable;
using XenoGears.ComponentModel;
using XenoGears.Traits.Hierarchy;

namespace Truesight.Decompiler.Hir.Core.ControlFlow
{
    // todo. track changes to locals and report them to INPC via SetProperty
    // todo. preserve semantic integrity and prohibit adding/removing locals

    public partial class Iter : Scope
    {
        private readonly IObservableList<Local> _locals = new List<Local>().Observe();
        public IObservableList<Local> Locals { get { return _locals; } }
        protected override void OnFreezing() { Locals.ListChanging += BanChangesToLocalsWhenFrozen; Locals.ForEach(l => l.Freeze()); }
        protected override void OnUnfreezing() { Locals.ListChanging -= BanChangesToLocalsWhenFrozen; Locals.ForEach(l => l.Unfreeze()); }
        private void BanChangesToLocalsWhenFrozen(Object sender, ListChangeEventArgs args) { throw AssertionHelper.Fail(); }

        Scope IImmutableHierarchy2<Scope>.Parent { get { return this.ParentScope(); } }
        ReadOnlyCollection<Scope> IImmutableHierarchy2<Scope>.Children
        {
            get
            {
                // don't just iterate children - we need a hardcode here to be more clear
                return new[] { Body }.Where(n => n != null).Cast<Scope>().ToReadOnly();
            }
        }

        #region Boring boilerplate

        int IImmutableHierarchy2<Scope>.Index { get { return ((IImmutableHierarchy2<Scope>)this).Parent == null ? -1 : ((IImmutableHierarchy2<Scope>)this).Parent.Children.IndexOf(this); } }
        Scope IImmutableHierarchy2<Scope>.Prev { get { return ((IImmutableHierarchy2<Scope>)this).Parent == null ? null : (((IImmutableHierarchy2<Scope>)this).Index == 0 ? null : ((IImmutableHierarchy2<Scope>)this).Parent.Children[Index - 1]); } }
        Scope IImmutableHierarchy2<Scope>.Next { get { return ((IImmutableHierarchy2<Scope>)this).Parent == null ? null : (((IImmutableHierarchy2<Scope>)this).Index == ((IImmutableHierarchy2<Scope>)this).Parent.Children.Count() - 1 ? null : ((IImmutableHierarchy2<Scope>)this).Parent.Children[Index + 1]); } }


        Scope Scope.Parent { get { return ((IImmutableHierarchy2<Scope>)this).Parent; } }
        ReadOnlyCollection<Scope> Scope.Children { get { return ((IImmutableHierarchy2<Scope>)this).Children; } }
        Scope Scope.Prev { get { return ((IImmutableHierarchy2<Scope>)this).Prev; ; } }
        Scope Scope.Next { get { return ((IImmutableHierarchy2<Scope>)this).Next; ; } }

        IImmutableHierarchy2 IImmutableHierarchy2.Parent { get { return ((IImmutableHierarchy2<Scope>)this).Parent; } }
        ReadOnlyCollection<IImmutableHierarchy2> IImmutableHierarchy2.Children { get { return ((IImmutableHierarchy2<Scope>)this).Children.Cast<IImmutableHierarchy2>().ToReadOnly(); } }
        IImmutableHierarchy2 IImmutableHierarchy2.Prev { get { return ((IImmutableHierarchy2<Scope>)this).Prev; } }
        IImmutableHierarchy2 IImmutableHierarchy2.Next { get { return ((IImmutableHierarchy2<Scope>)this).Next; } }

        #endregion
    }
}