using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.Archive
{
    public class DataArchiveModel
    {
        [JsonPropertyName("data")]
        public List<UserDataArchiveModel> Users { get; set; }

        [JsonPropertyName("biblerefs")]
        public Dictionary<long, string> BibleReferences { get; set; }
    }
}
