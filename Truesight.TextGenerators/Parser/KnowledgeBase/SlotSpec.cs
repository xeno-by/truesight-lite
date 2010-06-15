using System;
using XenoGears.Assertions;

namespace Truesight.TextGenerators.Parser.KnowledgeBase
{
    public class SlotSpec
    {
        private String _name;
        public String Name
        {
            get { return _name; }
            set
            {
                ((_name ?? value) == value).AssertTrue();
                _name = value;
            }
        }

        private Type _type;
        public Type Type
        {
            get { return _type; }
            set
            {
                ((_type ?? value) == value).AssertTrue();
                _type = value;
            }
        }

        private bool _isUnsafe;
        public bool IsUnsafe
        {
            get { return _isUnsafe; }
            set { _isUnsafe = value; }
        }
    }
}