using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Infrastructure.Settings
{
    public class IdentitySettings : AppSettingsBase
    {
        public GoogleIdentitySettings Google { get; }
        public MicrosoftIdentitySettings Microsoft { get; }

        public IdentitySettings(IConfiguration config) : base(config, "Identity") 
        {
            Google = new GoogleIdentitySettings(config);
            Microsoft = new MicrosoftIdentitySettings(config);
        }
    }
}
