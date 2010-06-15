using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection.Emit;
using XenoGears.Functional;
using XenoGears.Assertions;

namespace Truesight.Parser.Impl.Reader
{
    [DebuggerNonUserCode]
    internal class ILOpStreamEnumerator : IEnumerator<ILOp>
    {
        private readonly MethodBody _body;
        private readonly BinaryReader _reader;

        object IEnumerator.Current { get { return Current; } }
        public ILOp Current { get; private set; }
        private ILOp Previous { get; set; }

        public ILOpStreamEnumerator(MethodBody body, byte[] bytes)
        {
            _body = body;

            var stream = new MemoryStream(bytes, 0, bytes.Length, false, false);
            stream.CanRead.AssertTrue();
            stream.CanSeek.AssertTrue();
            _reader = new BinaryReader(stream);

            Reset();
        }

        private bool _isDisposed = false;
        public void Dispose()
        {
            ((IDisposable)_reader).Dispose();
            _isDisposed = true;
        }

        public bool MoveNext()
        {
            _isDisposed.AssertFalse();
            if (_reader.BaseStream.Position < _reader.BaseStream.Length)
            {
                Previous = Current;
                Current = ReadNext();

                Current.Prev = Previous;
                if (Previous != null) Previous.Next = Current;

                return true;
            }
            else
            {
                return false;
            }
        }

        public void Reset()
        {
            _isDisposed.AssertFalse();
            _reader.BaseStream.Seek(0, SeekOrigin.Begin);
            Current = null;
        }

        private ILOp ReadNext()
        {
            // Ecma-335 Common Language Infrastructure (CLI)
            // Partition III. 2. Prefixes to instructions
            //
            // It is not valid CIL to have a prefix without immediately following it by one of the
            // instructions it is permitted to precede.
            // note. this is enforced by the while (true) loop
            //
            // It is not valid CIL to branch to the instruction following the prefix, 
            // but the prefix itself is a valid branch target.
            // note. this is implemented by offset magic in generated ops

            var prefixes = new List<ILOp>();
            while (true)
            {
                var opcode = OpCodeReader.ReadOpCode(_reader).AssertNotNull().Value;
                if (opcode.FlowControl == FlowControl.Meta)
                {
                    prefixes.Add(ILOpFactory.Create(opcode, _body, _reader, null));
                }
                else
                {
                    // not clearing prefixes since we just return away from here
                    return ILOpFactory.Create(opcode, _body, _reader, prefixes.ToReadOnly());
                }
            }
        }
    }
}