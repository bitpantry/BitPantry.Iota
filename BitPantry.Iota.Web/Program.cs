using BitPantry.Iota.Infrastructure.Settings;
using BitPantry.Iota.Web.IoC;
using BitPantry.Iota.Infrastructure.IoC;
using BitPantry.Iota.Application.IoC;
using BitPantry.Iota.Web.Logging;
using BitPantry.Iota.Common;
using Microsoft.Data.SqlClient;
using System.Drawing.Text;
using BitPantry.Iota.Application.Service;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using BitPantry.Iota.Application;

namespace BitPantry.Iota.Web
{
    public class Program
    {
        private static AppSettings _settings = null;

        public static void Main(string[] args)
        {
            var app = IotaWebBootstrap.BuildIotaWebApp(null, args, null);
            app.LogConfiguration();
            app.Run();
        }
    }
}

//https://www.youtube.com/watch?v=lxJutCKH1fs