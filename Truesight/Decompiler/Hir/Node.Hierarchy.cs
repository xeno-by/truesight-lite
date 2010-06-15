using System.Diagnostics;
using XenoGears.Assertions;
using XenoGears.Collections.Observable;
using XenoGears.Traits.Hierarchy;

namespace Truesight.Decompiler.Hir
{
    public abstract partial class Node : Hierarchy<Node>
    {
        // note. changing child's parent is a great failboat
        // since it automatically implies removing the child from parent's Children collection
        // which contradicts to almost all AST nodes' assumption that # of children never changes
        private bool _historicalNote_parentBecameNotNull = false;
        private bool _historicalNote_parentBecameNullAgain = false;
        protected override void OnParentChanged(Node oldParent, Node newParent)
        {
            (oldParent == null ^ newParent == null).AssertTrue();
            (newParent == null && !_historicalNote_parentBecameNotNull).AssertFalse();
            (newParent != null && _historicalNote_parentBecameNullAgain).AssertFalse();

            _historicalNote_parentBecameNotNull = newParent != null;
            _historicalNote_parentBecameNullAgain = newParent == null;
        }

        // note. the one and only echelon of protection
        // here we clone all nodes that are added to our children collection
        //
        // todo. here we've got a non-obvious problem
        // what if the node that currently doesn't have a parent
        // gets occasionally captured by some parent node
        // programmer would think that before the capture the node will be cloned
        // however it would not, because it still ain't have a parent
        // when the programmer would assign a legitimate parent, the program will crash
        //
        // note. see DecompileComplexConditions to get the idea in detail:
        // at the beginning we've got shitloads of control flow blocks
        // that don't contain balanced code and have exactly one expression in their residues
        // when merging those we compose new expressions from those residues
        // those composes expressions would hijack fatherhood
        //
        // in that particular case such behavior doesn't lead to anything unexpected
        // however, potentially it might spawn a hard-to-understand bug
        protected override IObservableList<Node> InitChildren() { return new ChildrenCollection(this); }
        [DebuggerNonUserCode] private class ChildrenCollection : ObservableList<Node>
        {
            private readonly Node _this;
            public ChildrenCollection(Node parent) { _this = parent; }

            protected override void InsertItem(int index, Node item)
            {
                if (item != null && item.Parent != null && item.Parent != _this)
                    item = item.DeepClone();
                base.InsertItem(index, item);
            }

            protected override void SetItem(int index, Node item)
            {
                if (item != null && item.Parent != null && item.Parent != _this)
                    item = item.DeepClone();
                base.SetItem(index, item);
            }
        }
    }
}