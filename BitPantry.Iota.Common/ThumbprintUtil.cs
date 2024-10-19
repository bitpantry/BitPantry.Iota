using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Common
{
    public static class ThumbprintUtil
    {
        public static string Generate(List<long> ids)
        {
            // Sort the list to ensure order-independent comparison (optional, based on your use case)
            ids.Sort();

            // Concatenate all the ids into a single string
            var idString = string.Join(",", ids);

            // Convert the string to a byte array
            var idBytes = Encoding.UTF8.GetBytes(idString);

            // Generate SHA256 hash
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(idBytes);

                // Convert hash bytes to a hexadecimal string
                var thumbprint = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

                return thumbprint;
            }
        }
    }
}
