using System;
using System.Collections.Generic;
using System.Linq;

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

            var eulerFunc = GetEulerFunction((long) pq.X, (long) pq.N);

            var d = CalculateD(eulerFunc);
            var e = CalculateE(d, eulerFunc);
            
            var secretKey = new Pair((ulong)d, n);
            var publicKey = new Pair((ulong)e, n);

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

            return new Pair((ulong) p, (ulong) q);
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

        public List<ulong> Encrypt(string text, ulong e, ulong n)
        {
            var msgArr = text.ToCharArray();
            var cipher = new List<ulong>();
            
            for(var i = 0; i < msgArr.Length; ++i)
            {
                var encryptedSymbol = Cryption(msgArr[i], e, n);
                cipher.Add(encryptedSymbol);
            }

            return cipher;
        }
        
        public string Decrypt(List<ulong> cipher)
        {
            var encryptedMsgArr = cipher.ToArray();
            var decryptedCharArr = new char[cipher.Count];

            var decryptedArr = encryptedMsgArr
                .Select(t => Cryption(t, SecretKey.X, SecretKey.N))
                .Select(decryptedSymbol => (int) decryptedSymbol).ToList();

            for (var i = 0; i < decryptedArr.Count; ++i)
            {
                decryptedCharArr[i] = (char) decryptedArr[i];
            }

            /*for (int i = decryptedCharArr.Length - 1, j = 0; i >= 0; --i, j++)
            {
                decryptedCharArr[j] = (char) decryptedArr[i];
            }*/
            
            return new string(decryptedCharArr);
        }
        
        private ulong Cryption(ulong a, ulong b, ulong c)
        {
            ulong res = a;
            ulong remainder = 1;
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