using System;
using XenoGears.Assertions;

namespace Truesight.TextGenerators.Parser.KnowledgeBase
{
    public class PropertySpec : SlotSpec
    {
        private String _getter;
        private Func<PropertySpec, String> _deferredGetter;
        public String Getter
        {
            get
            {
                return _deferredGetter != null ? 
                    _deferredGetter(this) : _getter;
            }

            set
            {
                _deferredGetter.AssertNull();
                ((_getter ?? value) == value).AssertTrue();
                _getter = value;
            }
        }

        public void SetLazyGetter(Func<PropertySpec, String> deferred)
        {
            _deferredGetter.AssertNull();
            _deferredGetter = deferred;
        }

        private String _setter;
        private Func<PropertySpec, String> _deferredSetter;
        public String Setter
        {
            get
            {
                return _deferredSetter != null ?
                    _deferredSetter(this) : _setter;
            }

            set
            {
                _deferredSetter.AssertNull();
                ((_setter ?? value) == value).AssertTrue();
                _setter = value;
            }
        }

        public void SetLazySetter(Func<PropertySpec, String> deferred)
        {
            _deferredSetter.AssertNull();
            _deferredSetter = deferred;
        }
    }
}