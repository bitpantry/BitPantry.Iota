using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Test.Playwright
{
    public static class WebHostEnvironmentExtensions
    {
        public static UrlHelper GetUrlBuilder(this WebHostEnvironment env)
            => new UrlHelper(env);
    }
}
