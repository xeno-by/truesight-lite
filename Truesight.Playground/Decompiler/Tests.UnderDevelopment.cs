using NUnit.Framework;
using XenoGears.Reflection.Shortcuts;

namespace Truesight.Playground.Decompiler
{
    [TestFixture, Category("Under Development")]
    public class UnderDevelopment : Tests
    {
        [Test, Category("Hot")]
        public void StructCtors()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("StructCtors", BF.All));
        }

        [Test]
        public void ComplexConditions1()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("ComplexConditions1", BF.All));
        }

        [Test]
        public void ComplexConditions2()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("ComplexConditions2", BF.All));
        }

        [Test]
        public void SizeOf()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("SizeOf", BF.All));
        }

        [Test]
        public void TypeOf()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("TypeOf", BF.All));
        }

        [Test]
        public void SimpleThrow()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("SimpleThrow", BF.All));
        }

        [Test]
        public void Conditional1()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("Conditional1", BF.All));
        }

        [Test]
        public void Conditional2()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("Conditional2", BF.All));
        }
    }
}
