using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Infrastructure.Settings
{
    public class GoogleClientSettings : AppSettingsBase
    {
        public GoogleClientSettings(IConfiguration config) : base(config, "Identity:Google") { }

        public string ClientId => GetValue("ClientId");
        public string ClientSecret => GetValue("ClientSecret");
    }
}
