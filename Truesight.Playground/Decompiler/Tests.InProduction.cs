using System.Linq;
using NUnit.Framework;
using Truesight.Decompiler.Hir.Prettyprint;
using XenoGears.Assertions;
using XenoGears.Reflection.Generics;
using XenoGears.Reflection.Shortcuts;

namespace Truesight.Playground.Decompiler
{
    [TestFixture, Category("In Production")]
    public class InProduction : Tests
    {
        [Test]
        public void Operators1()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("Operators1", BF.All));
        }

        [Test]
        public void Operators2()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("Operators2", BF.All));
        }

        [Test]
        public void ExoticOperators1()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("ExoticOperators1", BF.All));
        }

        [Test]
        public void ExoticOperators2()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("ExoticOperators2", BF.All));
        }

        [Test]
        public void ExoticOperators3()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("ExoticOperators3", BF.All));
        }

        [Test]
        public void PreAndPost_VerySimple()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("PreAndPost_VerySimple", BF.All));
        }

        [Test]
        public void PreAndPost_MoreOrLessSimple()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("PreAndPost_MoreOrLessSimple", BF.All));
        }

        [Test]
        public void PreAndPost_WithFields()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("PreAndPost_WithFields", BF.All));
        }

        [Test]
        public void PreAndPost_WithPropsAndIndexers()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("PreAndPost_WithPropsAndIndexers", BF.All));
        }

        [Test]
        public void ComplexConditions3()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("ComplexConditions3", BF.All));
        }

        [Test]
        public void SomeControlFlow()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("SomeControlFlow", BF.All));
        }
        [Test]
        public void SomeControlFlow2()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("SomeControlFlow2", BF.All));
        }

        [Test]
        public void ComplexControlFlow()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("ComplexControlFlow", BF.All));
        }

        [Test]
        public void DecompileCtor_This()
        {
            var itw_notab = typeof(IndentedTextWriter).GetConstructors(BF.All).Single(ctor => ctor.Paramc() == 1);
            TestMethodDecompilation(itw_notab);
        }

        [Test]
        public void DecompileCtor_Base()
        {
            var itw_tab = typeof(IndentedTextWriter).GetConstructors(BF.All).Single(ctor => ctor.Paramc() == 2);
            TestMethodDecompilation(itw_tab);
        }

        [Test]
        public void ByRefCall()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("ByRefCall", BF.All));
        }

        [Test]
        public void ByRefUse_Int32()
        {
            var byrefuse_int32 = typeof(Snippets).GetMethods(BF.All).AssertSingle(m => m.Name == "ByRefUse" && m.Param(0).GetElementType() == typeof(int));
            TestMethodDecompilation(byrefuse_int32);
        }

        [Test]
        public void ByRefUse_Value()
        {
            var byrefuse_value = typeof(Snippets).GetMethods(BF.All).AssertSingle(m => m.Name == "ByRefUse" && m.Param(0).GetElementType() == typeof(Snippets.V1));
            TestMethodDecompilation(byrefuse_value);
        }

        [Test]
        public void ByRefUse_Ref()
        {
            var byrefuse_ref = typeof(Snippets).GetMethods(BF.All).AssertSingle(m => m.Name == "ByRefUse" && m.Param(0).GetElementType() == typeof(Snippets.R1));
            TestMethodDecompilation(byrefuse_ref);
        }

        [Test]
        public void StructByRef()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("StructByRef", BF.All));
        }

        [Test]
        public void ClassByRef()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("ClassByRef", BF.All));
        }

        [Test]
        public void TypeIs()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("TypeIs", BF.All));
        }

        [Test]
        public void TypeAs()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("TypeAs", BF.All));
        }

        [Test]
        public void StructOpAss()
        {
            TestMethodDecompilation(typeof(Snippets).GetMethod("StructOpAss", BF.All));
        }
    }
}
