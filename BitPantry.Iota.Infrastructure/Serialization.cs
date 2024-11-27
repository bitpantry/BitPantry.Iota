using Newtonsoft.Json;
using System.Text;
using System.Text.Json;

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

        public static void SerializeObject(this Utf8JsonWriter jsonWriter, object obj, string propertyName = null)
        {
            if(!string.IsNullOrEmpty(propertyName))
                jsonWriter.WritePropertyName(propertyName);

            JsonSerializerOptions options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            System.Text.Json.JsonSerializer.Serialize(jsonWriter, obj, options);
        }
    }
}
