using BitPantry.Tabs.Web;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BitPantry.Tabs.Test.Playwright
{
    public static class IPageExtensions
    {
        public static async Task AuthenticateUser(this IPage page, long userId)
        {
            await page.GotoAsync(Fixture.Environment.GetUrlBuilder().AuthenticateTestUser(userId));
            await page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build(""));
        }

        public static async Task GotoRelativeAsync(this IPage page, string path, PageGotoOptions options = null)
        {
            await page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build(path), options);
        }

        public static async Task WaitForUrlAsyncIgnoreCase(this IPage page, string path, PageWaitForURLOptions opt = null)
            => await page.WaitForURLAsync(new Regex($"^{path}$", RegexOptions.IgnoreCase), opt);

        public static async Task SetUserCurrentTimeUtcOverride(this IPage page, DateTime userCurrentTimeUtcOverride)
            => await page.Context.AddCookiesAsync(
            [
                new Cookie
                {
                    Name = Constants.USER_CURRENT_TIME_OVERRIDE_UTC_KEY,
                    Value = Uri.EscapeDataString(userCurrentTimeUtcOverride.ToString("o")),
                    Path = "/", 
                    Domain = "localhost"
                }
            ]);

        public static async Task ClearUserCurrentTimeUtcOverride(this IPage page)
            => await page.Context.ClearCookiesAsync(new() { Name = Constants.USER_CURRENT_TIME_OVERRIDE_UTC_KEY });

        public static async Task SetUserTimezoneOverride(this IPage page, string userTimezoneOverride)
            => await page.Context.AddCookiesAsync(
            [
                new Cookie
                {
                    Name = Constants.TIMEZONE_OVERRIDE_KEY,
                    Value = userTimezoneOverride,
                    Path = "/",
                    Domain = "localhost"
                }
            ]);

        public static async Task ClearUserTimezoneOverride(this IPage page)
            => await page.Context.ClearCookiesAsync(new() { Name = Constants.TIMEZONE_OVERRIDE_KEY });

    }
}
