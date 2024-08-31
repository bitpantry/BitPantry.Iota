using BitPantry.Iota.Data.Entity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace BitPantry.Iota.Application.CRQS.Card.Command
{
    public class DeleteCardCommandHandler : IRequestHandler<DeleteCardCommand>
    {
        private EntityDataContext _dbCtx;

        public DeleteCardCommandHandler(EntityDataContext dbCtx)
        {
            _dbCtx = dbCtx;
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
                    // Get the current order and divider of the card to be deleted
                    var cardInfo = dbConnection.QuerySingleOrDefault<dynamic>(
                        "SELECT [Order], Divider FROM Cards WHERE Id = @CardId",
                        new { request.CardId },
                        transaction: transaction);

                    if (cardInfo == null)
                        throw new Exception("Card not found.");

                    int currentOrder = cardInfo.Order;
                    int divider = cardInfo.Divider;

                    // Delete the card
                    dbConnection.Execute(
                        "DELETE FROM Cards WHERE Id = @CardId",
                        new { request.CardId },
                        transaction: transaction);

                    // Update the order of the remaining cards within the same divider
                    dbConnection.Execute(
                        "UPDATE Cards SET [Order] = [Order] - 1 WHERE Divider = @Divider AND [Order] > @CurrentOrder",
                        new { Divider = divider, CurrentOrder = currentOrder },
                        transaction: transaction);

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
