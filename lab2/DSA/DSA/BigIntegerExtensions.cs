using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading;

namespace BigPrime
{
    public static class BigIntegerExtensions
    {
        private static ThreadLocal<Random> s_Gen = new ThreadLocal<Random>(() => new Random());

        private static Random Gen => s_Gen.Value;

        public static Boolean IsProbablyPrime(this BigInteger value, int witnesses = 10)
        {
            if (value <= 1)
                return false;

            if (witnesses <= 0)
                witnesses = 10;

            var d = value - 1;
            var s = 0;

            while (d % 2 == 0)
            {
                d /= 2;
                s++;
            }

            var bytes = new Byte[value.ToByteArray().LongLength];
            BigInteger a;

            for (var i = 0; i < witnesses; i++)
            {
                do
                {
                    Gen.NextBytes(bytes);

                    a = new BigInteger(bytes);
                }
                while (a < 2 || a >= value - 2);

                var x = BigInteger.ModPow(a, d, value);
                if (x == 1 || x == value - 1)
                    continue;

                for (var r = 1; r < s; r++)
                {
                    x = BigInteger.ModPow(x, 2, value);

                    if (x == 1)
                        return false;
                    if (x == value - 1)
                        break;
                }

                if (x != value - 1)
                    return false;
            }

            return true;
        }
    }
}
