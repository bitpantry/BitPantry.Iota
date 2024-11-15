using BitPantry.Iota.Application.Service;
using Microsoft.AspNetCore.DataProtection.Repositories;
using System.Xml.Linq;

namespace BitPantry.Iota.Web
{
    public class DatabaseDataProtectionKeyStore : IXmlRepository
    {
        private IServiceScopeFactory factory;

        public DatabaseDataProtectionKeyStore(IServiceScopeFactory factory)
        {
            this.factory = factory;
        }

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            using (var scope = factory.CreateScope())
            {
                return scope.ServiceProvider.GetRequiredService<DataProtectionService>()
                    .ReadDataProtectionKeys()
                    .GetAwaiter().GetResult();
            }
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            using (var scope = factory.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<DataProtectionService>()
                    .StoreDataProtectionKeys(element.ToString(SaveOptions.DisableFormatting), DateTime.UtcNow)
                    .GetAwaiter().GetResult();
            }
            
        }
    }
}
