using System;
using System.Diagnostics;
using System.Reflection;

namespace Truesight.Parser
{
    // todo. also support debug info context here

    [DebuggerNonUserCode]
    public class ParserContext
    {
        public Module Module { get; private set; }
        public Type Type { get; private set; }
        public MethodBase Method { get; private set; }

        public ParserContext(Module module) { Module = module; }
        public static implicit operator ParserContext(Module module)
        {
            return new ParserContext(module);
        }

        public ParserContext(Type type) : this(type.Module) { Type = type; }
        public static implicit operator ParserContext(Type type)
        {
            return new ParserContext(type);
        }

        public ParserContext(MethodBase method) : this(method.DeclaringType) { Method = method; }
        public static implicit operator ParserContext(MethodBase method)
        {
            return new ParserContext(method);
        }
    }
}