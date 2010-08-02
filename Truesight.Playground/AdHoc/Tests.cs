using NUnit.Framework;
using Truesight.Decompiler.Hir.Core.Expressions;
using Truesight.Playground.InAction.Kernels;
using XenoGears.Reflection.Shortcuts;
using Truesight.Decompiler;
using XenoGears.Assertions;
using Truesight.Decompiler.Hir.TypeInference;

namespace Truesight.Playground.AdHoc
{
    [TestFixture]
    public class Tests
    {
        [Test, Category("Hot")]
        public void TypeInferenceForIntegerArithmetic()
        {
            var mmk = typeof(MatMulKernel).GetMethod("RunKernel", BF.AllInstance | BF.DeclOnly);
            var blk = mmk.Decompile().Body;
            var ass = blk[0].AssertCast<Assign>();

            (ass.Lhs.Type() == typeof(int)).AssertTrue();
            (ass.Rhs.Type() == typeof(int)).AssertTrue();
        }
    }
}
