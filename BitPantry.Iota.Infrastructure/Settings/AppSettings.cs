using Microsoft.Extensions.Configuration;
using System.Net;

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

        public Environment Environment => GetValue<Environment>("Environment");

        public string ApplicationName => GetValue("ApplicationName");

        public ConnectionStrings ConnectionStrings { get; }

        public AppSettings(IConfiguration config) : base(config) 
        {
            ConnectionStrings = new ConnectionStrings(config);
        }
    }
}
