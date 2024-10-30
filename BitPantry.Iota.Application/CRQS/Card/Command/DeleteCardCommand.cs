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
using BitPantry.Iota.Application.Service;

namespace BitPantry.Iota.Application.CRQS.Card.Command
{
    public class DeleteCardCommandHandler : IRequestHandler<DeleteCardCommand>
    {
        private EntityDataContext _dbCtx;
        private CardService _cardSvc;

        public DeleteCardCommandHandler(EntityDataContext dbCtx, CardService cardSvc)
        {
            _dbCtx = dbCtx;
            _cardSvc = cardSvc;
        }

        public async Task Handle(DeleteCardCommand request, CancellationToken cancellationToken)
        {
            var dbConnection = _dbCtx.Database.GetDbConnection();

            if (dbConnection.State == System.Data.ConnectionState.Closed)
                dbConnection.Open();

            using (var transaction = dbConnection.BeginTransaction())
            {
                try
                {
                    // Get the current order and tab of the card to be deleted

                    var cardInfo = dbConnection.QuerySingleOrDefault<dynamic>(
                        "SELECT [Order], UserId, Tab FROM Cards WHERE Id = @CardId",
                        new { request.CardId },
                        transaction: transaction);

                    if (cardInfo == null)
                        throw new Exception("Card not found.");

                    int currentOrder = cardInfo.Order;
                    int tab = cardInfo.Tab;
                    long userId = cardInfo.UserId;

                    // Delete the card
                    dbConnection.Execute(
                        "DELETE FROM Cards WHERE Id = @CardId",
                        new { request.CardId },
                        transaction: transaction);

                    // Update the order of the remaining cards within the same tab
                    dbConnection.Execute(
                        "UPDATE Cards SET [Order] = [Order] - 1 WHERE Tab = @Tab AND UserId = @UserId AND [Order] > @CurrentOrder",
                        new { Tab = tab, UserId = userId, CurrentOrder = currentOrder },
                        transaction: transaction);

                    // if the card was in the daily tab, promote the next queued card

                    if(tab == (int)Tab.Daily)
                        _ = await _cardSvc.PromoteNextQueueCard(userId, dbConnection, transaction);

                    // Commit the transaction
                    transaction.Commit();
                }
                catch
                {
                    // Rollback the transaction in case of an error
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }

    public record DeleteCardCommand(long CardId) : IRequest { }
}
