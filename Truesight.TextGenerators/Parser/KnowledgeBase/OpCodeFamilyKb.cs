using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection.Emit;
using XenoGears;

namespace Truesight.TextGenerators.Parser.KnowledgeBase
{
    public class OpCodeFamilyKb
    {
        public String Name { get; private set; }
        public ReadOnlyCollection<OpCode> OpCodes { get { return SubSpecs.Keys.ToReadOnly(); } }
        public Dictionary<OpCode, OpCodeKb> SubSpecs { get; private set; }

        public OpCodeFamilyKb(String name)
        {
            Name = name;
            SubSpecs = new Dictionary<OpCode, OpCodeKb>();
        }
    }
}