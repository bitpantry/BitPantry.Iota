using BitPantry.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using BitPantry.Iota.Infrastructure.IoC;
using BitPantry.Iota.Application.IoC;
using Microsoft.Extensions.Configuration;
using BitPantry.Iota.Infrastructure.Settings;
using System.Reflection;
using BitPantry.CommandLine.Processing.Activation;
using Microsoft.EntityFrameworkCore.Query;
using BitPantry.Iota.Console.Commands.Bible;

namespace BitPantry.Iota.Console
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .AddJsonFile("appsettings.json")
                .Build();

            var settings = new AppSettings(config);

            var appBuilder = new CommandLineApplicationBuilder();

            appBuilder.Services.ConfigureInfrastructureServices(settings, CachingStrategy.InMemory);
            appBuilder.Services.ConfigureApplicationServices();

            appBuilder.RegisterCommands(typeof(Program));

            var app = appBuilder.Build();

            do
            {
                try
                {
                    System.Console.Write("$ ");
                    var input = System.Console.ReadLine();

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
    }
}
