using System;
using System.Collections.Generic;
using Truesight.Parser.Api;
using Truesight.Parser.Impl;
using XenoGears.Assertions;
using XenoGears.Logging;

namespace Truesight.Playground.Parser
{
    internal static class Snippets
    {
        public static void SmallSnippet(int a, double b)
        {
            if (b < (a + 15))
            {
                var s = ((b / a).ToString() + "str").ToString();
                checked
                {
                    var x = a + 10;

                    switch (x)
                    {
                        case 2:
                            var y = (UInt32)4 + (UInt32)x;
                            Log.WriteLine(String.Empty + x + y);
                            break;

                        case 4:
                            a = 15;
                            var arr = new Type[1];
                            arr[0] = typeof(Single);
                            break;

                        case 7:
                            Object abox = a;
                            b = (Int32)(UInt32)abox + 5;
                            var c = (UInt32)b < 2;
                            break;

                        default:
                            var z = typeof(Type);
                            Log.WriteLine(z.ToString() + sizeof(Double));
                            throw new NotSupportedException();
                    }
                }
            }
            else
            {
                DateTime dt1;
                dt1 = DateTime.Now;
                var dt = new DateTime(1987, 8, 5);
                var arr = new[]{dt, new DateTime(dt1.Year, dt1.Month, dt1.Day)};
                (arr.Length == new List<Int32>().Count).AssertTrue();

                Func<int, int> foo = i => dt.Day + i + arr[15].Month;
                (foo(arr.Length) != 5.0d).AssertTrue();
                arr[4] = dt;

                var dts = new List<DateTime>();
                var ss = dts[0].ToString();

                IILOp iilop = null;
                (iilop is ILOp && new []{"hello", "world"}[0] == "hello").AssertFalse();
            }
        }

        public static void SmallSnippetWithoutSwitch(int a, double b)
        {
            if (b < (a + 15))
            {
                var s = ((b / a).ToString() + "str").ToString();
                checked
                {
                    var x = a + 10;

                    var y = (UInt32)4 + (UInt32)x;
                    Log.WriteLine(String.Empty + x + y);

                    a = 15;
                    var arr = new Type[1];
                    arr[0] = typeof(Single);

                    Object abox = a;
                    b = (Int32)(UInt32)abox + 5;
                    var c = (UInt32)b < 2;

                    var z = typeof(Type);
                    Log.WriteLine(z.ToString() + sizeof(Double));
                    throw new NotSupportedException();
                }
            }
            else
            {
                DateTime dt1;
                dt1 = DateTime.Now;
                var dt = new DateTime(1987, 8, 5);
                var arr = new[]{dt, new DateTime(dt1.Year, dt1.Month, dt1.Day)};
                (arr.Length == new List<Int32>().Count).AssertTrue();

                Func<int, int> foo = i => dt.Day + i + arr[15].Month;
                (foo(arr.Length) != 5.0d).AssertTrue();
                arr[4] = dt;

                var dts = new List<DateTime>();
                var ss = dts[0].ToString();

                IILOp iilop = null;
                (iilop is ILOp && new []{"hello", "world"}[0] == "hello").AssertFalse();
            }
        }

        public static void SnippetWithGuards()
        {
            Int64 local;

            try
            {
                local = int.Parse("1");
            }
            catch (FormatException)
            {
                local = 2;
                try { }
                finally { }
            }
            catch (Exception)
            {
                local = 3;
                throw;
            }
            finally
            {
                try
                {
                    local = 4;
                }
                catch (Exception)
                {
                    local = 5;
                    throw;
                }
            }

            try
            {
                local = local * 10;
            }
            finally
            {
                Log.WriteLine(local);
            }
        }
    }
}
