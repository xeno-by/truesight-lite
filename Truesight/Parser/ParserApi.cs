using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Truesight.Parser.Api;

namespace Truesight.Parser
{
    [DebuggerNonUserCode]
    public static class ParserApi
    {
        public static IMethodBody ParseBody(this MethodBase method)
        {
            return method.ParseBody(false);
        }

        public static IMethodBody ParseBody(this MethodBase method, bool loadDebugInfo)
        {
            return method.GetMethodBody() == null ? null : new Impl.MethodBody(method, loadDebugInfo);
        }

        public static IMethodBody Parse(this MethodBody methodBody)
        {
            return methodBody.Parse(false);
        }

        public static IMethodBody Parse(this MethodBody methodBody, bool loadDebugInfo)
        {
            return methodBody == null ? null : new Impl.MethodBody(methodBody, loadDebugInfo);
        }

        public static IMethodBody ParseRawIL(this byte[] rawIL)
        {
            return new Impl.MethodBody(rawIL);
        }

        public static IMethodBody ParseRawIL(this byte[] rawIL, ParserContext ctx)
        {
            return new Impl.MethodBody(rawIL, ctx);
        }

        public static IMethodBody ParseRawIL(this byte[] rawIL, IEnumerable<ExceptionHandlingClause> rawEhc)
        {
            return new Impl.MethodBody(rawIL, rawEhc);
        }

        public static IMethodBody ParseRawIL(this byte[] rawIL, IEnumerable<ExceptionHandlingClause> rawEhc, ParserContext ctx)
        {
            return new Impl.MethodBody(rawIL, rawEhc, ctx);
        }
    }
}
