using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging;

namespace BitPantry.Iota.Web.Logging
{
    public static class ILoggingBuilderExtensions
    {
        public static ILoggingBuilder ConfigureIotaLogging(this ILoggingBuilder builder, string envName, ConfigurationManager config)
        {
            // builder = builder.ClearProviders();
            
            //builder.AddJsonConsole(options =>
            //{
            //    options.IncludeScopes = true;
            //    options.UseUtcTimestamp = true;
            //    options.JsonWriterOptions = options.JsonWriterOptions with { Indented = true };
            //});

            if(envName == "Development")
                builder.AddSeq(config.GetSection("Seq"));

            builder.EnableEnrichment();
            
            builder.Services.AddLogEnricher<IotaLogEnricher>();
            builder.Services.AddScoped<IotaLogEnricherMiddleware>();

            return builder;
        }
    }
}
