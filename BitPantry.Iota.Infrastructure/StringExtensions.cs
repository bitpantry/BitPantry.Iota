using System.Text;

namespace BitPantry.Iota.Infrastructure
{
    public static class StringExtensions
    {
        public static byte[] ToByteArray(this string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }
    }
}
