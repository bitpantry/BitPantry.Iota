using BitPantry.Tabs.Infrastructure.Settings;
using BitPantry.Tabs.Web.IoC;
using BitPantry.Tabs.Infrastructure.IoC;
using BitPantry.Tabs.Application.IoC;
using BitPantry.Tabs.Web.Logging;
using BitPantry.Tabs.Common;
using Microsoft.Data.SqlClient;
using System.Drawing.Text;
using BitPantry.Tabs.Application.Service;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using BitPantry.Tabs.Application;

namespace BitPantry.Tabs.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var app = TabsWebBootstrap.BuildTabsWebApp(null, args, null);
            app.LogConfiguration();
            app.Run();
        }
    }
}

//https://www.youtube.com/watch?v=lxJutCKH1fs