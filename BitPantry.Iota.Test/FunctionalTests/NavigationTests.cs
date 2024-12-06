using BitPantry.Iota.Test.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Playwright;
using System.Xml.Linq;

namespace BitPantry.Iota.Test.FunctionalTests
{
    public class NavigationTests : IClassFixture<WebServerFixture>, IClassFixture<ApplicationEnvironmentFixture>
    {
        private readonly WebServerFixture _webSvrFixture;
        private readonly ApplicationEnvironmentFixture _testEnvFixture;

        public NavigationTests(WebServerFixture webSvrFixture, ApplicationEnvironmentFixture testEnvFixture)
        {
            _webSvrFixture = webSvrFixture;
            _testEnvFixture = testEnvFixture;
        }

        [Fact]
        public async Task HomePageHasText()
        {
            //var page = await _webSvrFixture.Browser.NewPageAsync();
            //await page.GotoAsync(_webSvrFixture.BaseUrl);

            await Task.Delay(60000);

            //await page.ScreenshotAsync(new()
            //{
            //    Path = "screenshot.png",
            //    FullPage = true
            //});
        }

    }
}
