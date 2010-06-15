using System;
using XenoGears.Assertions;

namespace Truesight.TextGenerators.Parser.KnowledgeBase
{
    public class FieldSpec : SlotSpec
    {
        private String _initializer;
        private Func<FieldSpec, String> _deferredInitializer;
        public String Initializer
        {
            get
            {
                return _deferredInitializer != null ? 
                    _deferredInitializer(this) : _initializer;
            }

            set
            {
                _deferredInitializer.AssertNull();
                ((_initializer ?? value) == value).AssertTrue();
                _initializer = value;
            }
        }

        public void SetLazyInitializer(Func<FieldSpec, String> deferred)
        {
            _deferredInitializer.AssertNull();
            _deferredInitializer = deferred;
        }
    }
}