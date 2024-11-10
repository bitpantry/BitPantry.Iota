using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Azure.Core;
using System.Data.Common;
using BitPantry.Iota.Application.Logic;
using Microsoft.Extensions.Logging;

namespace BitPantry.Iota.Application.CRQS.Card.Command
{
    public class MoveCardCommandHandler : IRequestHandler<MoveCardCommand>
    {
        private readonly ILogger<MoveCardCommandHandler> _logger;
        private readonly EntityDataContext _dbCtx;
        private readonly CardLogic _cardLgc;

        public MoveCardCommandHandler(ILogger<MoveCardCommandHandler> logger, EntityDataContext dbCtx, CardLogic cardLgc)
        {
            _logger = logger;
            _dbCtx = dbCtx;
            _cardLgc = cardLgc;
        }

        public async Task Handle(MoveCardCommand request, CancellationToken cancellationToken)
        {
            await _dbCtx.UseConnection(async (conn, trans) => await _cardLgc.MoveCardCommand(conn, trans, request.CardId, request.NewTab));
        }
    }

    public record MoveCardCommand(long CardId, Tab NewTab) : IRequest { } 
}
