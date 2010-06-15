using System.Linq;
using Truesight.Decompiler.Domains;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Traits.Hierarchy;

namespace Truesight.Decompiler.Hir
{
    public abstract partial class Node
    {
        private Domain _domain = Domain.Current;
        public Domain Domain
        {
            get { return _domain; }
            set
            {
                SetProperty("Domain", 
                v =>
                {
                    _domain = v;
                    var children = this.ChildrenRecursive().Where(c => c != null);
                    children.ForEach(c => c.Domain = v);
                },
                _domain, value);
            }
        }

        private void SetDomainRelatedChangeTrackingHooks()
        {
            this.ChildAdded += child =>
            {
                if (child == null) return;
                (child.Domain == null || child.Domain == this.Domain).AssertTrue();
                child.Domain = this.Domain;
            };

            this.PropertyChanged += (o, args) =>
            {
                if (Domain != null)
                {
                    var dtc = this.Domain.DumpAsTextCache;
                    var tic = this.Domain.TypeInferenceCache;
                    if (dtc.ContainsKey(this) || tic.ContainsKey(this))
                    {
                        var hierarchy = this.Hierarchy();
                        hierarchy.ForEach(n => this.Domain.DumpAsTextCache.Remove(n));
                        hierarchy.ForEach(n => this.Domain.TypeInferenceCache.Remove(n));
                    }
                }
            };
        }
    }
}
