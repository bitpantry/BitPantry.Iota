using System.Net.Sockets;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using BitPantry.Tabs.Infrastructure;
using Microsoft.Extensions.Logging;
using BitPantry.Tabs.Data.Entity;
using Microsoft.EntityFrameworkCore;
using BitPantry.Tabs.Web;
using BitPantry.Tabs.Web.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace BitPantry.Tabs.Test
{
    public class WebHostEnvironment : IHaveServiceProvider, IDisposable
    {
        private readonly object _lock = new object();

        private readonly IHost _host;
        private bool _isDeployed = false;

        public string ContextId { get; } = Crypt.GenerateSecureRandomString(8);
        public string BaseUrl { get; } = $"http://localhost:{GetRandomUnusedPort()}";
        public IServiceProvider ServiceProvider => _host.Services;

        private WebHostEnvironment(WebHostEnvironmentOptions options)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");

            _host = TabsWebBootstrap.BuildTabsWebApp(builder =>
            {
                builder.WebHost.UseUrls(BaseUrl);

                builder.Services.AddControllersWithViews()
                    .AddApplicationPart(typeof(HomeController).Assembly);

                builder.Services.AddSingleton<LocalDb>();

            }, null, ContextId);

            ((WebApplication)_host).LogConfiguration();
            ((WebApplication)_host).Logger.LogInformation(BaseUrl);
        }

        public static WebHostEnvironment Create(Action<WebHostEnvironmentOptions> createAction = null)
        {
            var opts = new WebHostEnvironmentOptions();
            createAction?.Invoke(opts);
            var env = new WebHostEnvironment(opts);

            env.Deploy();

            return env;

        }

        private void Deploy()
        {
            lock (_lock)
            {

                if (_isDeployed) return;

                using (var scope = ServiceProvider.CreateScope())
                {
                    scope.ServiceProvider.GetRequiredService<LocalDb>().Deploy(true).Wait();

                    var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();
                    dbCtx.Database.Migrate();
                }

                _host.StartAsync().Wait();

                _isDeployed = true;

            }
        }

        private static int GetRandomUnusedPort()
        {
            using var listener = new TcpListener(IPAddress.Any, 0);
            listener.Start();
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if(!_isDeployed) return;

                using (var scope = ServiceProvider.CreateScope())
                    scope.ServiceProvider.GetRequiredService<LocalDb>().DropDatabase();

                _host?.Dispose();

                _isDeployed = false;
            }
        }
    }
}
