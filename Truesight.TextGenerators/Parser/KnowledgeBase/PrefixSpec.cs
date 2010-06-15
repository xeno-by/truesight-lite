using System;
using XenoGears.Assertions;

namespace Truesight.TextGenerators.Parser.KnowledgeBase
{
    public class PrefixSpec : PropertySpec
    {
        private String _prefixName;
        public String PrefixName
        {
            get { return _prefixName; }
            set
            {
                ((_prefixName ?? value) == value).AssertTrue();
                _prefixName = value;
            }
        }

        private String _filter;
        private Func<PrefixSpec, String> _deferredFilter;
        public String Filter
        {
            get
            {
                return _deferredFilter != null ?
                    _deferredFilter(this) : _filter;
            }

            set
            {
                _deferredFilter.AssertNull();
                ((_filter ?? value) == value).AssertTrue();
                _filter = value;
            }
        }

        public void SetLazyFilter(Func<PrefixSpec, String> deferred)
        {
            _deferredFilter.AssertNull();
            _deferredFilter = deferred;
        }
    }
}