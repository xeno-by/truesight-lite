using System;
using System.Collections.Generic;
using System.IO;
using XenoGears.Exceptions;
using XenoGears.Logging;
using XenoGears.Reflection;
using System.Linq;
using XenoGears.Strings;

namespace Truesight.Playground.Decompiler
{
    internal static class Snippets
    {
        public static int Operators1(int a, byte b, short c)
        {
            return (a - c + b * b) / a;
        }

        public static ulong Operators2(int a, sbyte b, short c)
        {
            return (ulong)(a - c + b * b) / (uint)a;
        }

        public static bool OmitUnnecessaryCasts(uint a, byte b, short c)
        {
            return ((a | b) == (a & c)) && (a == b + c);
        }

        public class Cell
        {
            public float Tau; 
            public float Ux; 
            public float Uz;
        }

        public static void ExoticOperators1(float a, Cell v)
        {
            v.Ux += a;
        }

        internal class P { public virtual C InnerC() { return new C(); } }
        internal class C 
        { 
            public double f;
            public double p { get; set; }
            public double this[int i]
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }
        }

        public static void ExoticOperators2(double a, P p)
        {
            p.InnerC().f += (p.InnerC().p /= p.InnerC()[(int)++a]++);
        }

        public static void ExoticOperators3(float a, float? b, Cell v)
        {
            a %= (int)v.Tau;
            v.Ux += (a /= v.Ux);
            v.Tau = (int)a-- ^ (int)v.Uz;
            v.Uz -= a;
            v.Ux += (v.Ux /= a);
        }

        public static int PreAndPost_VerySimple(B a, B b)
        {
            if (++a.f > b.f++) return 0;
            return 1;
        }

        public static int PreAndPost_MoreOrLessSimple(int a, int b)
        {
            if (++a * (b += 2) > --b * a++) return 0;
            return a - b;
        }

        internal class B { public int f; }
        public static int PreAndPost_WithFields(int a, B b)
        {
            if (++a * (b.f += 2) > --b.f * a++) return 0;
            return a - b.f;
        }

        internal interface A { int prop { get; set; } int this[int idx] { get; set; } }
        public static int PreAndPost_WithPropsAndIndexers(A a, int b)
        {
            if (++a.prop * (a.prop += 2) > --b * a[b]++) return 0;
            return a.prop - b;
        }

        public class Va
        {
            public int[] Ints { get { throw new NotImplementedException(); } }
            public int Vararg(params int[] va) { throw new NotImplementedException(); }
        }

        public static void ArraysAndVarargs()
        {
            var regular = new int[2];
            var init1 = new []{1, 2, 3};
            var init2 = new []{new Va(), new Va()};
            init2[0].Vararg(1, init2[1].Vararg(init2[0].Ints));
            var i = new Va().Vararg(init1);
            init2[1].Vararg(regular[i], i);
        }

        internal class L
        {
            public virtual int Virtual(int a, int b) { throw new NotImplementedException(); }
            public void NonVirtual() { throw new NotImplementedException(); }
        }

        public static int Lambdas_ImmutableClosures(int a, L l)
        {
            Func<int, int> lambda = i =>
            {
                Func<int, int, int> virt1 = l.Virtual;
                Func<int, int> virt2 = b => l.Virtual(a, b);
                Func<int> virt3 = () => l.Virtual(a, 0);
                Action virt4 = () => l.Virtual(a, 0);
                Action nonVirt = l.NonVirtual;

                return virt1(a, i) + virt2(i) + virt3() + ((Func<Func<int>, Func<int>>)(f =>
                {
                    Action deferred = () => { virt4(); nonVirt(); };
                    return () => { deferred(); return i * l.Virtual(a, f() * virt3()); };
                }))(virt3)();
            };

            Func<int> rnd = () => new Random((int)DateTime.Now.Ticks).Next();
            return lambda(a) * rnd();
        }

        public static void SomeControlFlow(int a, int b, int c)
        {
            for (var i = 0; i < --a; ++i)
            {
                while (++b >= --c) i++;
                if (--a < i++) return;
                while (true) if (a++ > 10) break;
                do { c += --a; } while (++b > c++);
            }
        }

        public static void SomeControlFlow2(int a, int b, int c)
        {
            for (var x = 0; x < 10; x += 1)
            {
                if (x > b) { Console.WriteLine("for-for-continue"); continue; }
                x += 2;
                if (x < b) { Console.WriteLine("for-for-break"); break; }
                x -= 2;
            }
        }

        public static void ComplexConditions1(bool a, bool b)
        {
            var pap = a && b;
            var pan = a && !b;
            var nap = !a && b;
            var nan = !a && !b;
            var pop = a || b;
            var pon = a || !b;
            var nop = !a || b;
            var non = !a || !b;
        }

