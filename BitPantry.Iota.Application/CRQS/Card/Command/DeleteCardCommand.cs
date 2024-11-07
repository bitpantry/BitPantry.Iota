using BitPantry.Iota.Data.Entity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using BitPantry.Iota.Common;
using System.Data.Common;
using BitPantry.Iota.Application.Logic;

namespace BitPantry.Iota.Application.CRQS.Card.Command
{
    public class DeleteCardCommandHandler : IRequestHandler<DeleteCardCommand>
    {
        private EntityDataContext _dbCtx;
        private CardLogic _cardLgc;

        public DeleteCardCommandHandler(EntityDataContext dbCtx, CardLogic cardLgc)
        {
            _dbCtx = dbCtx;
            _cardLgc = cardLgc;
        }

        public async Task Handle(DeleteCardCommand request, CancellationToken cancellationToken)
        {
            await _dbCtx.UseConnection(async (conn, trans) =>
            {
                // Get the current order and tab of the card to be deleted

                var cardInfo = conn.QuerySingleOrDefault<dynamic>(
                    "SELECT [Order], UserId, Tab FROM Cards WHERE Id = @CardId",
                    new { request.CardId },
                    transaction: trans);

                if (cardInfo == null)
                    throw new Exception("Card not found.");

                int currentOrder = cardInfo.Order;
                int tab = cardInfo.Tab;
                long userId = cardInfo.UserId;

                // Delete the card
                conn.Execute(
                    "DELETE FROM Cards WHERE Id = @CardId",
                    new { request.CardId },
                    transaction: trans);

                // Update the order of the remaining cards within the same tab
                conn.Execute(
                    "UPDATE Cards SET [Order] = [Order] - 1 WHERE Tab = @Tab AND UserId = @UserId AND [Order] > @CurrentOrder",
                    new { Tab = tab, UserId = userId, CurrentOrder = currentOrder },
                    transaction: trans);

                // if the card was in the daily tab, promote the next queued card

                if (tab == (int)Tab.Daily)
                    _ = await _cardLgc.TryPromoteNextQueueCardCommand(conn, trans, userId);
            });
        }
    }

    public record DeleteCardCommand(long CardId) : IRequest { }
}
