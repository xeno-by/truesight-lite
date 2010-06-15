using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Hir.Traversal;
using Truesight.Decompiler.Pipeline.Attrs;
using Truesight.Decompiler.Pipeline.Cil.OpAssign;
using Truesight.Decompiler.Pipeline.Flow.Cfg;
using System.Linq;
using XenoGears.Functional;
using XenoGears.Traits.Equivatable;

namespace Truesight.Decompiler.Pipeline.Cil
{
    // todo. will this work for user-defined operators?
    [Decompiler(Weight = (int)Stages.PostprocessEarlyHir)]
    internal static class RestoreOpAssignOperators
    {
        // todo. there are cases when decompilation result will not be semantically correct
        // that's because we can't 100% deterministically analyze side-effects in IL
        // so here we just search for patterns that are emitted by C# compiler
        // see InitialDecompilation::FixupEvaluationOrder for related information
        [DecompilationStep(Weight = 3)]
        public static void DoRestoreOpAssignOperators(ControlFlowGraph cfg, Symbols symbols)
        {
            var dirty = true;
            var dfa = new DfaHelper(cfg);
            while (dirty)
            {
                dirty = false;
                foreach (var atom in dfa.Atoms())
                {
                    if (dirty |= TryMatchPattern1(dfa, atom)) break;
                    if (dirty |= TryMatchPattern2(dfa, atom)) break;
                    if (dirty |= TryMatchPattern3(dfa, atom)) break;
                    if (dirty |= TryMatchPattern4(dfa, atom)) break;
                }
            }
        }

        // note. pattern #1:
        // * ref = atom
        // * read(ref)
        // conditions:
        // 1) ref is read only once after it had been assigned
        // 2) atom isn't reassigned before read(ref) takes place
        // 3) if ref is CF$XXXX then atom can be of any node type (not only an atom)
        // transformed into:
        // * read(atom)
        private static bool TryMatchPattern1(DfaHelper dfa, Expression atom)
        {
            var p1_ref = atom as Ref;
            if (p1_ref == null) return false;

            var p1_lastass = dfa.Writes(p1_ref).LastOrDefault();
            if (p1_lastass == null) return false;

            var p1_ref_onlyread_afterass = dfa.Reads(p1_ref).SingleOrDefault2(
                r => dfa.ExecOrderOfStmt(r) >= dfa.ExecOrderOfStmt(p1_lastass));
            if (p1_ref_onlyread_afterass == null) return false;

            var p1_atom = p1_lastass.Rhs;
            if (!p1_ref.Sym.Name.StartsWith("CF$") &&
                !p1_ref.Sym.Name.StartsWith("CS$"))
            {
                if (!p1_atom.IsAtom()) return false;

                var p1_atom_reasses = dfa.Writes(p1_atom).Where(w =>
                    dfa.ExecOrderOfStmt(p1_lastass) <= dfa.ExecOrderOfStmt(w) &&
                    dfa.ExecOrderOfStmt(w) <= dfa.ExecOrderOfStmt(p1_ref_onlyread_afterass));
                if (p1_atom_reasses.IsNotEmpty()) return false;
            }

            dfa.Remove(p1_lastass);
            var p1_ref_onlyread_stmt = p1_ref_onlyread_afterass.Stmt();
            dfa.ReplaceRecursive(p1_ref_onlyread_stmt, p1_ref, p1_atom);
            return true;
        }

        // note. pattern #2: 
        // * atom = atom op any
        // transformed into: 
        // * atom [op=] any
        private static bool TryMatchPattern2(DfaHelper dfa, Expression atom)
        {
            var p2_atom = atom;
            foreach (var p2_ass in dfa.Writes(atom))
            {
                if (!(p2_ass.Rhs is BinaryOperator)) continue;
                var binary = p2_ass.Rhs as BinaryOperator;
                if (!p2_atom.Equiv(binary.Lhs)) continue;

                var p2_opeq = p2_atom.CreateOpPreAssign(binary.OperatorType, binary.Rhs);
                if (p2_opeq == null) return false;

                dfa.Replace(p2_ass, p2_opeq);
                return true;
            }

            return false;
        }

