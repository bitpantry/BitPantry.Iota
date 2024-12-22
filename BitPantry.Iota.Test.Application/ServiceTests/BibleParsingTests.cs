using BitPantry.Iota.Application.Parsers.BibleData;
using FluentAssertions;

namespace BitPantry.Iota.Test.Application.ServiceTests
{
    public class BibleParsingTests
    {
        [Fact]
        public void ParseBible_BibleParsed()
        {
            IBibleDataParser parser = new DefaultXmlBibleDataParser();

            var bible = parser.Parse(new MemoryStream(TestResources.Translations.MSG));

            bible.Should().NotBeNull();

            bible.TranslationShortName.Should().Be("MSG");
            bible.TranslationLongName.Should().Be("The Message");
            bible.Description.Should().Be("The Message: The Bible in Contemporary Language. Copyright © 2002 by Eugene H. Peterson. All rights reserved. Published by NavPress, a ministry of The Navigators.");
            bible.Classification.Should().Be(Data.Entity.BibleClassification.Protestant);

            bible.Testaments.Should().HaveCount(2);
            bible.Testaments[0].Books.Should().HaveCount(39);
            bible.Testaments[1].Books.Should().HaveCount(27);
        }

        [Fact]
        public void ParseBibleWithBadClassification_Exception()
        {
            IBibleDataParser parser = new DefaultXmlBibleDataParser();

            parser.Invoking(p => p.Parse(new MemoryStream(TestResources.Translations.ESV_badClassification)))
                .Should()
                .Throw<BibleDataParsingException>()
                .Subject.First().Message.Should().StartWith("Invalid classification value:");
        }

        [Fact]
        public void ParseBibleWithBadTestamentName_Exception()
        {
            IBibleDataParser parser = new DefaultXmlBibleDataParser();

            parser.Invoking(p => p.Parse(new MemoryStream(TestResources.Translations.ESV_badTestamentName)))
                .Should()
                .Throw<BibleDataParsingException>()
                .Subject.First().Message.Should().StartWith("Invalid testament name value:");
        }
    }
}
