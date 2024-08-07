using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Infrastructure
{
    public static class Serialization
    {
        public static byte[] Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj).ToByteArray();
        }

        public static T Deserialize<T>(byte[] bytes)
        {
            return JsonConvert.DeserializeObject<T>(Encoding.ASCII.GetString(bytes));
        }
    }
}
