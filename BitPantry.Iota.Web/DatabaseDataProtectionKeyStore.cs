using BitPantry.Iota.Application.CRQS.DataProtection.Commands;
using BitPantry.Iota.Application.CRQS.DataProtection.Queries;
using MediatR;
using Microsoft.AspNetCore.DataProtection.Repositories;
using System.Xml.Linq;
using static System.Formats.Asn1.AsnWriter;

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
                return scope.ServiceProvider.GetRequiredService<IMediator>()
                    .Send(new ReadDataProtectionKeysQuery())
                    .GetAwaiter().GetResult();
            }
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            using (var scope = factory.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<IMediator>()
                    .Send(new StoreDataProtectionKeyCommand(element.ToString(SaveOptions.DisableFormatting), DateTime.UtcNow))
                    .GetAwaiter().GetResult();
            }
            
        }
    }
}
