using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Tabs.Infrastructure
{
    public static class Crypt
    {
        public static string GenerateSecureRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringBuilder = new StringBuilder();
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] data = new byte[4]; // Generate random bytes
                for (int i = 0; i < length; i++)
                {
                    rng.GetBytes(data);
                    int index = (int)(BitConverter.ToUInt32(data, 0) % chars.Length);
                    stringBuilder.Append(chars[index]);
                }
            }
            return stringBuilder.ToString();
        }

    }
}
