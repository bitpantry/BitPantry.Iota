using BitPantry.Iota.Application.CRQS.Bible.Query;
using BitPantry.Iota.Application.Parsers;
using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Infrastructure.Caching;
using BitPantry.Parsing.Strings.Parsers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.CRQS.Card.Command
{
    public class CreateCardCommandHandler : IRequestHandler<CreateCardCommand, CreateCardCommandResponse>
    {
        private EntityDataContext _dbCtx;
        private BibleService _bibleSvc;

        public CreateCardCommandHandler(EntityDataContext dbCtx, BibleService bibleSvc)
        {
            _dbCtx = dbCtx;
            _bibleSvc = bibleSvc;
        }

        public async Task<CreateCardCommandResponse> Handle(CreateCardCommand request, CancellationToken cancellationToken)
        {
            // read the passage data

            var result = await _bibleSvc.GetPassage(request.BibleId, request.Address);

            if (result.Code != GetPassageResultCode.Ok)
                return new CreateCardCommandResponse(false, false, 0);

            // see if the user already has this card created

            var thumbprint = ThumbprintUtil.Generate(result.Passage.Verses.Select(v => v.Id).ToList());
            if (await _dbCtx.Cards.AnyAsync(c => c.Thumbprint.Equals(thumbprint) && c.UserId == request.UserId))
                return new CreateCardCommandResponse(true, true, 0);

            // create the card

            var card = new Data.Entity.Card
            {
                UserId = request.UserId,
                AddedOn = DateTime.UtcNow,
                LastMovedOn = DateTime.UtcNow,
                Verses = result.Passage.Verses,
                Divider = Divider.Queue,
                Order = await _dbCtx.Cards.GetNextAvailableOrder(request.UserId, Divider.Queue)
            };

            _dbCtx.Cards.Add(card);
            await _dbCtx.SaveChangesAsync(cancellationToken);

            return new CreateCardCommandResponse(true, false, card.Id);
        }
    }

    public record CreateCardCommand(long UserId, long BibleId, string Address) : IRequest<CreateCardCommandResponse> { }

    public record CreateCardCommandResponse(bool IsValidAddress, bool isAlreadyCreated, long CardId) { }
}
