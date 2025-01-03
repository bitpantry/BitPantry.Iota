using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BitPantry.Tabs.Infrastructure.Settings
{
    public class ConnectionStrings : AppSettingsBase
    {
        private readonly string _contextId;

        internal ConnectionStrings(IConfiguration config, string contextId = null) : base(config, "ConnectionStrings") 
        {
            _contextId = contextId;
        }


        private string _entityDataContext_og = null;
        private string _entityDataContext_withCtx = null;
        public string EntityDataContext
        {
            get
            {
                var val = GetValue("EntityDataContext");

                if(_entityDataContext_og == null || !_entityDataContext_og.Equals(val))
                {
                    _entityDataContext_og = val;
                    _entityDataContext_withCtx = null;

                    if(_contextId != null)
                    {
                        var builder = new SqlConnectionStringBuilder(val);
                        builder.InitialCatalog = $"{builder.InitialCatalog}_{_contextId}";
                        _entityDataContext_withCtx = builder.ConnectionString;
                    }
                }

                return _entityDataContext_withCtx ?? _entityDataContext_og;
            }
        }

        public string AzureAppConfiguration => GetValue<string>("AzureAppConfiguration", null);
    }
}
