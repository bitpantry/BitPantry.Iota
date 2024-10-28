using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Data.Entity
{
    public class DbConnectionFactory
    {
        private EntityDataContext _dbCtx;

        public DbConnectionFactory(EntityDataContext dbCtx)
        {
            _dbCtx = dbCtx;
        }

        public DbConnection GetDbConnection()
            => _dbCtx.Database.GetDbConnection();
    }
}
