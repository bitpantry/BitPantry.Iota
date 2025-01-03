using Microsoft.Extensions.Configuration;

namespace BitPantry.Tabs.Infrastructure.Settings
{
    public class InfrastructureAppSettings : AppSettingsBase
    {
        public IConfiguration Configuration => Config;

        public string ContextId { get; }
        public ConnectionStrings ConnectionStrings { get; }

        public InfrastructureAppSettings(IConfiguration config, string contextId = null) : base(config) 
        {
            ContextId = contextId;

            ConnectionStrings = new ConnectionStrings(config, contextId);
        }
    }
}
