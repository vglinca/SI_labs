using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using BigPrime;

// ReSharper disable MemberCanBePrivate.Global

namespace DSA
{
    public class Dsa
    {
        public BigInteger P { get; private set; }
        public BigInteger Q { get; private set; }
        public BigInteger G { get; private set; }
        public BigInteger X { get; private set; }
        public BigInteger Y { get; private set; }
        public BigInteger K { get; private set; }
        public BigInteger R { get; private set; }
        public BigInteger S { get; private set; }

        public Dsa()
        {
            R = BigInteger.Zero;
            S = BigInteger.Zero;
            Q = BigInteger.Zero;
        }
        
        public void GenerateKeys()
        {
            FindQ();
            Console.WriteLine($"q = {Q}");
            Console.WriteLine();
			
            FindP();
            Console.WriteLine($"p = {P}");
            Console.WriteLine();
			
            FindG();
            Console.WriteLine($"g = {G}");
            Console.WriteLine();
			
            X = GetRandomNum(Q);
            Console.WriteLine($"Private key is: x = {X}");
            Console.WriteLine();
			
            Y = FindY(G, X, P);
            Console.WriteLine($"Public key is: y = {Y}");
            Console.WriteLine();
			
            K = GetRandomNum(Q);
            Console.WriteLine($"k = {K}");
            Console.WriteLine();
        }

        private void FindQ()
        {
            Q = GetPrimeWithin(159, 160);
        }
        
        private void FindP()
        {
            if (Q == BigInteger.Zero)
            {
               FindQ(); 
            }
            
            var l = GetPow();
            var low = BigInteger.Pow(2, l - 1);
            var high = BigInteger.Pow(2, l);
            var p = low + 1;

            var k = BigInteger.DivRem(low, Q, out _);

            while (k * Q + 1 < high)
            {
                var tmp = k * Q + 1;
                if (tmp.IsProbablyPrime(10) && tmp >= p)
                {
                    p = tmp;
                    break;
                }

                k++;
            }

            P = p;
        }

        private void FindG()
        {
            var h = FindH(P, Q);
            var pow = (P - 1) / Q;
            G = BigInteger.ModPow(h, pow, P);
        }
        
        public (BigInteger, BigInteger) SignMessage(string message)
        {
            var hash = HashMsg(message);
            
            var modPow = BigInteger.ModPow(G, K, P);
            R = modPow % Q;
            var firstPiece = ModInv(K, Q); /*BigInteger.ModPow(K, Q - 2, Q);*/
            var secondPiece = (hash + X * R) % Q;
            S = (firstPiece * secondPiece) % Q;

            while (R == 0 || S == 0)
            {
                K = GetRandomNum(Q);
                Console.WriteLine($"k = {K}");
                Console.WriteLine();
                modPow = BigInteger.ModPow(G, K, P);
                R = modPow % Q;
                S = ((hash + X * R) / K) % Q;
            } 

            return (R, S);
        }
        
        public bool ValidateSign(string message1, BigInteger r1, BigInteger s1, BigInteger q, BigInteger p, BigInteger g, BigInteger y)
        {
            if (r1 <= 0 || s1 <= 0 || r1 >= q || s1 >= q)
            {
                return false;
            }

            var w = ModInv(s1, q); /*BigInteger.ModPow(s1, q - 2, q);*/
            var hash = HashMsg(message1);
            BigInteger.DivRem(hash * w, q, out var u1);
            BigInteger.DivRem(r1 * w, q, out var u2);

            var modPowU1 = BigInteger.ModPow(g, u1, p);
            var modPowU2 = BigInteger.ModPow(y, u2, p);
            var res = (modPowU1 * modPowU2) % p;
            BigInteger.DivRem(res, q, out var v);

            return v == r1;
        }

        
        private BigInteger GetPrimeWithin(int lowPow, int highPow)
        {
            var val = BigInteger.Zero;
            var low = BigInteger.Pow(2, lowPow);
            var high = BigInteger.Pow(2, highPow);
            var bytes = new Byte[low.ToByteArray().LongLength];
            var rnd = new Random();

            do
            {
                do
                {
                    rnd.NextBytes(bytes);
                    val = new BigInteger(bytes);
                } while (!val.IsProbablyPrime(10));
            } while (val >= high || val <= low);

            return val;
        }
        
        private static BigInteger FindY(BigInteger g, BigInteger x, BigInteger p) => 
            BigInteger.ModPow(g, x, p);
        private BigInteger FindK() => new Random().Next(1, Int32.MaxValue);
        
        private int GetPow()
        {
            var l = 0;
            var rnd = new Random();

            do
            {
                l = rnd.Next(512, 1025);
            } while (l % 64 != 0);

            return l;
        }
        
        private BigInteger FindH(BigInteger p, BigInteger q)
        {
            BigInteger h;
            var modPow = BigInteger.Zero;
            do
            {
                h = GetRandomNum(p - 1);
                modPow = BigInteger.ModPow(h, (p - 1) / q, p);
            } while (modPow <= 1);

            return h;
        }
        
        private BigInteger GetRandomNum(BigInteger q)
        {
            var i = 0;
            var tmp = q;
            BigInteger res;

            while (tmp != 0)
            {
                tmp /= 10;
                i++;
            }
            var rnd = new Random();
            var numLength = rnd.Next(1, i / 2);
            var bytes = new byte[numLength];
            do
            {
                rnd.NextBytes(bytes);
                bytes[^1] &= (byte) 0x7F;
                res = new BigInteger(bytes);
            } while (res <= 0 || res >= q);

            return res;
        }

        private BigInteger HashMsg(string message)
        {
            using var sha1 = new SHA1Managed();
            var hash = sha1.ComputeHash(Encoding.ASCII.GetBytes(message));
            hash[^1] &= (byte) 0x7F;

            return new BigInteger(hash);
        }

        private BigInteger ModInv(BigInteger num, BigInteger mod)
        {
            var dividend = mod;
            var divisor = num;
            var v = BigInteger.Zero;
            var d = BigInteger.One;
            var quotient = BigInteger.One;
            BigInteger x;
            while (divisor > BigInteger.Zero)
            {
                quotient = dividend / divisor;
                x = divisor;
                divisor = dividend - quotient * x;
                dividend = x;
                x = d;
                d = v - quotient * x;
                v = x;
            }

            return (v + mod) % mod;
        }
    }
}