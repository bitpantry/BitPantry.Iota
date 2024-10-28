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
        public  GoogleClientSettings Google { get; }

        public IdentitySettings(IConfiguration config) : base(config, "Identity") 
        {
            Google = new GoogleClientSettings(config);
        }
    }
}
