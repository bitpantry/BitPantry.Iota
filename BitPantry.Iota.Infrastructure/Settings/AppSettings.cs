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
        public bool UseMiniProfiler => GetValue("UseMiniProfiler", false);
        public bool EnableTestInfrastructure => GetValue("EnableTestInfrastructure", false);

        public AppSettings(IConfiguration config, string dataContextId = null) : base(config) 
        {
            ConnectionStrings = new ConnectionStrings(config, dataContextId);
            Identity = new IdentitySettings(config);
        }
    }
}
