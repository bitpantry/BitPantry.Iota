using BitPantry.Iota.Data.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application
{
    public static class EntityDataContextExtensions
    {
        public static async Task UseConnection(this EntityDataContext dbCtx, Func<DbConnection, DbTransaction, Task> func)
        {
            var conn = dbCtx.Database.GetDbConnection();

            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();

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
            
        }

        public static async Task<T> UseConnection<T>(this EntityDataContext dbCtx, Func<DbConnection, Task<T>> func)
        {
            var conn = dbCtx.Database.GetDbConnection();
            
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();

            return await func(conn);
        }
    }
}
