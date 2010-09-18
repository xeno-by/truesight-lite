using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Truesight.Parser;
using Truesight.Parser.Api;
using Truesight.Parser.Api.Emit;
using Truesight.Parser.Api.Ops;
using XenoGears.Collections.Dictionaries;
using XenoGears.Functional;
using XenoGears.Assertions;
using XenoGears.Collections;
using XenoGears.Reflection.Emit;
using XenoGears.Reflection.Shortcuts;
using XenoGears.Reflection.Simple;
using Switch = Truesight.Parser.Api.Ops.Switch;

namespace Truesight.Playground.Parser.ILRewriter
{
    internal class ILRewriter : IILRewriteControl, IILRewriterContext
    {
        private readonly MethodBase _src;
        private readonly ReadOnlyCollection<IILOp> _body;
        private readonly ReadOnlyDictionary<int, IILOp> _off2op;
        private readonly MethodBuilder _dest;
        private readonly Func<IILOp, IILRewriteControl, IILRewriteControl> _logic;

        private IILOp _curr;
        private ILGenerator _il { get { return _dest.il(); } }

        private byte[] _originalIL;
        private byte[] _rewrittenIL;

        public ILRewriter(MethodBase src, MethodBuilder dest,
            Func<IILOp, IILRewriteControl, IILRewriteControl> logic)
        {
            _src = src;
            var body = _src.ParseBody();

            _originalIL = body.RawIL.ToArray();
            body.RawEHC.AssertEmpty();
            _body = body.ToReadOnly();
            _off2op = _body.ToDictionary(op => op.Offset, op => op).ToReadOnly();

            _dest = dest;
            _logic = logic;
        }

        #region Rewriting logic

        private readonly Status _status = new Status();
        [DebuggerNonUserCode] private class Status : IDisposable
        {
            private bool _inProgress = true;
            public bool InProgress { get { return _inProgress; } }

            public void Dispose()
            {
                _inProgress = false;
            }
        }