        public static void ComplexConditions2(bool a, bool b, bool c)
        {
            var papap = (a && b) && c;
            var papan = (a && b) && !c;
            var panap = (a && !b) && c;
            var panan = (a && !b) && !c;
            var napap = (!a && b) && c;
            var napan = (!a && b) && !c;
            var nanap = (!a && !b) && c;
            var nanan = (!a && !b) && !c;

            var papop = (a && b) || c;
            var papon = (a && b) || !c;
            var panop = (a && !b) || c;
            var panon = (a && !b) || !c;
            var napop = (!a && b) || c;
            var napon = (!a && b) || !c;
            var nanop = (!a && !b) || c;
            var nanon = (!a && !b) || !c;

            var popap = (a || b) && c;
            var popan = (a || b) && !c;
            var ponap = (a || !b) && c;
            var ponan = (a || !b) && !c;
            var nopap = (!a || b) && c;
            var nopan = (!a || b) && !c;
            var nonap = (!a || !b) && c;
            var nonan = (!a || !b) && !c;

            var popop = (a || b) || c;
            var popon = (a || b) || !c;
            var ponop = (a || !b) || c;
            var ponon = (a || !b) || !c;
            var nopop = (!a || b) || c;
            var nopon = (!a || b) || !c;
            var nonop = (!a || !b) || c;
            var nonon = (!a || !b) || !c;
        }

        public static int ComplexConditions3(int a, int b, int c)
        {
            if (
                    (
                        (a == b && a == c && b == c) ||
                        (b * c == 0 && a == b) ||
                        (a - b == b - c || a + b == b - c || a == c)
                    )
                    &&
                    (
                        a * b * c > 0
                    )
               )
            {
                return a / b;
            }
            else
            {
                return 42;
            }
        }

        public static int ComplexControlFlow(int a, int b, int c)
        {
            for (var i = 0; i < a * b || b < a; /*nothing here*/)
            {
                if (a * b * c > 0)
                {
                    Console.WriteLine("for-if");

                    var k = 0;
                    do
                    {
                        k++;
                        if (k == a++ + c) return a / b;
                        if (k-- > a + c) continue;
                        Console.WriteLine("for-if-dowhile");
                    }
                    while (k < b + c);

                    if (k + i == c) break;
                }

                while (i - c > b / a)
                {
                    Console.WriteLine("for-while");
                    i += a;
                }

                for (var z = 0;;z += 10)
                {
                    if (z > 101) break;
                }

                if (i > a * b * c)
                {
                    while (a < 2 / c)
                    {
                        i++;
                        if (a == i) break;
                    }
                }

                for (var x = 0; x < 10; x += 1)
                {
                    if (x > b) { Console.WriteLine("for-for-continue"); continue; }
                    x += 2;
                    if (x < b) { Console.WriteLine("for-for-break"); break; }
                    x -= 2;
                }

                if (i + b * c == a - b)
                {
                    i += a;
                    if (i > b) { Console.WriteLine("for-if-iftrue-continue"); continue; }
                    else { Console.WriteLine("for-if-iffalse-nocontinue"); }
                    i -= a;
                }

                if (a == b)
                {
                    if (i > b) { Console.WriteLine("for-if-iftrue-return"); return 10; }
                    else { Console.WriteLine("for-if-iffalse-break"); break; }
                }

                for (var j = i; j < b++ * ++c && --i < a--; j++)
                {
                    Console.WriteLine("for-for");
                    if (i + j == a - c) continue;
                    if (i * j == c) return 1 + j * a;

                    do
                    {
                        ++j;
                        Console.WriteLine("for-for-dowhile");
                    }
                    while (j < a / c);

                    if (i / j == a + b) { Console.WriteLine("for-for-iftrue-endofbody"); }
                    else { i += j; Console.WriteLine("for-for-iffalse-endofbody"); }
                }
            }

            var m = 5;
            while (true)
            {
                Console.WriteLine("while-true");
                if (m > 2) m--;
                else break;
            }

            var d = a + b + c * c;
            return 2 * m + d;
        }

        public static int IrregularControlFlow(int a, int b, int c)
        {
            if (a > b)
            {
                Foo: for (var i = a; i < c; ++i)
                {
                    for (var j = b; j < a; ++j)
                    {
                        a += c;
                        if (i + a > b) goto Foo;
                        b *= (i + j);
                        if (a - b > i) goto Bar;
                    }

                    a = Math.Min(b, c);
                    i += c / a;
                }
            }

            Bar: return a * b;
        }

        internal class R { public int x; public int[] xar = new int[0]; }
        internal struct V { public float x; public int[] xar; public V(int[] xar) : this() { this.xar = xar; } }

