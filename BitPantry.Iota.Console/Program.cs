using BitPantry.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using BitPantry.Iota.Infrastructure.IoC;
using BitPantry.Iota.Application.IoC;
using Microsoft.Extensions.Configuration;
using BitPantry.Iota.Infrastructure.Settings;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using BitPantry.Iota.Application.Service;

namespace BitPantry.Iota.Console
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var environment = args.Length > 0 ? args[0] : "Development";

            var config = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .Build();

            var settings = new AppSettings(config);

            System.Console.WriteLine($"Environment :: {environment}");
            System.Console.WriteLine();

            var appBuilder = new CommandLineApplicationBuilder();

            appBuilder.Services.ConfigureInfrastructureServices(settings, CachingStrategy.InMemory);
            appBuilder.Services.ConfigureApplicationServices(GetWorkflowService);

            appBuilder.Services.AddLogging(cfg =>
            {
                cfg.AddConfiguration(config.GetSection("Logging"));
                cfg.AddSimpleConsole(opt =>
                {
                    opt.SingleLine = true;
                });
            });

            appBuilder.RegisterCommands(typeof(Program));

            var app = appBuilder.Build();

            do
            {
                try
                {
                    System.Console.Write("$ ");
                    var input = System.Console.ReadLine();

                    if(System.IO.File.Exists(input))
                        await ExecuteScript(input, app);
                    else
                        await app.Run(input);
                }
                catch (Exception ex)
                {
                    System.Console.ForegroundColor = ConsoleColor.Red;
                    System.Console.WriteLine($"An unhandled exception occured :: {ex.Message}");
                    System.Console.ResetColor();
                }
            } while (true);

        }

        private static async Task ExecuteScript(string input, CommandLineApplication app)
        {
            var lines = File.ReadAllLines(input);
            foreach (var line in lines)
            {
                System.Console.WriteLine(line);
                var resp = await app.Run(line);

                if (resp.ResultCode != RunResultCode.Success)
                {
                    System.Console.WriteLine();
                    System.Console.WriteLine();
                    System.Console.ForegroundColor = ConsoleColor.Red;
                    System.Console.WriteLine("Script execution cannot continue.");
                    System.Console.ResetColor();
                }
            }
        }

        private static IWorkflowService GetWorkflowService(IServiceProvider svcProvider)
        {
            return svcProvider.GetRequiredService<BasicWorkflowService>();
        }
    }
}
