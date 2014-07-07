using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Monads
    {
        public void main()
        {
            var x = Compose<double, double, double>(sin, cube);
        }

        public Func<T, V> Compose<T, U, V>(Func<U, V> f, Func<T, U> g)
        {
            return x => f(g(x));
        }

        public double sin(double x)
        {
            return Math.Sin(x);
        }

        public double cube(double x)
        {
            return x * x * x;
        }

        async static Task<int> AddOne(Task<int> task)
        {
            int unwrapped = await task;
            int result = unwrapped + 1;
            return result;
        }

        public static void Test()
        {
            var x1 = Tainted<int>.Bind(Tainted<int>.MakeTainted(5), y => { return Tainted<int>.MakeTainted(y); });
            var x2 = Tainted<int>.Bind(Tainted<int>.MakeClean(5), y => { return Tainted<int>.MakeClean(y); });
            var x3 = Tainted<int>.MakeTainted(5);
        }

        static Func<T> CreateSimpleOnDemand<T>(T t)
        {
            return () => t;
        }

        static Func<R> ApplySpecialFunction<A, R>(
          Func<A> onDemand,
          Func<A, Func<R>> function)
        {
            return () => function(onDemand())();
        }
    }

    struct Tainted<T>
    {
        public T Value { get; private set; }
        public bool IsTainted { get; private set; }
        private Tainted(T value, bool isTainted)
            : this()
        {
            this.Value = value;
            this.IsTainted = isTainted;
        }
        public static Tainted<T> MakeTainted(T value)
        {
            return new Tainted<T>(value, true);
        }
        public static Tainted<T> MakeClean(T value)
        {
            return new Tainted<T>(value, false);
        }
        public static Tainted<R> Bind<A, R>(
          Tainted<A> tainted, Func<A, Tainted<R>> function)
        {
            Tainted<R> result = function(tainted.Value);
            if (tainted.IsTainted && !result.IsTainted)
                return new Tainted<R>(result.Value, true);
            else
                return result;
        }
    }
}
