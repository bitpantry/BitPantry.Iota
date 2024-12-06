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

        public string ContextId { get; }
        public ConnectionStrings ConnectionStrings { get; }
        public IdentitySettings Identity { get; }
        public bool UseMiniProfiler => GetValue("UseMiniProfiler", false);
        public bool EnableTestInfrastructure => GetValue("EnableTestInfrastructure", false);

        public AppSettings(IConfiguration config, string contextId = null) : base(config) 
        {
            ContextId = contextId;

            ConnectionStrings = new ConnectionStrings(config, contextId);
            Identity = new IdentitySettings(config);
        }
    }
}
