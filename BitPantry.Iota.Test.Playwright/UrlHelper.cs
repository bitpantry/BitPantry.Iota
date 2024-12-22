using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Test.Playwright
{
    public class UrlHelper
    {
        private readonly WebHostEnvironment _env;

        public UrlHelper(WebHostEnvironment env)
        {
            _env = env;
        }

        public string AuthenticateTestUser(long userId)
            => $"{_env.BaseUrl}/test/auth/{userId}";

        public string Build(string relativePath)
            => $"{_env.BaseUrl.Trim().TrimEnd('/')}/{relativePath.Trim().TrimStart('/')}";
    }
}
