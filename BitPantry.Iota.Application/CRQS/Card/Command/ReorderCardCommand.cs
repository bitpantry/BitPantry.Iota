using BitPantry.Iota.Data.Entity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using BitPantry.Iota.Common;
using BitPantry.Iota.Application.Logic;

namespace BitPantry.Iota.Application.CRQS.Card.Command
{
    public class ReorderCardCommandHandler : IRequestHandler<ReorderCardCommand>
    {
        private EntityDataContext _dbCtx;
        private CardLogic _cardLgc;

        public ReorderCardCommandHandler(EntityDataContext dbCtx, CardLogic cardLgc)
        {
            _dbCtx = dbCtx;
            _cardLgc = cardLgc;
        }

        public async Task Handle(ReorderCardCommand request, CancellationToken cancellationToken)
        {
            await _dbCtx.UseConnection(async (conn, trans) 
                => await _cardLgc.ReorderCardCommand(conn, trans, request.UserId, request.CardId, request.Tab, request.NewOrder));
        }
    }

    public record ReorderCardCommand(Tab Tab, long UserId, long CardId, int NewOrder) : IRequest { }
}
