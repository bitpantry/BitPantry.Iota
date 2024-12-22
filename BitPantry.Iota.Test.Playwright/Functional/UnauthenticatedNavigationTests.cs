using BitPantry.Iota.Web;
using BitPantry.Iota.Web.Controllers;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

namespace BitPantry.Iota.Test.Playwright.Functional
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
