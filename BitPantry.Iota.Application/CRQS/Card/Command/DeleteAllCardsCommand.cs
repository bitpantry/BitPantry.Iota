using BitPantry.Iota.Data.Entity;
using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.CRQS.Card.Command
{
    public class DeleteAllCardsCommandHandler : IRequestHandler<DeleteAllCardsCommand>
    {
        private ILogger<DeleteAllCardsCommandHandler> _logger;
        private EntityDataContext _dbCtx;

        public DeleteAllCardsCommandHandler(ILogger<DeleteAllCardsCommandHandler> logger, EntityDataContext dbCtx)
        {
            _logger = logger;
            _dbCtx = dbCtx;
        }

        public async Task Handle(DeleteAllCardsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Deleting call cards");

            await _dbCtx.UseConnection(async (conn, trans) => await conn.ExecuteAsync("DELETE FROM Cards", transaction: trans));
        }
    }

    public record DeleteAllCardsCommand() : IRequest
    {
    }
}
