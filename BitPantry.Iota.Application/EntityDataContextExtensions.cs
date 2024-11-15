using BitPantry.Iota.Data.Entity;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace BitPantry.Iota.Application
{
    public static class EntityDataContextExtensions
    {
        public static async Task UseConnection(this EntityDataContext dbCtx, CancellationToken cancellationToken, Func<DbConnection, DbTransaction, Task> func)
        {
            var conn = dbCtx.Database.GetDbConnection();

            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync(cancellationToken);

            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    await func(conn, trans);
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }

            await conn.CloseAsync();
            
        }

        public static async Task<T> UseConnection<T>(this EntityDataContext dbCtx, CancellationToken cancellationToken, Func<DbConnection, Task<T>> func)
        {
            var conn = dbCtx.Database.GetDbConnection();
            
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync(cancellationToken);

            return await func(conn);
        }
    }
}
