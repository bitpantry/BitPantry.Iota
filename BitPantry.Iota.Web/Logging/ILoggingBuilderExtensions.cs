namespace BitPantry.Iota.Web.Logging
{
    public static class ILoggingBuilderExtensions
    {
        public static ILoggingBuilder ConfigureIotaWebLogging(this ILoggingBuilder builder, string envName, ConfigurationManager config)
        {
            var seqSection = config.GetSection("Seq");
            if (seqSection.Exists()) 
                builder.AddSeq(seqSection);

            builder.EnableEnrichment();
            
            builder.Services.AddLogEnricher<IotaLogEnricher>();
            builder.Services.AddScoped<IotaLogEnricherMiddleware>();

            return builder;
        }
    }
}