        unsafe public static void Pointers(DateTime* x)
        {
            *x = DateTime.Now;
            var foo = x->Day;
            var bar = x->IsDaylightSavingTime();
            var qux = (*x).Day;
            PointersAux(*x);
        }

        unsafe public static void PointersAux(DateTime x)
        {
            Pointers(&x);
        }

        unsafe public static void Pointers2(ref int i, ref int j)
        {
            fixed (int* pi = &i)
            {
                i = pi[42];
                j = *pi;
            }
        }

        unsafe public static void PointersAndFixed(int[] ints, R r, V v)
        {
            fixed (int* p_ints = ints)
            {
                var index = 10 * (int)DateTime.Now.Ticks;
                p_ints[2 * index] = 0;
                *(p_ints + index) = 10;

                fixed (int* p_rx = &r.x)
                {
                    float* p_vx = &v.x;
                    fixed (int* p_rxar = r.xar, p_vxar = v.xar)
                    {
                        p_rxar[10] += *p_rx + p_vxar[*p_rx] * *((int*)p_vx);
                    }
                }
            }
        }

        unsafe public static void StackAllocFib()
        {
            const int arraySize = 20;
            int* fib = stackalloc int[arraySize];
            int* p = fib;
            *p++ = 1;
            for (int i = 2; i < arraySize; ++i, ++p)
            {
                // Sum the previous two numbers.
                *p = p[-1] + p[-2];
            }
            for (int i = 0; i < arraySize - 1; ++i)
            {
                Console.WriteLine(fib[i]);
            }
        }

        internal class R1
        {
            public int @int;
            public int[] ints1d = new int[0];
            public int[,] ints2d = new int[0, 0];
            public R1 r;
            public V1 v;
            public static R1 operator +(R1 x, R1 y) { return new R1(); }
            public TypeCode GetTypeCode() { return TypeCode.Object; }
        }

        internal struct V1
        {
            public TypeCode GetTypeCode() { return TypeCode.Object; }
            public static V1 operator +(V1 x, V1 y) { return new V1(); }
        }

        public static void ByRefCall(R1 r, int a_int)
        {
            var l_int = 0;
            ByRefUse(ref a_int);
            ByRefUse(ref l_int);
            ByRefUse(ref r.@int);
            ByRefUse(ref r.ints1d[0]);
            ByRefUse(ref r.ints2d[0, 0]);
            ByRefUse(ref r.r.@int);
            ByRefUse(ref r.r.ints1d[0]);
            ByRefUse(ref r.r.ints2d[0, 0]);
            ByRefUse(ref r.v);
            ByRefUse(ref r);
            ByRefUse(ref r.r);
        }

        public static void ByRefUse(ref int i)
        {
            Console.WriteLine(i.ToString());
            Console.WriteLine(i.GetTypeCode());
            var li = i;
            var li2 = li;
            Console.WriteLine(li.ToString());
            Console.WriteLine(li.GetTypeCode());
            i = li2 + i;
            ByRefUse(ref i);
        }

        public static void ByRefUse(ref V1 v)
        {
            Console.WriteLine(v.ToString());
            Console.WriteLine(v.GetTypeCode());
            var lv = v;
            var lv2 = lv;
            Console.WriteLine(lv.ToString());
            Console.WriteLine(lv.GetTypeCode());
            v = lv2 + v;
            ByRefUse(ref v);
        }

        public static void ByRefUse(ref R1 r)
        {
            Console.WriteLine(r.ToString());
            Console.WriteLine(r.GetTypeCode());
            var lr = r;
            var lr2 = lr;
            Console.WriteLine(lr.ToString());
            Console.WriteLine(lr.GetTypeCode());
            r = lr2 + r;
            ByRefUse(ref r);
        }

        public static void StructByRef(ref DateTime x)
        {
            x = DateTime.Now;
            var foo = x.Ticks - DateTime.Now.Ticks;
            var bar = x.IsDaylightSavingTime();
            StructByRefAux(x);
        }

        public static void StructByRefAux(DateTime x)
        {
            StructByRef(ref x);
        }

        internal class RDateTime
        {
            public static RDateTime Now { get { return new RDateTime(); } }
            public long Ticks { get { throw new NotImplementedException(); } }
            public bool IsDaylightSavingTime() { throw new NotImplementedException(); }
        }

        public static void ClassByRef(ref RDateTime x)
        {
            x = RDateTime.Now;
            var foo = x.Ticks - RDateTime.Now.Ticks;
            var bar = x.IsDaylightSavingTime();
            ClassByRefAux(x);
        }

        public static void ClassByRefAux(RDateTime x)
        {
            ClassByRef(ref x);
        }

        private struct SStatic { public int i; public int pi; }
        unsafe private struct SDynamic { public int i; public int* pi; }

