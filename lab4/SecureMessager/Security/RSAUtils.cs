using System;
using System.Collections.Generic;

namespace Security
{
    public class RSAUtils
    {
        private const int HighBoundary = 30000;
        private const int LowBoundary = 60000;

        public Pair SecretKey { get; private set; }
        public Pair PublicKey { get; private set; }

        public Pair[] KeyGen()
        {
            var pq = FindPandQ();

            var n = pq.X * pq.N;

            var eulerFunc = GetEulerFunction(pq.X, pq.N);

            var d = CalculateD(eulerFunc);
            var e = CalculateE(d, eulerFunc);
            
            var secretKey = new Pair(d, n);
            var publicKey = new Pair(e, n);

            SecretKey = secretKey;
            PublicKey = publicKey;

            return new[] {secretKey, publicKey};
        }


        private Pair FindPandQ()
        {
            var rnd = new Random();
            long p, q;
            do
            {
                p = rnd.Next(HighBoundary, LowBoundary);
            }
            while (!IsPrime(p));

            do
            {
                q = rnd.Next(HighBoundary, LowBoundary);
            }
            while (!IsPrime(q) || p == q);

            return new Pair(p, q);
        }
        
        private long CalculateD(long eulerFuncVal)
        {
            long d;

            for (d = 2; d < eulerFuncVal; d++)
            {
                if (GCD(d, eulerFuncVal) == 1)
                {
                    return d;
                }
            }
            return 0;
        }

        //e * d mod eulerFunc = 1 => e * d = 1 + k*eulerFunc
        private long CalculateE(long d, long eulerFuncVal)
        {
            var e = eulerFuncVal / d;
            long t = 1;
            while (((e * d) - 1) != t * eulerFuncVal)
            {
                if (t * eulerFuncVal < e * d)
                {
                    t++;
                }
                e++;
            }

            return e;
        }
        
        private long GetEulerFunction(long p, long q) => (p - 1) * (q - 1);
        
        private long GCD(long e, long d)
        {
            while (e != 0 && d != 0)
            {
                if (e > d)
                {
                    e %= d;
                }
                else
                {
                    d %= e;
                }
            }

            return e | d;
        }

        private bool IsPrime(long num)
        {
            if (num <= 1) return false;
            if (num == 2) return true;
            if (num % 2 == 0) return false;

            var sqrt = (long)Math.Floor(Math.Sqrt(num));
            for (var i = 3; i <= sqrt; i++)
            {
                if (num % i == 0)
                {
                    return false;
                }
            }

            return true;
        }

        public List<int> Encrypt(string text)
        {
            var msgArr = text.ToCharArray();

            /* for(int i = 0; i < msgArr.Length; ++i)
             {
                 var encryptedSymbol = Cryption(msgArr[i], )
             }*/

            return null;
        }
        
        static long Cryption(int a, long b, long c)
        {
            long res = a;
            long remainder = 1;
            while (true)
            {
                if (res >= c)
                {
                    res %= c;
                }
                else
                {
                    if (b % 2 != 0)
                    {
                        if (b == 1)
                        {
                            var ans = ((res % c) * (remainder % c)) % c;
                            return ans;
                        }

                        b--;
                        remainder *= res;
                        remainder %= c;
                        b /= 2;

                    }
                    else
                        b /= 2;

                    res *= res;
                }
            }
        }
    }
}