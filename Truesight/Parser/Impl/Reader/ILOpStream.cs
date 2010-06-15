using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using XenoGears.Assertions;

namespace Truesight.Parser.Impl.Reader
{
    [DebuggerNonUserCode]
    internal class ILOpStream : IEnumerable<ILOp>
    {
        private readonly MethodBody _body;
        private readonly byte[] _ilBytes;

        public ILOpStream(MethodBody body, byte[] ilBytes)
        {
            _body = body;
            _ilBytes = ilBytes.AssertNotNull();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<ILOp> GetEnumerator()
        {
            return new ILOpStreamEnumerator(_body, _ilBytes);
        }
    }
}
