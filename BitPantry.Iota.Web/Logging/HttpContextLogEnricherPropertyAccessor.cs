namespace BitPantry.Iota.Web.Logging
{

    // Creates a consistent way to set / get HttpContext items, primarily used for logging enrichment

    public class HttpContextLogEnricherPropertyAccessor
    {
        private const string LOG_ENRICHER_DICTIONARY_KEY = "BitPantry.Iota.Web.LogEnricher";

        private const string PROPERTY_KEY_CURRENT_USER_ID = "BitPantry.Iota.Web.LogEnricher.CurrentUserId";

        private readonly HttpContext _context;

        public bool IsAvailable => _context != null;

        public long CurrentUserId
        {
            get => (long)Get(PROPERTY_KEY_CURRENT_USER_ID, 0L);
            set => Set(PROPERTY_KEY_CURRENT_USER_ID, value);
        }

        public HttpContextLogEnricherPropertyAccessor(HttpContext context)
        {
            if (context != null)
            {
                if (!context.Items.ContainsKey(LOG_ENRICHER_DICTIONARY_KEY))
                    context.Items.Add(LOG_ENRICHER_DICTIONARY_KEY, new Dictionary<string, object>());

                _context = context;
            }
        }

        private void Set(string key, object value)
        {
            if (!IsAvailable)
                throw new InvalidOperationException("HttpContext is not available");

            var dictionary = (Dictionary<string, object>)_context.Items[LOG_ENRICHER_DICTIONARY_KEY];
            dictionary[key] = value;
        }

        private object Get(string key, object defaultValue = null)
        {
            if (!IsAvailable)
                throw new InvalidOperationException("HttpContext is not available");

            var dictionary = (Dictionary<string, object>)_context.Items[LOG_ENRICHER_DICTIONARY_KEY];
            return dictionary.TryGetValue(key, out object value) ? value : defaultValue;
        }

    }
}
