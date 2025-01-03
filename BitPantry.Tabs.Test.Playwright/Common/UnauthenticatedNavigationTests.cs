using BitPantry.Tabs.Web;
using BitPantry.Tabs.Web.Controllers;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

namespace BitPantry.Tabs.Test.Playwright.Common
{
    [TestClass]
    [DoNotParallelize]
    public class UnauthenticatedNavigationTests : PageTest
    {
        [TestMethod]
        public async Task NavigateToHomePage_NotAuthenticated()
        {
            await Page.GotoAsync(Fixture.Environment.BaseUrl);
            await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Sign in with Google" })).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task NavigateToCollection_NotAuthenticated()
        {
            await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build("collection"));
            await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Sign in with Google" })).ToBeVisibleAsync();
        }

        public override BrowserNewContextOptions ContextOptions()
            => Fixture.BrowserOptions.Default;
    }
}
