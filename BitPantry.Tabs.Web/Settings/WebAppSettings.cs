using BitPantry.Tabs.Infrastructure.Settings;

namespace BitPantry.Tabs.Web.Settings
{
    public class WebAppSettings : InfrastructureAppSettings
    {
        public IdentitySettings Identity { get; }
        public bool UseMiniProfiler => GetValue("UseMiniProfiler", false);
        public bool EnableTestInfrastructure => GetValue("EnableTestInfrastructure", false);

        public WebAppSettings(IConfiguration config, string contextId = null) : base(config, contextId) 
        {
            Identity = new IdentitySettings(config);
        }
    }
}
