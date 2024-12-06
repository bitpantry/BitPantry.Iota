using Microsoft.Extensions.Hosting;
using Microsoft.Playwright;
using System.Net.Sockets;
using System.Net;
using Xunit;
using BitPantry.Iota.Web;
using Microsoft.AspNetCore.Hosting;
using BitPantry.Iota.Test;
using BitPantry.Iota.Infrastructure.IoC;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using BitPantry.Iota.Web.Controllers;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.FileProviders;
using BitPantry.Iota.Test.Fixtures;
using BitPantry.Iota.Infrastructure;
using Microsoft.Extensions.Logging;
using BitPantry.Iota.Application;
using BitPantry.Iota.Data.Entity;
using Microsoft.EntityFrameworkCore;

[CollectionDefinition("playwright")]
public class PlaywrightFixture_Collection : ICollectionFixture<PlaywrightFixture> { }

public class PlaywrightFixture : IAsyncLifetime, IDisposable, IHaveServiceProvider
{
    private readonly IHost _host;

    public string ContextId { get; } = Crypt.GenerateSecureRandomString(8);
    public IBrowser Browser { get; private set; }
    public IPlaywright Playwright { get; private set; }
    public string BaseUrl { get; } = $"http://localhost:{GetRandomUnusedPort()}";
    public IServiceProvider ServiceProvider => _host.Services;

    public PlaywrightFixture()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");

        _host = IotaWebBootstrap.BuildIotaWebApp(builder =>
        {
            builder.WebHost.UseUrls(BaseUrl);

            builder.Services.AddControllersWithViews()
                .AddApplicationPart(typeof(HomeController).Assembly);

            builder.Services.AddSingleton<LocalDb>();

        }, null, ContextId);

        ((WebApplication)_host).LogConfiguration();
        ((WebApplication)_host).Logger.LogInformation(BaseUrl);
    }

    public void Dispose()
    {
        using (var scope = ServiceProvider.CreateScope())
            scope.ServiceProvider.GetRequiredService<LocalDb>().DropDatabase();
        
        _host?.Dispose();
        Playwright?.Dispose();
    }

    public async Task InitializeAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();

        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Args = new[] { "--ignore-certificate-errors" }
        });

        using (var scope = ServiceProvider.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<LocalDb>().Deploy(true);

            var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();
            dbCtx.Database.Migrate();
        }

        await _host.StartAsync();
    }

    public async Task DisposeAsync()
    { 
        await _host.StopAsync();

        using (var scope = ServiceProvider.CreateScope())
            scope.ServiceProvider.GetRequiredService<LocalDb>().DropDatabase();

        _host?.Dispose();
        Playwright?.Dispose();
    }

    private static int GetRandomUnusedPort()
    {
        using var listener = new TcpListener(IPAddress.Any, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }


}
