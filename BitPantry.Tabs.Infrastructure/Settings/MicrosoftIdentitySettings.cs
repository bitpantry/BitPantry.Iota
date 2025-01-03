using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Tabs.Infrastructure.Settings
{
    public class MicrosoftIdentitySettings : AppSettingsBase
    {
        public MicrosoftIdentitySettings(IConfiguration config) : base(config, "Identity:Microsoft") { }

        public string ClientId => GetValue("ClientId");
        public string ClientSecret => GetValue("ClientSecret");
    }
}
