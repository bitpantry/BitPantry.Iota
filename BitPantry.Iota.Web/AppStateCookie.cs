using BitPantry.Iota.Data.Entity;
using BitPantry.Parsing.Strings;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BitPantry.Iota.Web
{
    public class AppStateCookie
    {
        public string _uid = Guid.NewGuid().ToString();

        private const string COOKIE_NAME = "bitpantry.iota.web.state";

        private const string KEY_CURRENT_USER_ID = "cuid";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDataProtector _dataProtection;
        private readonly ILogger<AppStateCookie> _logger;

        public bool IsChanged { get; private set; } = false;

        private Dictionary<string, string> _dict = new Dictionary<string, string>();

        public long CurrentUserId
        {
            get { return GetValue<long>(KEY_CURRENT_USER_ID); }
            set 
            {   
                _logger.LogDebug("AppStateCookie:CurrentSureId.Set :: {AppStateCookieId}, Value={NewUserIdValue}", _uid, value);
                SetValue(KEY_CURRENT_USER_ID, value); 
            }
        }

        public AppStateCookie(IHttpContextAccessor httpContextAccessor, IDataProtectionProvider dataProtectionProvider, ILogger<AppStateCookie> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _dataProtection = dataProtectionProvider.CreateProtector(Constants.DEFAULT_DATA_PROTECTOR);
            _logger = logger;

            try
            {
                if (_httpContextAccessor.HttpContext.Request.Cookies.ContainsKey(COOKIE_NAME))
                    _dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(_dataProtection.Unprotect(_httpContextAccessor.HttpContext.Request.Cookies[COOKIE_NAME]));
            }
            catch(System.Security.Cryptography.CryptographicException ex)
            {
                _logger.LogError(ex, "Error decrypting application state cookie");
                _httpContextAccessor.HttpContext.Response.Cookies.Delete(COOKIE_NAME);
            }

            _logger.LogDebug("AppStateCookie:ctor :: {AppStateCookieUid}, CurrentUserId={CurrentUserId}", _uid, CurrentUserId);
        }

        public void PersistCookie()
        {
            _logger.LogDebug("AppStateCookie:PersistCookie :: {AppStateCookieUid}, CurrentUserId={CurrentUserId}, IsChanged={IsChanged}", _uid, CurrentUserId, IsChanged);

            if (IsChanged)
            {
                _httpContextAccessor.HttpContext.Response.Cookies.Append(COOKIE_NAME, _dataProtection.Protect(JsonConvert.SerializeObject(_dict, Formatting.None)), new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // Ensures the cookie is only sent over HTTPS
                    SameSite = SameSiteMode.Strict
                });
            }
        }

        private string GetValue(string key, string defaultValue = null)
            => GetValue<string>(key, defaultValue);

        private T GetValue<T>(string key, T defaultValue = default(T))
        {
            if(_dict.ContainsKey(key))
                return StringParsing.Parse<T>(_dict[key]);
            return defaultValue; 
        }

        private void SetValue(string key, object value)
        {
            IsChanged = true;
            _dict[key] = value.ToString();
        }
    }
}
