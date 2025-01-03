namespace BitPantry.Tabs.Web.Logging
{
    public static class ILoggingBuilderExtensions
    {
        public static ILoggingBuilder ConfigureTabsWebLogging(this ILoggingBuilder builder, string envName, ConfigurationManager config)
        {
            var seqSection = config.GetSection("Seq");
            if (seqSection.Exists()) 
                builder.AddSeq(seqSection);

            builder.EnableEnrichment();
            
            builder.Services.AddLogEnricher<TabsLogEnricher>();
            builder.Services.AddScoped<TabsLogEnricherMiddleware>();

            return builder;
        }
    }
}
