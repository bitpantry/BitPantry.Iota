namespace BitPantry.Tabs.Web.Logging
{
    /*
     * Middleware that stores temporary information on the HttpContext.Items collection for logging enrichment. This
     * approach is used because certain dependencies would cause circular references for the logging system, so they
     * are used here in middleware and relevant information is stored in the HttpContext.Items collection to be accessed
     * later during logging enrichment.
     */

    public class TabsLogEnricherMiddleware : IMiddleware
    {
        private AppStateCookie _appStateCookie;

        public TabsLogEnricherMiddleware(AppStateCookie appStateCookie)
        {
            _appStateCookie = appStateCookie;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var props = context.GetLogEnricherProperties();

            if (props.IsAvailable)
                context.GetLogEnricherProperties().CurrentUserId = _appStateCookie.CurrentUserId;

            await next(context);
        }
    }

}
