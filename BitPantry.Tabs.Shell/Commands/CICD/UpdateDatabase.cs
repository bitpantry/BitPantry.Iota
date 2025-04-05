//using BitPantry.CommandLine.API;
//using BitPantry.Tabs.Data.Entity;
//using BitPantry.Tabs.Infrastructure.Settings;
//using Microsoft.Data.SqlClient;
//using Microsoft.EntityFrameworkCore;

//namespace BitPantry.Tabs.Shell.Commands.CICD
//{
//    [Command(Namespace ="cicd")]
//    public class UpdateDatabase : CommandBase
//    {
//        private readonly EntityDataContext _dbCtx;
//        private readonly InfrastructureAppSettings _settings;

//        public UpdateDatabase(EntityDataContext dbCtx, InfrastructureAppSettings settings)
//        {
//            _dbCtx = dbCtx;
//            _settings = settings;
//        }

//        public async Task Execute(CommandExecutionContext ctx)
//        {
//            var connStrBuilder = new SqlConnectionStringBuilder(_settings.ConnectionStrings.EntityDataContext);

//            Info.WriteLine($"Examining database :: {connStrBuilder.DataSource} ...");

//            var pendingMigrations = await _dbCtx.Database.GetPendingMigrationsAsync();

//            if(pendingMigrations.Any())
//            {
//                Info.WriteLine($"{pendingMigrations.Count()} migrations pending");
//                Info.WriteLine();

//                foreach (var mig in pendingMigrations)
//                    Info.WriteLine($"\t- {mig}");

//                Info.WriteLine();
                
//                if(Confirm($"Apply migrations to {connStrBuilder.DataSource}?"))
//                    await _dbCtx.Database.MigrateAsync(ctx.CancellationToken);
//            }
//            else
//            {
//                Info.WriteLine("Database is up to date");
//            }
//        }

//    }
//}
