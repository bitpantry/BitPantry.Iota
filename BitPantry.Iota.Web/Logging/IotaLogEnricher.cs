using Microsoft.Extensions.Diagnostics.Enrichment;

namespace BitPantry.Iota.Web.Logging
{

    /* Adapted from https://josef.codes/append-correlation-id-to-all-log-entries-in-asp-net-core/
     * 
     * This class is an implementation of the ILogEnricher interface. It is used to enrich log entries with additional
     * information. Here it uses the httpContextAccessor to get the current user id and add it to the log entry. The 
     * CurrentUserId is stored in the HttpContext.Items collection and accessed using the HttpContextLogEnricherPropertyAccessor
     */

    public class IotaLogEnricher : ILogEnricher
    {
        private readonly IHttpContextAccessor _httpCtxAccessor;

        public IotaLogEnricher(IHttpContextAccessor httpCtxAccessor)
        {
            _httpCtxAccessor = httpCtxAccessor;
        }

        public void Enrich(IEnrichmentTagCollector collector)
        {
            var props = _httpCtxAccessor.HttpContext.GetLogEnricherProperties();
            if (props.IsAvailable)
            {
                collector.Add("UserId", _httpCtxAccessor.HttpContext.GetLogEnricherProperties().CurrentUserId);
            }
        }
    }
}
