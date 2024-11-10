using BitPantry.Iota.Application.CRQS.Bible.Query;
using BitPantry.Iota.Application.Logic;
using BitPantry.Iota.Application.Parsers;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Infrastructure.Caching;
using BitPantry.Parsing.Strings.Parsers;
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
    public class CreateCardCommandHandler : IRequestHandler<CreateCardCommand, CreateCardCommandResponse>
    {
        private ILogger<CreateCardCommandHandler> _logger;
        private EntityDataContext _dbCtx;
        private PassageLogic _bibleLgc;

        public CreateCardCommandHandler(ILogger<CreateCardCommandHandler> logger, EntityDataContext dbCtx, PassageLogic bibleLgc)
        {
            _logger = logger;
            _dbCtx = dbCtx;
            _bibleLgc = bibleLgc;
        }

        public async Task<CreateCardCommandResponse> Handle(CreateCardCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Creating new card :: {@Request}", request);

            // read the passage data

            var result = await _bibleLgc.GetPassageQuery(_dbCtx, request.BibleId, request.Address);

            if (result.Code != GetPassageResultCode.Ok)
                return new CreateCardCommandResponse();

            // see if the user already has this card created

            if (await _dbCtx.Cards.AnyAsync(c => c.Address.Equals(result.Passage.GetAddressString(false)) && c.UserId == request.UserId, cancellationToken: cancellationToken))
                return new CreateCardCommandResponse(true, true);

            // does the user have a daily card? If so, create in queue, otherwise create daily card

            var tab = request.ToTab;

            if(!tab.HasValue)
                tab = await _dbCtx.Cards.AnyAsync(c => c.UserId == request.UserId && c.Tab == Tab.Daily)
                    ? Tab.Queue
                    : Tab.Daily;

            // create the card

            var card = result.Passage.ToCard(
                request.UserId, 
                tab.Value, 
                await _dbCtx.Cards.GetNextAvailableOrder(request.UserId, tab.Value));

            _dbCtx.Cards.Add(card);
            await _dbCtx.SaveChangesAsync(cancellationToken);

            // return the response

            return new CreateCardCommandResponse(true, false, result.Passage.GetAddressString(true), card.Id, tab);
        }
    }

    public record CreateCardCommand(long UserId, long BibleId, string Address, Tab? ToTab = null) : IRequest<CreateCardCommandResponse> { }

    public record CreateCardCommandResponse(
        bool IsValidAddress = false, 
        bool isAlreadyCreated = false, 
        string Address = null,
        long CardId = 0, 
        Tab? Tab = null) { }
}
