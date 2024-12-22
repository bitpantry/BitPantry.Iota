using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Test.Playwright.Functional
{
    [TestClass]
    [DoNotParallelize]
    public class NewUserNavigationTests : PageTest
    {
        private static long _userId = 0;

        [ClassInitialize]
        public static void InitializeClass(TestContext context)
        {
            _userId = Fixture.Environment.CreateUser().GetAwaiter().GetResult();
        }

        [TestMethod]
        public async Task HomePage_Success()
        {
            await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().AuthenticateTestUser(_userId));
            await Expect(Page.GetByTestId("home.index.createCardsTwo")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("home.index.readGettingStarted")).ToBeVisibleAsync();
            await Expect(Page.GetByText("You haven't created any cards")).ToBeVisibleAsync();

        }

        [TestMethod]
        public async Task NavBarInspection_Success()
        {
            await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().AuthenticateTestUser(_userId));
            await Expect(Page.GetByTestId("nav.startReview")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("nav.cards")).ToBeVisibleAsync();
            await Page.GetByTestId("nav.cards").ClickAsync();
            await Expect(Page.GetByTestId("nav.cards.new")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("nav.cards.viewAll")).ToBeVisibleAsync();
            await Page.GetByTestId("nav.cards").ClickAsync();
            await Expect(Page.GetByTestId("nav.settings")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("nav.help")).ToBeVisibleAsync();
            await Page.GetByTestId("nav.help").ClickAsync();
            await Expect(Page.GetByTestId("nav.help.gettingStarted")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("nav.help.documentation")).ToBeVisibleAsync();
        }

        public override BrowserNewContextOptions ContextOptions()
            => Fixture.BrowserOptions.Default;
    }
}
