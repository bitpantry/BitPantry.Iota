using BitPantry.CommandLine.API;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Infrastructure.Settings;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Console.Commands.CICD
{
    [Command(Namespace ="cicd")]
    public class UpdateDatabase : CommandBase
    {
        private EntityDataContext _dbCtx;
        private AppSettings _settings;

        public UpdateDatabase(EntityDataContext dbCtx, AppSettings settings)
        {
            _dbCtx = dbCtx;
            _settings = settings;
        }

        public async Task Execute(CommandExecutionContext ctx)
        {
            var connStrBuilder = new SqlConnectionStringBuilder(_settings.ConnectionStrings.EntityDataContext);

            Info.WriteLine($"Examining database :: {connStrBuilder.DataSource} ...");

            var pendingMigrations = await _dbCtx.Database.GetPendingMigrationsAsync();

            if(pendingMigrations.Any())
            {
                Info.WriteLine($"{pendingMigrations.Count()} migrations pending");
                Info.WriteLine();

                foreach (var mig in pendingMigrations)
                    Info.WriteLine($"\t- {mig}");

                Info.WriteLine();
                
                if(Confirm($"Apply migrations to {connStrBuilder.DataSource}?"))
                    await _dbCtx.Database.MigrateAsync(ctx.CancellationToken);
            }
            else
            {
                Info.WriteLine("Database is up to date");
            }
        }

    }
}
