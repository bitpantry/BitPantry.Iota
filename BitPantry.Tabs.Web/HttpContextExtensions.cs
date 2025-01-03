using BitPantry.Tabs.Web.Logging;

namespace BitPantry.Tabs.Web
{
    public static class HttpContextExtensions
    {
        public static HttpContextLogEnricherPropertyAccessor GetLogEnricherProperties(this HttpContext context)
        {
            return new HttpContextLogEnricherPropertyAccessor(context);
        }
    }
}
