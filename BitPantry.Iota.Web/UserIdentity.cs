using BitPantry.Parsing.Strings;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;

namespace BitPantry.Iota.Web
{
    public class UserIdentity
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppStateCookie _stateCookie;

        public UserIdentity(IHttpContextAccessor httpContextAccessor, AppStateCookie stateCookie) 
        { 
            _httpContextAccessor = httpContextAccessor; 
            _stateCookie = stateCookie; 
        }

        public bool IsAuthenticated => _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated;

        public long UserId
        {
            get { return _stateCookie.CurrentUserId; }
            set { _stateCookie.CurrentUserId = value; }    
        }

        public string EmailAddress =>  GetClaim(ClaimTypes.Email);

        private string GetClaim(string claimName, string defaultValue = null) => GetClaim<string>(claimName, defaultValue);
        private T GetClaim<T>(string claimName, T defaultValue = default(T))
        {
            if (!IsAuthenticated) return defaultValue;
            return StringParsing.Parse<T>(_httpContextAccessor.HttpContext.User.Claims.Single(c => c.Type == claimName).Value);
        }

    }
}
