using System;
using System.Diagnostics;
using XenoGears.Assertions;

namespace Truesight.Decompiler.Pipeline.Flow.Cfg
{
    [DebuggerDisplay("{Name}")]
    internal abstract class BaseNamedEntity
    {
        private String _name;
        private Func<String> _nameProvider;

        public String Name
        {
            get { return _nameProvider != null ? _nameProvider() : _name; }
        }

        public BaseNamedEntity SetName(String name)
        {
            if (_name == "start")
            {
                throw AssertionHelper.Fail();
            }

            _nameProvider = null;
            _name = name;
            return this;
        }

        public BaseNamedEntity SetName(Func<String> nameProvider)
        {
            _nameProvider = nameProvider;
            _name = null;
            return this;
        }
    }
}