using System;
using XenoGears.Collections.Observable;

// note. this file is necessary in order to expose XenoGears' API to outer world
// the problem is that when we ilmerge and internalize XenoGears
// then all of a sudden a bunch of APIs become unavailable to our users

namespace Truesight.Decompiler.Hir
{
    public abstract partial class Node
    {
        public new Node Parent { get { return base.Parent; } set { base.Parent = value; } }
        public new event Action<Node, Node> ParentChanging { add { base.ParentChanging += value; } remove { base.ParentChanging -= value; } }
        public new event Action<Node, Node> ParentChanged { add { base.ParentChanged += value; } remove { base.ParentChanged -= value; } }
        public new IObservableList<Node> Children { get { return base.Children; } }

        public new event Action<Node> ChildAdding { add { base.ChildAdding += value; } remove { base.ChildAdding -= value; } }
        public new event Action<Node> ChildAdded { add { base.ChildAdded += value; } remove { base.ChildAdded -= value; } }
        public new event Action<Node> ChildRemoving { add { base.ChildRemoving += value; } remove { base.ChildRemoving -= value; } }
        public new event Action<Node> ChildRemoved { add { base.ChildRemoved += value; } remove { base.ChildRemoved -= value; } }
    }
}
