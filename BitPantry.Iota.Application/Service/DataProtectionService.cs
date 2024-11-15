using BitPantry.Iota.Data.Entity;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace BitPantry.Iota.Application.Service
{
    public class DataProtectionService
    {
        private readonly EntityDataContext _dbCtx;

        public DataProtectionService(EntityDataContext dbCtx)
        {
            _dbCtx = dbCtx;
        }

        public async Task StoreDataProtectionKeys(string xml, DateTime createdOn)
        {
            _dbCtx.DataProtectionKeys.Add(new DataProtectionKey
            {
                Xml = xml,
                CreatedOn = createdOn
            });

            await _dbCtx.SaveChangesAsync();
        }

        public async Task<IReadOnlyCollection<XElement>> ReadDataProtectionKeys()
            => (await _dbCtx.DataProtectionKeys
                .AsNoTracking()
                .Select(x => XElement.Parse(x.Xml))
                .ToListAsync()).AsReadOnly();
    }
}
