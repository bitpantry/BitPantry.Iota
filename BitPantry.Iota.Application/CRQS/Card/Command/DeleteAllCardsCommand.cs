using BitPantry.Iota.Data.Entity;
using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.CRQS.Card.Command
{
    public class DeleteAllCardsCommandHandler : IRequestHandler<DeleteAllCardsCommand>
    {
        private EntityDataContext _dbCtx;

        public DeleteAllCardsCommandHandler(EntityDataContext dbCtx)
        {
            _dbCtx = dbCtx;
        }

        public async Task Handle(DeleteAllCardsCommand request, CancellationToken cancellationToken)
        {
            await _dbCtx.UseConnection(async (conn, trans) => await conn.ExecuteAsync("DELETE FROM Cards", transaction: trans));
        }
    }

    public record DeleteAllCardsCommand() : IRequest
    {
    }
}
