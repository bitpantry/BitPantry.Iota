using BitPantry.Iota.Data.Entity;
using MediatR;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.CRQS.DataProtection.Commands
{
    public class StoreDataProtectionKeyCommandHandler : IRequestHandler<StoreDataProtectionKeyCommand>
    {
        private EntityDataContext _dbCtx;

        public StoreDataProtectionKeyCommandHandler(EntityDataContext dbCtx) => _dbCtx = dbCtx;

        public async Task Handle(StoreDataProtectionKeyCommand request, CancellationToken cancellationToken)
        {
            _dbCtx.DataProtectionKeys.Add(new DataProtectionKey
            {
                Xml = request.Xml,
                CreatedOn = request.CreatedOn
            });

            await _dbCtx.SaveChangesAsync();
        }
    }

    public record StoreDataProtectionKeyCommand(string Xml, DateTime CreatedOn) : IRequest { }
}