        [Conditional("TRACE")]
        private void TraceRawIl(IEnumerable<byte> bytes, int i)
        {
            if (Directory.Exists(@"d:\rawil\")) 
            {
                var rawIL = bytes.ToArray();

                using (var sw = new StreamWriter(@"d:\rawil\rawil-diff-" + i))
                {
                    foreach (var op in rawIL.ParseRawIL().SelectMany(op => op.Prefixes.Concat(op)))
                    {
                        sw.WriteLine(String.Format("0x{0:x4}: {1} {2}",
                            0, /* for the sake of easy diffing */
                            op.OpSpec.OpCode.Name.PadRight(10),
                            op.BytesWithoutPrefixes.Select(b => b.ToString("x2")).StringJoin(" ")));
                    }
                }

                using (var sw = new StreamWriter(@"d:\rawil\rawil-offsets-" + i))
                {
                    foreach (var op in rawIL.ParseRawIL().SelectMany(op => op.Prefixes.Concat(op)))
                    {
                        sw.WriteLine(String.Format("0x{0:x4}: {1} {2}",
                            op.OffsetOfOpCode,
                            op.OpSpec.OpCode.Name.PadRight(10),
                            op.BytesWithoutPrefixes.Select(b => b.ToString("x2")).StringJoin(" ")));
                    }
                }
            }
        }

        public void DoRewrite()
        {
            _status.InProgress.AssertTrue();

            using (_status)
            {
                Func<byte[]> m_ILStream = () => _il.Get("m_ILStream").AssertCast<byte[]>();
                var typeBuilder = _dest.Get("m_containingType").AssertCast<TypeBuilder>();
                var module = typeBuilder.Get("m_module").AssertCast<ModuleBuilder>();
                Func<int> get_length = () => _il.Get("m_length").AssertCast<int>();
                Action<int> set_length = v => _il.Set("m_length", v);

                // Stage 1: Write rewritten ops according to _logic
                // So far we don't care about branches being fix't up
                var rawIl_BeforeStage1 = _src.ParseBody().RawIL;
                TraceRawIl(rawIl_BeforeStage1, 0);

                foreach (var op in _body)
                {
                    _curr = op;
                    _logic(op, this);
                }

                // Stage 2: Replace short branches with regular ones
                // since fixups (see below) might overflow the 1-byte offset storage
                _rewrittenIL = m_ILStream().Take(get_length()).ToArray();
                TraceRawIl(_rewrittenIL, 1);
                set_length(0);

                var fixupUpdates = new List<Action>();
                var branchExpansionPoints = new Dictionary<int, int>();
                foreach (var rop in _rewrittenIL.ParseRawIL(module))
                {
                    var rop1 = rop;
                    if (rop1 is Branch)
                    {
                        var br = rop1.AssertCast<Branch>();
                        if (rop1.OpSpec.OperandType == OperandType.ShortInlineBrTarget)
                        {
                            // rewrite short branch to normal form
                            var opcode_sig = rop1.OpSpec.OpCode.Value + 0x0d;
                            var allOpcodes = typeof(OpCodes).GetFields(BF.PublicStatic).Select(f => f.GetValue(null).AssertCast<OpCode>());
                            var opcode = allOpcodes.Single(oc => oc.Value == opcode_sig);
                            _il.raw(opcode, BitConverter.GetBytes(br.RelativeTargetOffset));

                            // update fixup of the next operation
                            // fixup = translate from original offsets to after-stage-2 ones
                            var preImage = _off2op[RewrittenToNearestOrig(rop1.Offset)];
                            if (preImage.Next != null) fixupUpdates.Add(() => _fixups[preImage.Next] += 3);

                            // update the map that memorizes BEPs in after-stage-1 coordinates
                            fixupUpdates.Add(() => branchExpansionPoints[rop1.Offset] = 3);

                            // also be careful to fixup _offsetsModeChangeLog => lazily!
                            // fixup = translate from after-stage-1 to after-stage-2 coordinates
                            foreach (var entry in _offsetsModeChangeLog)
                            {
                                (rop1.Offset < entry.Offset && entry.Offset < rop1.OffsetOfNextOp).AssertFalse();

                                var entry1 = entry;
                                var fixup = rop1.Offset >= entry1.Offset ? 0 : 3;
                                fixupUpdates.Add(() => entry1.Offset += fixup);
                            }
                        }
                        else
                        {
                            _il.clone(rop1);
                        }
                    }
                    else
                    {
                        _il.clone(rop1);
                    }
                }

                // we defer updates so that they won't fuck up _fixups in process
                fixupUpdates.RunEach();

                // Stage 3: Fixup branches since their offsets are most likely invalid now
                // todo. also fixup switches and protected regions
                //
                // note. this works as follows:
                // 1) if an instruction was emitted during OffsetsMode.Original, 
                //    i.e. (if EnterXXXMode wasn't called) during Clone 
                //    it needs a fixup and uses the main fixup log, i.e. _fixups.
                // 2) if an instruction was emitted during OffsetsMode.Rewritten,
                //    i.e. (if EnterXXXMode wasn't called) during Clone 
                //    it ain't need a fixup due to rewriting, 
                //    but does need a fixup to compensate for short branches expansion (see above)
                //    and thusly uses the branchExpansionFixups log
                _rewrittenIL = m_ILStream().Take(get_length()).ToArray();
                TraceRawIl(_rewrittenIL, 2);
                foreach (var rop in _rewrittenIL.ParseRawIL(module))
                {
                    // note #1.
                    // we can't use Target here since offsets are most likely broken at the moment
                    // at best it'll return null, and at worst it'll crash

                    // note #2.
                    // we can't use AbsoluteTargetOffset either since it positions at after-stage-2 IL
                    // but not to before-stage-1 IL (i.e. original)

                    // note #3.
                    // neither we can use RewrittenToOrig(rop.Offset) 
                    // since, even if we're at OffsetsMode.Original, the rop might've been issued during r/w

                    if (rop is Branch)
                    {
                        var br = rop.AssertCast<Branch>();
                        var pivot = br.OffsetOfNextOp;

                        Action<int> rewriteRelOffset = newRelOffset =>
                        {
                            var addrOfOperand = rop.OffsetOfOperand;
                            var fixedUp = BitConverter.GetBytes(newRelOffset);
                            fixedUp.ForEach((b, i) => m_ILStream()[i + addrOfOperand] = b);
                        };

                        var mode = GetOffsetsMode(br.Offset);
                        // see section #1 of the above comment
                        if (mode == OffsetsMode.Original)
                        {
                            // note. important: original offsets mode here 
                            // doesn't mean that the op processed has a corresponding preimage (see also n0te #3 above)
                            // it only means that the relative offset works in original mode

                            // step 1. find out orig_relOffset (note: trick here!)
                            var r_r_relOffset = br.RelativeTargetOffset;
                            var nearestOrig = _off2op[RewrittenToNearestOrig(br.Offset)];
                            var r_off2op = _rewrittenIL.ParseRawIL(module).ToDictionary(op => op.Offset, op => op);
                            var r_nearestOrig = r_off2op[OrigToRewritten(nearestOrig.Offset)];
                            var orig_relOffset = r_r_relOffset + (br.Offset - r_nearestOrig.Offset);

                            // step 2. restore the op that orig_relOffset references
                            var orig_pivot = nearestOrig.OffsetOfNextOp;
                            var orig_absOffset = orig_pivot + orig_relOffset;

                            // step 3. find out the r_pivot (note: trick here!)
                            var r_pivot = br.OffsetOfNextOp;

                            // step 4. calculate the r_relOffset
                            var r_absOffset = OrigToRewritten(orig_absOffset);
                            var r_relOffset = r_absOffset - r_pivot;

                            rewriteRelOffset(r_relOffset);
                        }
                        // see section #2 of the above comment
                        else if (mode == OffsetsMode.Rewritten)
                        {
                            // we can't use relOffset here 
                            // because the "where" condition will become really complex
                            // since we need to take into account both negative and positive rels
                            var absOffset = br.AbsoluteTargetOffset;
                            branchExpansionPoints
                                .Where(kvp => kvp.Key < br.AbsoluteTargetOffset)
                                .ForEach(kvp => absOffset += kvp.Value);

                            rewriteRelOffset(absOffset - pivot);
                        }
                        else
                        {
                            throw AssertionHelper.Fail();
                        }
                    }
                    else if (rop is Switch)
                    {
                        throw AssertionHelper.Fail();
                    }
                    else
                    {
                        // do nothing - this op ain't eligible for fixup
                    }
                }

                // Finalizing the operations
                // fill in the RewrittenOps
                _rewrittenIL = m_ILStream().Take(get_length()).ToArray();
                TraceRawIl(_rewrittenIL, 3);
                var opsAfterRewriting = _rewrittenIL.ParseRawIL(module).ToDictionary(op => op.Offset, op => op);
                foreach (var rop in _rewrittenOps.Keys.ToArray())
                {
                    var startOffset = OrigToRewritten(rop.Offset);
                    var endOffset = rop.Next == null ? int.MaxValue : OrigToRewritten(rop.Next.Offset);
                    var imageOps = opsAfterRewriting
                        .Where(kvp2 => startOffset <= kvp2.Key && kvp2.Key < endOffset)
                        .Select(kvp2 => kvp2.Value);

                    _rewrittenOps[rop] = imageOps.ToReadOnly();
                }
            }
        }

        private OffsetsMode _offsetsMode = OffsetsMode.Rewritten;
        public void EnterOriginalOffsetsMode() { OffsetsMode = OffsetsMode.Original;  }
        public void EnterRewrittenOffsetsMode() { OffsetsMode = OffsetsMode.Rewritten; }

        private class ModeOffset { public OffsetsMode Mode { get; set; } public int Offset { get; set; } public ModeOffset(OffsetsMode mode, int offset) { Mode = mode; Offset = offset; } }
        private List<ModeOffset> _offsetsModeChangeLog = new List<ModeOffset>();
        private OffsetsMode GetOffsetsMode(int rewrittenOffset)
        {
            ModeOffset result = null;
            for (var i = -1; i < _offsetsModeChangeLog.Count() + 1; ++i)
            {
                var item1 = (0 <= i && i < _offsetsModeChangeLog.Count()) ? _offsetsModeChangeLog[i] : null;
                var item2 = (0 <= (i + 1) && (i + 1) < _offsetsModeChangeLog.Count()) ? _offsetsModeChangeLog[i + 1] : null;

                var offset1 = item1 != null ? item1.Offset : int.MinValue;
                var offset2 = item2 != null ? item2.Offset : int.MaxValue;
                if (offset1 <= rewrittenOffset && rewrittenOffset < offset2)
                {
                    result = item1;
                    continue;
                }
            }

            return result.AssertNotNull().Mode;
        }

        public OffsetsMode OffsetsMode
        {
            get { return _offsetsMode; } 
            set
            {
                _offsetsMode = value;

                var length = _il.Get("m_length").AssertCast<int>();
                _offsetsModeChangeLog.Add(new ModeOffset(value, length));
            }
        }

        IILRewriteControl IILRewriteControl.Clone()
        {
            _clonedOps.Add(_curr);
            EnterOriginalOffsetsMode();
            DoRewrite(il => il.clone(_curr));
            return this;
        }

        IILRewriteControl IILRewriteControl.Rewrite(Action<ILGenerator> rewriter)
        {
            _rewrittenOps.Add(_curr, null);
            EnterRewrittenOffsetsMode();
            DoRewrite(rewriter);
            return this;
        }

        IILRewriteControl IILRewriteControl.StripOff()
        {
            _rewrittenOps.Add(_curr, null);
            EnterRewrittenOffsetsMode();
            DoRewrite(il => il.nop());
            return this;
        }

        private void DoRewrite(Action<ILGenerator> logic)
        {
            _status.InProgress.AssertTrue();

            Func<int> get_length = () => _il.Get("m_length").AssertCast<int>();
            var before = get_length();
            logic(_il);
            var after = get_length();

            _fixups[_curr] = _pendingFixup;
            _pendingFixup = (after - before) - _curr.Size;
        }

        #endregion

        #region Post-rewrite services

        private readonly HashSet<IILOp> _clonedOps = new HashSet<IILOp>();
        public HashSet<IILOp> ClonedOps { get { return _clonedOps; } }

        private readonly Dictionary<IILOp, ReadOnlyCollection<IILOp>> _rewrittenOps = new Dictionary<IILOp, ReadOnlyCollection<IILOp>>();
        public Dictionary<IILOp, ReadOnlyCollection<IILOp>> RewrittenOps { get { return _rewrittenOps; } }

        private int _pendingFixup = 0;
        private readonly Dictionary<IILOp, int> _fixups = new Dictionary<IILOp, int>();

        private ReadOnlyCollection<Tuple<int, IILOp>> _appliedFixups
        {
            get
            {
                return _fixups.Scanae
                (
                    0,
                    (acc, kvp, _) => acc + kvp.Value,
                    (acc, kvp, _) => Tuple.Create(acc + kvp.Key.Offset, kvp.Key)
                ).ToReadOnly();
            }
        }

        // named "strict" because requires input match exactly the start of some orig op
        private Dictionary<int, int> _o2r_strict
        {
            get { return _appliedFixups.Skip(1).ToDictionary(t => t.Item2.Offset, t => t.Item1); }
        }

        // named "strict" because requires input match exactly the image of the start of some orig op
        private Dictionary<int, int> _r2o_strict
        {
            get { return _o2r_strict.ToDictionary(kvp => kvp.Value, kvp => kvp.Key); }
        }

        public int OrigToRewritten(int origOffset)
        {
            if (origOffset == _originalIL.Length) return _rewrittenIL.Length;
            if (origOffset == 0) return 0;
            return _o2r_strict[origOffset];
        }

        public int RewrittenToOrig(int rewrittenOffset)
        {
            if (rewrittenOffset == _rewrittenIL.Length) return _originalIL.Length;
            if (rewrittenOffset == 0) return 0;
            return _r2o_strict[rewrittenOffset];
        }

        public int RewrittenToNearestOrig(int rewrittenOffset)
        {
            if (rewrittenOffset == _rewrittenIL.Length) return _originalIL.Length;

            Tuple<int, IILOp> result = null;
            for (var i = -1; i < _appliedFixups.Count() + 1; ++i)
            {
                var item1 = (0 <= i && i < _appliedFixups.Count()) ? _appliedFixups[i] : null;
                var item2 = (0 <= (i + 1) && (i + 1) < _appliedFixups.Count()) ? _appliedFixups[i + 1] : null;

                var offset1 = item1 != null ? item1.Item1 : int.MinValue;
                var offset2 = item2 != null ? item2.Item1 : int.MaxValue;
                if (offset1 <= rewrittenOffset && rewrittenOffset < offset2)
                {
                    result = item1;
                    continue;
                }
            }

            // todo. fixme!
            // consult previous revisions of this file
            return result.AssertNotNull().Item2.Offset;
        }

        #endregion
    }
}