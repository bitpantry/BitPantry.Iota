using System.Security.Cryptography;
using System.Text;

namespace BitPantry.Iota.Infrastructure
{
    public static class StringExtensions
    {
        public static byte[] ToByteArray(this string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        public static string ComputeHash(this string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                var builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static int CalculateLevenshteinDistance(this string source, string target)
        {
            if (string.IsNullOrEmpty(source))
                return string.IsNullOrEmpty(target) ? 0 : target.Length;

            if (string.IsNullOrEmpty(target))
                return source.Length;

            int sourceLength = source.Length;
            int targetLength = target.Length;
            int[,] distanceMatrix = new int[sourceLength + 1, targetLength + 1];

            for (int i = 0; i <= sourceLength; i++)
                distanceMatrix[i, 0] = i;

            for (int j = 0; j <= targetLength; j++)
                distanceMatrix[0, j] = j;

            for (int i = 1; i <= sourceLength; i++)
            {
                for (int j = 1; j <= targetLength; j++)
                {
                    int cost = (source[i - 1] == target[j - 1]) ? 0 : 1;
                    distanceMatrix[i, j] = Math.Min(Math.Min(
                        distanceMatrix[i - 1, j] + 1,
                        distanceMatrix[i, j - 1] + 1),
                        distanceMatrix[i - 1, j - 1] + cost);
                }
            }

            return distanceMatrix[sourceLength, targetLength];
        }
    }
}
