using NUnit.Framework;
using XenoGears.Reflection.Shortcuts;

namespace Truesight.Playground.Decompiler
{
    [TestFixture, Category("Under Development")]
    public class UnderDevelopment : Tests
    {
        [Test]
        public void SimpleThrow()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("SimpleThrow", BF.All));
        }

        [Test]
        public void TypeOf()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("TypeOf", BF.All));
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
