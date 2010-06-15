using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Truesight.Decompiler.Hir;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Decompiler.Pipeline.Flow.Cfg;
using System.Linq;
using XenoGears.Functional;
using XenoGears.Assertions;
using Truesight.Decompiler.Pipeline.Cil.Common;
using Truesight.Decompiler.Hir.Traversal;

namespace Truesight.Decompiler.Pipeline.Cil.OpAssign
{
    [DebuggerNonUserCode]
    internal class DfaHelper
    {
        public double ExecOrderOfStmt(Node anyNode)
        {
            return _execOrders[anyNode.Stmt()];
        }

        public ReadOnlyCollection<Expression> Atoms()
        {
            return _atoms.ToReadOnly();
        }

        public ReadOnlyCollection<Expression> Usages(Node atom)
        {
            return _usages[atom.AssertCast<Expression>()].ToReadOnly();
        }

        public ReadOnlyCollection<Expression> Reads(Node atom)
        {
            return _usages[atom.AssertCast<Expression>()].Where(u => !(u is Assign)).ToReadOnly();
        }

        public ReadOnlyCollection<Assign> Writes(Node atom)
        {
            return _usages[atom.AssertCast<Expression>()].OfType<Assign>().ToReadOnly();
        }

        #region Implementation details

        private readonly ControlFlowGraph _cfg;
        private readonly List<ControlFlowBlock> _vertices = new List<ControlFlowBlock>();
        private readonly Dictionary<Node, double> _execOrders = new Dictionary<Node, double>();
        private readonly HashSet<Expression> _atoms = new HashSet<Expression>(Node.EquivComparer<Expression>());
        private readonly Dictionary<Expression, List<Expression>> _usages =
            new Dictionary<Expression, List<Expression>>(Node.EquivComparer<Expression>());

        public DfaHelper(ControlFlowGraph cfg)
        {
            _cfg = cfg;
            // todo. use Cflow instead for being correct with exec order
            _vertices = cfg.Vertices.ToList();

            var allStmts = _vertices.SelectMany(cfb => 
                cfb.BalancedCode.Concat(cfb.Residue.Cast<Node>())).ToReadOnly();
            allStmts.ForEach((stmt, i) => PutIntoCache(stmt, i));
        }

        private void PutIntoCache(Node stmt)
        {
            double? index = null;
            _vertices.TakeWhile(_ => index == null).ForEach((cfb, i) =>
            {
                var united = cfb.BalancedCode.Concat(cfb.Residue.Cast<Node>()).ToReadOnly();
                var iof = united.IndexOf(stmt);
                if (iof == -1) return;

                Node before = null;
                if (iof > 0) before = united[iof - 1];
                else
                {
                    before = (i - 1).DownTo(0).Select(j => _vertices[j].Residue.LastOrDefault() ??
                        _vertices[j].BalancedCode.LastOrDefault()).Where(n => n != null).FirstOrDefault();
                }

                Node after = null;
                if (iof < united.Count() - 1) after = united[iof + 1];
                else
                {
                    after = (i + 1).UpTo(_vertices.Count() - 1).Select(j => _vertices[j].BalancedCode.FirstOrDefault() ?? 
                        _vertices[j].Residue.FirstOrDefault()).Where(n => n != null).FirstOrDefault();
                }

                var i_before = before == null ? _execOrders.Values.MinOrDefault() - 1 : _execOrders[before];
                var i_after = after == null ? _execOrders.Values.MaxOrDefault() + 1 : _execOrders[after];
                index = (i_before + i_after) / 2;
            });

            index.AssertNotNull();
            PutIntoCache(stmt, index.Value);
        }

        private void PutIntoCache(Node stmt, double execOrder)
        {
            _execOrders.Add(stmt, execOrder);

            // todo. use Family instead of ChildrenRecursive and make this not crash
            var usages = stmt.ChildrenRecursive().Where(n => n.IsAtom()).ToReadOnly();
            foreach (Expression u in usages)
            {
                _atoms.Add(u);
                var cache = _usages.GetOrCreate(u, () => new List<Expression>());
                var ins = cache.TakeWhile((u2, i) => _execOrders[u2.Stmt()] <= execOrder).Count();

                var isAssignToAtom = u.Parent is Assign && ((Assign)u.Parent).Lhs == u;
                cache.Insert(ins, isAssignToAtom ? u.Parent.AssertCast<Assign>() : u);
            }
        }

        private void EvictFromCache(Node stmt)
        {
            _execOrders.Remove(stmt);

            // todo. use Family instead of ChildrenRecursive and make this not crash
            var atoms = stmt.ChildrenRecursive().Where(n => n.IsAtom()).Distinct(Node.EquivComparer()).ToReadOnly();
            foreach (Expression atom in atoms)
            {
                _usages[atom].RemoveElements(e => e.Stmt() == stmt);
                if (_usages[atom].IsEmpty()) _atoms.Remove(atom);
            }
        }

        public void Remove(params Node[] toDelete)
        {
            _cfg.Remove(toDelete);
            toDelete.ForEach(EvictFromCache);
        }

        public void Remove(IEnumerable<Node> toDelete)
        {
            _cfg.Remove(toDelete);
            toDelete.ForEach(EvictFromCache);
        }

        public void Remove(Node toDelete)
        {
            _cfg.Remove(toDelete);
            EvictFromCache(toDelete);
        }

        public void Replace(Node replacee, Node replacer)
        {
            _cfg.Replace(replacee, replacer);
            EvictFromCache(replacee);
            PutIntoCache(replacer);
        }

        public void ReplaceRecursive(Node root, Node find, Node replace)
        {
            EvictFromCache(root.Stmt());
            root.ReplaceRecursive(find, replace);
            PutIntoCache(root.Stmt());
        }

        public void ReplaceRecursive(Node root, Func<Node, bool> find, Node replace)
        {
            EvictFromCache(root.Stmt());
            root.ReplaceRecursive(find, replace);
            PutIntoCache(root.Stmt());
        }

        public void ReplaceRecursive(Node root, Node find, Func<Node, Node> replace)
        {
            EvictFromCache(root.Stmt());
            root.ReplaceRecursive(find, replace);
            PutIntoCache(root.Stmt());
        }

        public void ReplaceRecursive(Node root, Func<Node, bool> find, Func<Node, Node> replace)
        {
            EvictFromCache(root.Stmt());
            root.ReplaceRecursive(find, replace);
            PutIntoCache(root.Stmt());
        }

        #endregion
    }
}
