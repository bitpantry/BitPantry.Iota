using Microsoft.Extensions.Configuration;

namespace BitPantry.Iota.Infrastructure.Settings
{
    public enum Environment
    {
        Development,
        Integration,
        Production,
        Test
    }

    public class AppSettings : AppSettingsBase
    {
        public IConfiguration Configuration => Config;

        public ConnectionStrings ConnectionStrings { get; }
        public IdentitySettings Identity { get; }

        public AppSettings(IConfiguration config) : base(config) 
        {
            ConnectionStrings = new ConnectionStrings(config);
            Identity = new IdentitySettings(config);
        }
    }
}
