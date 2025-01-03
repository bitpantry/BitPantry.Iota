using BitPantry.Tabs.Application.Service;
using BitPantry.Tabs.Data.Entity;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Tabs.Test.Playwright.Common
{
    [TestClass]
    public class CreateCardTests : PageTest
    {

        private async Task<long> Init()
        {
            var userId = Fixture.Environment.CreateUser().GetAwaiter().GetResult();
            await Page.AuthenticateUser(userId);
            await Page.GotoRelativeAsync("/card/new");
            return userId;
        }

        [TestMethod]
        public async Task SingleVerseSearch_Success()
        {
            _ = await Init();
            await Page.GetByTestId("card.new.txtAddress").FillAsync("rom 1:1");
            await Page.GetByTestId("card.new.btnGo").ClickAsync();
            await Expect(Page.GetByTestId("card.new.searchResultAddress")).ToContainTextAsync("Romans 1:1");
            await Expect(Page.GetByText("1 Paul, a servant of Christ")).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Paragraph)).ToContainTextAsync("1 Paul, a servant of Christ Jesus, called to be an apostle, set apart for the gospel of God,");
        }

        [TestMethod]
        public async Task MultipleVerseSearch_Success()
        {
            _ = await Init();
            await Page.GetByTestId("card.new.txtAddress").FillAsync("rom 1:1-3");
            await Page.GetByTestId("card.new.btnGo").ClickAsync();
            await Expect(Page.GetByTestId("card.new.searchResultAddress")).ToContainTextAsync("Romans 1:1-3");
            await Expect(Page.GetByRole(AriaRole.Paragraph)).ToContainTextAsync("1 Paul, a servant of Christ Jesus, called to be an apostle, set apart for the gospel of God, 2 which he promised beforehand through his prophets in the holy Scriptures, 3 concerning his Son, who was descended from David according to the flesh (ESV)");
        }

        [TestMethod]
        public async Task MultipleChapterSearch_Success()
        {
            _ = await Init();
            await Page.GetByTestId("card.new.txtAddress").FillAsync("ps 16:11-17:1");
            await Page.GetByTestId("card.new.btnGo").ClickAsync();
            await Expect(Page.GetByTestId("card.new.searchResultAddress")).ToContainTextAsync("Psalms 16:11-17:1");
            await Expect(Page.GetByTestId("card.new.searchResultPassage")).ToContainTextAsync("11 You make known to me the path of life; in your presence there is fullness of joy; at your right hand are pleasures forevermore. 17 1 A PRAYER OF DAVID.Hear a just cause, O LORD; attend to my cry! Give ear to my prayer from lips free of deceit! (ESV)");
        }

        [TestMethod]
        public async Task InvalidBookNameSearch_InvalidAddress()
        {
            _ = await Init();
            await Page.GetByTestId("card.new.txtAddress").FillAsync("bookOfEli 1:1");
            await Page.GetByTestId("card.new.btnGo").ClickAsync();
            await Expect(Page.GetByTestId("card.new.noPassageFoundMsg")).ToContainTextAsync("No passage found for \"bookOfEli 1:1\"");
        }

        [TestMethod]
        public async Task InvalidTooManyVerses_InvalidAddress()
        {
            _ = await Init();
            await Page.GetByTestId("card.new.txtAddress").FillAsync("ps 117:1-100");
            await Page.GetByTestId("card.new.btnGo").ClickAsync();
            await Expect(Page.GetByTestId("card.new.searchResultAddress")).ToContainTextAsync("Psalms 117:1-2");
            await Expect(Page.GetByRole(AriaRole.Paragraph)).ToContainTextAsync("1 Praise the LORD, all nations! Extol him, all peoples! 2 For great is his steadfast love toward us, and the faithfulness of the LORD endures forever. Praise the LORD! (ESV)");
        }

        [TestMethod]
        public async Task NoVersesReturned_InvalidAddress()
        {
            _ = await Init();
            await Page.GetByTestId("card.new.txtAddress").FillAsync("gen 1:100-101");
            await Page.GetByTestId("card.new.btnGo").ClickAsync();
            await Expect(Page.GetByTestId("card.new.noPassageFoundMsg")).ToContainTextAsync("No passage found for \"gen 1:100-101\"");
        }

        [TestMethod]
        public async Task SelectTranslation_TranslationApplied()
        {
            _ = await Init();
            await Page.GetByTestId("card.new.txtAddress").FillAsync("rom 1:1");
            await Page.GetByTestId("card.new.btnGo").ClickAsync();
            await Expect(Page.GetByTestId("card.new.searchResultAddress")).ToContainTextAsync("Romans 1:1");
            await Expect(Page.GetByRole(AriaRole.Paragraph)).ToContainTextAsync("1 Paul, a servant of Christ Jesus, called to be an apostle, set apart for the gospel of God,");
            await Page.GetByTestId("card.new.ddlTranslation").SelectOptionAsync(["2"]);
            await Expect(Page.GetByTestId("card.new.searchResultAddress")).ToContainTextAsync("Romans 1:1");
            await Expect(Page.GetByRole(AriaRole.Paragraph)).ToContainTextAsync("1 I, Paul, am a devoted slave of Jesus Christ on assignment, authorized as an apostle to proclaim God's words and acts. I write this letter to all the Christians in Rome, God's friends.");
        }

        public override BrowserNewContextOptions ContextOptions()
            => Fixture.BrowserOptions.Default;
    }
}
