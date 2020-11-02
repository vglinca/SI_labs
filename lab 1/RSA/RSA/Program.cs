using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace RSA
{
	class Program
    {
        const int highBoundary = 30000;
        const int lowBoundary = 60000;

        static void Main(string[] args)
        {
            var encryptedMsg = new List<int>();
            var decriptedMsg = new List<int>();

            long p, q, n, d, e;
            (p, q) = FindPandQ();
            Console.WriteLine($"p = {p}; q = {q}");

            n = p * q;

            var eulerFuncVal = GetEulerFunction(p, q);

            d = CalculateD(eulerFuncVal);
            e = CalculateE(d, eulerFuncVal);

            Console.WriteLine($"Public key: (e = {e}, n = {n})");
            Console.WriteLine($"Private key: (d = {d}, n = {n})");

			Console.WriteLine("Enter a message:");
			var msg = Console.ReadLine().Trim();

			var message = new int[msg.Length];
			for (int i = 0; i < msg.Length; i++)
			{
				message[i] = (int)msg[i];
			}
			Console.WriteLine(string.Join('\0', message));

			var msgArr = msg.ToCharArray();

			for (int i = 0; i < msgArr.Length; i++)
            {
                //var encr = Operation(msgArr[i], e, n);
                var encr = Cryption(msgArr[i], e, n);
                encryptedMsg.Add((int)encr);
            }
            Console.WriteLine($"Encrypted message: {string.Join('\0', encryptedMsg)}");

            var encryptdMsgArr = encryptedMsg.ToArray();
            for (int i = 0; i < encryptdMsgArr.Length; i++)
            {
                //var decryptedChar = Operation(encryptdMsgArr[i], d, n);
                var decryptedChar = Cryption(encryptdMsgArr[i], d, n);
                decriptedMsg.Add((int)decryptedChar);
            }

            Console.WriteLine($"Decrypted message is: {string.Join('\0', decriptedMsg)}");
            var decryptedCharMsg = new char[encryptedMsg.Count];
            for (int i = 0; i < decryptedCharMsg.Length; i++)
            {
                decryptedCharMsg[i] = (char)decriptedMsg[i];
            }
            Console.WriteLine($"Decrypted message in symbols: {new string(decryptedCharMsg)}");

            Console.ReadLine();
        }

        static (long, long) FindPandQ()
        {
            var rnd = new Random();
            long p, q;
            do
            {
                p = rnd.Next(highBoundary, lowBoundary);
            }
            while (!IsPrime(p));

            do
            {
                q = rnd.Next(highBoundary, lowBoundary);
            }
            while (!IsPrime(q) || p == q);

            return (p, q);
        }

        static long CalculateD(long eulerFuncVal)
        {
            long d;

			for (d = 2; d < eulerFuncVal; d++)
			{
				if (GreatestCommonDivisior(d, eulerFuncVal) == 1)
				{
					return d;
				}
			}
			return 0;
		}

        //e * d mod eulerFunc = 1 => e * d = 1 + k*eulerFunc
        static long CalculateE(long d, long eulerFuncVal)
        {
			long e = eulerFuncVal / d;
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

        static bool IsPrime(long num)
        {
            if (num <= 1) return false;
            if (num == 2) return true;
            if (num % 2 == 0) return false;

            var sqrt = (long)Math.Floor(Math.Sqrt(num));
            for (int i = 3; i <= sqrt; i++)
            {
                if (num % i == 0)
                {
                    return false;
                }
            }

            return true;
        }

        static long GetEulerFunction(long p, long q) => (p - 1) * (q - 1);

        static long GreatestCommonDivisior(long e, long d)
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

        static long Operation(int s, long p, long n)
		{
            const int Size = 1000;
            const long NumOfNum = 1000000000;
            long[][] num = new long[Size][];

            for(int i = 0; i < Size; i++)
                num[i] = new long[Size];

            for(int i = 0; i < Size; i++)
                for(int j = 0; j < Size; j++)
                    num[i][j] = 0;

            num[0][0] = s;

            while(p != 1)
			{
                for (int i = 0; i < Size; i++)
                {
                    for (int j = 0; j < Size; j++)
                    {
                        num[i][j] *= s;
                    }
                }

                for (int i = 0; i < Size; i++)
                {
                    for (int j = 0; j < Size; j++)
                    {
                        if(num[i][j] / NumOfNum != 0)
						{
                            if(j < Size - 1)
							{
                                num[i][j + 1] += num[i][j] / NumOfNum;
							}
							else
							{
                                num[i + 1][0] += num[i][j] / NumOfNum;
                            }

                            num[i][j] %= NumOfNum;
						}
                    }
                }

                p--;
            }

            for(int i = Size - 1; i >= 0; i--)
			{
                for(int k = Size - 1; k >= 0; k--)
				{
                    num[i][k] %= n;

                    if(k != 0)
					{
                        num[i][k - 1] += num[i][k] * NumOfNum;
                        num[i][k] = 0;
					}
					else
					{
                        if(i != 0)
						{
                            num[i - 1][Size - 1] += num[i][k] * NumOfNum;
                            num[i][k] = 0;
                        }
					}
				}
			}

            return num[0][0];
        }

        static long Cryption(int a, long b, long c)
		{
            long rez = a;
            long ost = 1;
            while (true)
            {
                if (rez >= c)
                {
                    rez %= c;
                }
                else
                {
                    if (b % 2 != 0)
                    {
                        if (b == 1)
                        {
                            var ans = ((rez % c) * (ost % c)) % c;
                            return ans;
                        }

                        b--;
                        ost *= rez;
                        ost %= c;
                        b /= 2;

                    }
                    else
                        b /= 2;

                    rez *= rez;
                }
            }
        }

        static long PowMod(long a, long b, long c)
		{
            long r = 1;
            while(b != 0)
			{
                if((b & 0x01) != 0)
				{
                    r = (r * a) % c;
				}
                a = (a * a) % c;
                b >>= 1;
			}

            return r;
		}
    }
}
