using BitPantry.Iota.Web.Logging;

namespace BitPantry.Iota.Web
{
    public static class HttpContextExtensions
    {
        public static HttpContextLogEnricherPropertyAccessor GetLogEnricherProperties(this HttpContext context)
        {
            return new HttpContextLogEnricherPropertyAccessor(context);
        }
    }
}
