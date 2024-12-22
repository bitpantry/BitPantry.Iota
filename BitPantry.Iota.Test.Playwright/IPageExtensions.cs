using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Test.Playwright
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
    }
}
