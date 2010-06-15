using System;
using System.Linq;
using NUnit.Framework;
using Truesight.Parser.Api.Ops;
using XenoGears.Reflection.Generics;
using XenoGears.Reflection.Shortcuts;
using XenoGears.Assertions;

namespace Truesight.Playground.Decompiler
{
    [TestFixture, Category("Xtras To Be Implemented")]
    public class XtrasToBeImplemented : Tests
    {
        [Test]
        public void Coalesce()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("Coalesce", BF.All));
        }

        [Test]
        public void CoalesceCtor()
        {
            var stfld_mainctor = typeof(Stfld).GetConstructors(BF.All).Single(c => c.Paramc() == 3);
            TestMethodDecompilation(stfld_mainctor);
        }

        [Test]
        public void CoalesceAndConditional()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("CoalesceAndConditional", BF.All));
        }

        [Test]
        public void Enum()
        {
            var int32_tryparse = typeof(Int32).GetMethods(BF.All).AssertSingle(m => m.Name == "TryParse" && m.Paramc() == 2);
            TestMethodDecompilation(int32_tryparse);
        }

        [Test]
        public void ComplexThrow()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("ComplexThrow", BF.All));
        }

        [Test]
        public void Using()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("Using", BF.All));
        }

        [Test]
        public void Iter()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("Iter", BF.All));
        }

        [Test]
        public void Lambdas_ImmutableClosures()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("Lambdas_ImmutableClosures", BF.All));
        }

        [Test]
        public void Lambdas_MutableClosures()
        {
            throw new NotImplementedException("This test is not implemented");
        }

        [Test]
        public void ArraysAndVarargs()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("ArraysAndVarargs", BF.All));
        }

        [Test]
        public void ObjectInitsAndCollectionInits()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("ObjectInitsAndCollectionInits", BF.All));
        }

        [Test]
        public void Pointers()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("Pointers", BF.All));
        }

        [Test]
        public void Pointers2()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("Pointers2", BF.All));
        }

        [Test]
        public void PointersAndFixed()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("PointersAndFixed", BF.All));
        }

        [Test]
        public void StackAllocFib()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("StackAllocFib", BF.All));
        }

        [Test]
        public void OmitUnnecessaryCasts()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("OmitUnnecessaryCasts", BF.All));
        }

        [Test]
        public void IrregularControlFlow()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("IrregularControlFlow", BF.All));
        }
    }
}
