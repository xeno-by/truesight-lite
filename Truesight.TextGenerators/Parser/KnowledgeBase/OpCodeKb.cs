using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using XenoGears.Assertions;
using XenoGears.Functional;

namespace Truesight.TextGenerators.Parser.KnowledgeBase
{
    public class OpCodeKb
    {
        public OpCodeFamilyKb FamilyKb { get; private set; }
        public String Family { get { return FamilyKb.Name; } }

        private readonly OpCode? _opCode;
        public OpCode OpCode { get { return _opCode.AssertNotNull().Value; } }

        public HashSet<String> Tags { get; private set; }
        public Dictionary<String, String> Meta { get; private set; }
        public Dictionary<String, FieldSpec> Fields { get; private set; }
        public Dictionary<String, PropertySpec> Props { get; private set; }
        public Dictionary<String, PrefixSpec> Prefixes { get; private set; }

        public OpCodeKb(OpCodeFamilyKb familyKb)
            : this(familyKb, null)
        {
        }

        public OpCodeKb(OpCodeFamilyKb familyKb, OpCode opcode)
            : this(familyKb, (OpCode?)opcode)
        {
        }

        private OpCodeKb(OpCodeFamilyKb familyKb, OpCode? opcode)
        {
            FamilyKb = familyKb;
            _opCode = opcode;

            Tags = new HashSet<String>();
            Meta = new Dictionary<String, String>();
            Fields = new Dictionary<String, FieldSpec>();
            Props = new Dictionary<String, PropertySpec>();
            Prefixes = new Dictionary<String, PrefixSpec>();
        }

        public FieldSpec EnsureField(String name, Type type)
        {
            return Fields.GetOrCreate(name, () => new FieldSpec(){Name = name, Type = type});
        }

        public PropertySpec EnsureProperty(String name, Type type)
        {
            return Props.GetOrCreate(name, () => new PropertySpec(){Name = name, Type = type});
        }

        public PrefixSpec EnsurePrefix(String name, Type type, String prefixName)
        {
            return Prefixes.GetOrCreate(name, () => new PrefixSpec(){Name = name, Type = type, PrefixName = prefixName});
        }
    }
}