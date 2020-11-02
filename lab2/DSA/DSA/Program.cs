using System;
using System.Numerics;
using System.Threading;
using BigPrime;

// ReSharper disable RedundantAssignment

namespace DSA
{
	class Program
	{
		static void Main(string[] args)
		{
			BigInteger r, s;
			
			var dsa = new Dsa();
			dsa.GenerateKeys();
			
			while (true)
			{
				Console.WriteLine("Enter a message to sign or enter '.' to stop: ");
				var message = Console.ReadLine()?.Trim();
				if (message == ".")
				{
					Environment.Exit(0);
				}
				
				(r, s) = dsa.SignMessage(message);
				Console.WriteLine($"The message: {message} was has been signed with (R = {r}, S = {s}");

				Console.WriteLine("Enter message to validate: ");
				var msg1 = Console.ReadLine()?.Trim();
			
				var isValid = dsa.ValidateSign(msg1, r, s, dsa.Q, dsa.P, dsa.G, dsa.Y);
				Console.WriteLine(isValid ? $"Message: {msg1} has been successfully verified." : $"Message: {msg1}. Verification has failed.");

				Console.ReadLine();
				Console.Clear();
			}
		}
	}
}
