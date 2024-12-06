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

public class WebServerFixture : IAsyncLifetime, IDisposable
{
    private readonly IHost host;
    public IBrowser Browser { get; private set; }
    public IPlaywright Playwright { get; private set; }
    public string BaseUrl { get; }
    public ApplicationEnvironment AppEnvironment { get; }

    public WebServerFixture()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
        
        AppEnvironment = ApplicationEnvironment.Create();

        var port = GetRandomUnusedPort();
        BaseUrl = $"http://localhost:{port}";

        Console.WriteLine($"Using URL {BaseUrl}");

        var builder = BitPantry.Iota.Web.Program.CreateBuilder(null, AppEnvironment.ContextId);

        builder.WebHost.UseUrls(BaseUrl);

        builder.Services.AddControllersWithViews()
            .PartManager.ApplicationParts.Add(new AssemblyPart(typeof(HomeController).Assembly));

        host = BitPantry.Iota.Web.Program.CreateApp(builder);
    }

    public void Dispose()
    {
        host?.Dispose();
        Playwright?.Dispose();
        AppEnvironment?.Dispose();
    }

    public async Task InitializeAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Args = new[] { "--ignore-certificate-errors" }
        });
        await host.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await host.StopAsync();
        host?.Dispose();
        Playwright?.Dispose();
    }

    private static int GetRandomUnusedPort()
    {
        using var listener = new TcpListener(IPAddress.Any, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }


}