        // note. pattern #3:
        // <op> ref = atom op any
        // <wb> atom = ref
        // <usage> read(ref)
        // conditions:
        // 1) exactly one read of ref
        // 2) <op> comes before <wb> and <usage>
        // 3) <wb> and <usage> may be ordered arbitrarily
        // transformed into either:
        // * read(++atom), if op is an increment (use IsInc() method to test)
        // * read(--atom), if op is a decrement (use IsDec() method to test)
        // * read(atom [op=] any), otherwise
        private static bool TryMatchPattern3(DfaHelper dfa, Expression atom)
        {
            var p3_ref = atom as Ref;
            if (p3_ref == null) return false;
            var p3_usages = dfa.Usages(p3_ref);
            if (p3_usages.Count() != 3) return false;

            var p3_op = p3_usages.First() as Assign;
            if (p3_op == null) return false;
            if (!p3_op.Lhs.Equiv(p3_ref)) return false;
            var p3_bop = p3_op.Rhs as BinaryOperator;
            if (p3_bop == null) return false;
            var p3_atom = p3_bop.Lhs;
            var p3_optype = p3_bop.OperatorType;
            var p3_any = p3_bop.Rhs;
            if (!p3_atom.IsAtom()) return false;

            var p3_usage2_stmt = p3_usages.Second().Stmt();
            var p3_usage3_stmt = p3_usages.Third().Stmt();
            Expression p3_wb, p3_usage;
            var p3_wb_template = new Assign(p3_atom, p3_ref);
            if (p3_usage2_stmt.Equiv(p3_wb_template)) { p3_wb = (Expression)p3_usage2_stmt; p3_usage = (Expression)p3_usage3_stmt; }
            else if (p3_usage3_stmt.Equiv(p3_wb_template)) { p3_wb = (Expression)p3_usage3_stmt; p3_usage = (Expression)p3_usage2_stmt; }
            else return false;

            var p3_opeq = p3_atom.CreateOpPreAssign(p3_optype, p3_any);
            if (p3_opeq == null) return false;

            dfa.Remove(p3_op, p3_wb);
            dfa.ReplaceRecursive(p3_usage, p3_ref, p3_opeq);
            return true;
        }

        // note. pattern #4
        // <hoard> ref = atom
        // <op> atom = ref op any
        // <usage> read(ref)
        // conditions:
        // 1) exactly one read of ref
        // 2) <hoard> comes before <op> and <usage>
        // 3) <op> and <usage> may be ordered arbitrarily
        // 4) op is either an increment or a decrement
        // transformed into either:
        // * read(atom++), if op is an increment (use IsInc() method to test)
        // * read(atom--), if op is a decrement (use IsDec() method to test)
        private static bool TryMatchPattern4(DfaHelper dfa, Expression atom)
        {
            var p4_ref = atom as Ref;
            if (p4_ref == null) return false;
            var p4_usages = dfa.Usages(p4_ref);
            if (p4_usages.Count() != 3) return false;

            var p4_op = p4_usages.Select(u => u.Stmt()).OfType<Assign>().SingleOrDefault2(ass =>
            {
                var bo = ass.Rhs as BinaryOperator;
                if (bo == null) return false;
                return bo.Lhs.Equiv(p4_ref);
            });
            if (p4_op == null) return false;
            var p4_atom = p4_op.Lhs;
            if (!p4_atom.IsAtom()) return false;
            var p4_bop = p4_op.Rhs as BinaryOperator;
            if (p4_bop == null) return false;
            if (!p4_bop.Lhs.Equiv(p4_ref)) return false;
            var p4_optype = p4_bop.OperatorType;
            var p4_any = p4_bop.Rhs;

            var p4_hoard = p4_usages.First().Stmt();
            var p4_hoard_template = new Assign(p4_ref, p4_atom);
            if (!p4_hoard.Equiv(p4_hoard_template)) return false;
            var p4_usage = p4_usages.Select(u => u.Stmt()).Except(p4_op).Last();

            var p4_opeq = p4_atom.CreateOpPostAssign(p4_optype, p4_any);
            if (p4_opeq == null) return false;

            dfa.Remove(p4_hoard, p4_op);
            dfa.ReplaceRecursive(p4_usage, p4_ref, p4_opeq); 
            return true;
        }
    }
}