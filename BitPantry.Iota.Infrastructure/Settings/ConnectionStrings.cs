using Microsoft.Extensions.Configuration;

namespace BitPantry.Iota.Infrastructure.Settings
{
    public class ConnectionStrings : AppSettingsBase
    {
        internal ConnectionStrings(IConfiguration config) : base(config, "ConnectionStrings") { }
        public string EntityDataContext => GetValue("EntityDataContext");
    }
}
