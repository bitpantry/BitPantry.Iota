using BitPantry.Iota.Data.Entity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BitPantry.Iota.Application.CRQS.DataProtection.Queries
{
    public class ReadDataProtectionKeysQueryHandler : IRequestHandler<ReadDataProtectionKeysQuery, IReadOnlyCollection<XElement>>
    {
        private EntityDataContext _dbCtx;

        public ReadDataProtectionKeysQueryHandler(EntityDataContext dbCtx) => _dbCtx = dbCtx;
        public async Task<IReadOnlyCollection<XElement>> Handle(ReadDataProtectionKeysQuery request, CancellationToken cancellationToken)
            => (await _dbCtx.DataProtectionKeys
                .AsNoTracking()
                .Select(x => XElement.Parse(x.Xml))
                .ToListAsync(cancellationToken)).AsReadOnly();
    }

    public record ReadDataProtectionKeysQuery : IRequest<IReadOnlyCollection<XElement>> { }
}
