// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
namespace Security
{
    public class Pair
    {
        public ulong X { get; }
        public ulong N { get; }

        public Pair(ulong x, ulong n)
        {
            X = x;
            N = n;
        }
    }
}