        unsafe public static bool SizeOf()
        {
            var @static = sizeof(int) + sizeof(IntPtr) == sizeof(SStatic);
            var dynamic = sizeof(int) + sizeof(IntPtr) == sizeof(SDynamic);
            return @static && dynamic;
        }

        public static bool TypeIs(Object o)
        {
            return o is Type || o is SDynamic;
        }

        public static int TypeAs(Object o, String sub)
        {
            var str = o as String;
            if (str != null)
            {
                return str.IndexOf(sub);
            }
            else
            {
                return -1;
            }
        }

        public static bool TypeOf<T>(Object o)
        {
            var sameType = o.GetType() == typeof(Func<,,>);
            var sameToken = o.GetType().SameMetadataToken(typeof(Func<T, int>));
            return sameType || sameToken;
        }

        public static int Conditional1(int a, int b)
        {
            return a >= b && a <= b ? a + Conditional1(a, b) : b - a;
        }

        public static int Conditional2(int a, int b, int c)
        {
            var d = Conditional2(a, a >= b && a <= b ? a + Conditional1(a, b) : b - a, c) > 2 ? a : b;
            var e = Conditional1(a + (a * b > 10 ? 10 : a * b), b);
            var f = Conditional2(a, b, a >= b && a <= b ? a + Conditional1(a, b) : b - a);
            return Conditional2(d * e * f > b ? a : b, a >= b && a <= b ? a + Conditional1(a, b) : b - a, c) > 2 ? a : b;
        }

        public static int Coalesce(int a, int b, int? c)
        {
            return a * (c ?? b);
        }

        public static int CoalesceAndConditional(int a, int b, int? c)
        {
            for (var i = 0; i < a; i += b)
            {
                if (a > b) { return a < 2 * b ? c ?? 42 : a + b; }
                if (c.HasValue && (a < c.Value ? true : 2 * a > b)) break;
                b = a * c ?? 15;
            }

            return a > 12 ? 10 : a < 15 ? 20 : 30 + c ?? c.Value;
        }

        internal class OI
        {
            public int i;
            public int j { get; set; }
            public DateTime dt { get; set; }
            public OI oi { get; set; }
            public List<OI> ois { get; set; }

            public OI(){}
            public OI(int i, int j)
            {
                this.i = i;
                this.j = j;
            }
        }

        public static OI ObjectInitsAndCollectionInits()
        {
            return new OI(1, 2)
            {
                i = 2,
                j = 1,
                dt = new DateTime(2010, 06, 10),
                oi = {i = 1, j = 2, oi = {i = 2, j = 3}, ois = {}},
                ois = {new OI{i = 5, j = 7}},
            };
        }

        public static int SimpleThrow(int a)
        {
            if (a > 5) throw new ArgumentOutOfRangeException("a", String.Format("Failboat: {0} > 5", a));
            else
            {
                return 5 - a;
            }
        }

        public static void ComplexThrow(int a, int b, int c)
        {
            try
            {
                Using(String.Format("{0} + {1} / {2} = {3}", a, b, c, a + b / c));
            }
            catch (InvalidOperationException iox)
            {
                try
                {
                    Using(iox.ToString());
                }
                catch (Exception ex)
                {
                    throw new ExceptionWhenHandlingException(iox, ex);
                }
                finally
                {
                    Log.WriteLine("Finally in IOX handler");
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex);
                throw;
            }
        }

        public static void Using(String msg)
        {
            using (var f = new FileStream("foo.txt", FileMode.CreateNew))
            {
                using (var w = new StreamWriter(f))
                {
                    var lines = msg.SplitLines();
                    w.WriteLine("Total lines: {0}", lines.Count());
                    for (var i = 0; i < lines.Count(); ++i)
                    {
                        w.WriteLine(lines[i]);
                    }
                }
            }
        }

        public static int Iter<T>(IEnumerable<T> seq)
        {
            var codes = 0;
            var eqc = EqualityComparer<T>.Default;

            var rev_index = 0;
            foreach (var rev_el in seq.Reverse())
            {
                var el_index = 0;
                foreach (var el in seq)
                {
                    codes += eqc.GetHashCode(el) * el_index + rev_index;
                }

                codes += rev_index;
            }

            return codes;
        }

        internal struct S
        {
            public int x;

            public S(int x)
            {
                this.x = x;
            }
        }

        public static void StructOpAss(S p)
        {
            var l = new S();
            p.x += l.x += 2;
        }

        public static bool StructCtors()
        {
            var def = new S();
            var ctor = new S(10);
            return def.x + new S(9).x < new S().x + ctor.x;
        }

        public static bool Default<T>()
        {
            var i32_def = default(int);
            var t_def = default(T);
            var f64_def = default(double);
            var s_def = default(S);
            return t_def.GetType().Name.Length > (i32_def + (int)f64_def + s_def.x);
        }
    }
}