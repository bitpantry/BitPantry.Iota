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
            var dbConnection = _dbCtx.Database.GetDbConnection();

            if (dbConnection.State != System.Data.ConnectionState.Open)
                dbConnection.Open();

            using (var transaction = dbConnection.BeginTransaction())
            {
                try 
                {
                    await dbConnection.ExecuteAsync("DELETE FROM Cards", transaction: transaction);
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

    public record DeleteAllCardsCommand() : IRequest
    {
    }
}
