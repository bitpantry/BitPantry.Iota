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

namespace BitPantry.Iota.Application.CRQS.Card.Command
{
    public class ReorderCardCommandHandler : IRequestHandler<ReorderCardCommand>
    {
        private EntityDataContext _dbCtx;

        public ReorderCardCommandHandler(EntityDataContext dbCtx)
        {
            _dbCtx = dbCtx;
        }

        public async Task Handle(ReorderCardCommand request, CancellationToken cancellationToken)
        {
            string sql = @"
            DECLARE @CurrentOrder INT;
            SELECT @CurrentOrder = [Order] FROM Cards WHERE Id = @CardId;

            -- Case 1: Moving up
            IF @NewOrder < @CurrentOrder
            BEGIN
                UPDATE Cards
                SET [Order] = [Order] + 1
                WHERE [Order] >= @NewOrder AND [Order] < @CurrentOrder;

                UPDATE Cards
                SET [Order] = @NewOrder
                WHERE Id = @CardId;
            END

            -- Case 2: Moving down
            ELSE IF @NewOrder > @CurrentOrder
            BEGIN
                UPDATE Cards
                SET [Order] = [Order] - 1
                WHERE [Order] > @CurrentOrder AND [Order] <= @NewOrder;

                UPDATE Cards
                SET [Order] = @NewOrder
                WHERE Id = @CardId;
            END";

            var dbConnection = _dbCtx.Database.GetDbConnection();
            if (dbConnection.State != ConnectionState.Open)
                dbConnection.Open();

            using (var transaction = dbConnection.BeginTransaction())
            {
                try
                {
                    dbConnection.Execute(sql, new { CardId = request.CardId, NewOrder = request.NewOrder }, transaction: transaction);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }

    public record ReorderCardCommand(long CardId, int NewOrder) : IRequest { }
}
