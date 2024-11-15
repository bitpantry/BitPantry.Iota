using BitPantry.Iota.Data.Entity;
using System.Text;
using System.Text.RegularExpressions;

namespace BitPantry.Iota.Application.Parsers;

internal class PassageAddressParser
{
    public string RawValue { get; }
    public int BookNumber { get; }
    public string BookName { get; }
    public int FromChapterNumber { get; }
    public int FromVerseNumber { get; }
    public int ToChapterNumber { get; }
    public int ToVerseNumber { get; }
    public long BibleId { get; }
    public string TranslationShortName { get; }


    public PassageAddressParser(Bible bible, string addressString)
    {
        RawValue = addressString;
        BibleId = bible.Id;
        TranslationShortName = bible.TranslationShortName;

        // check for null address

        if (string.IsNullOrEmpty(addressString))
            throw new ArgumentNullException(nameof(addressString));

        // parse the raw address text

        var regex = new Regex(@"^\s*(?<Book>\d?\s?[A-Za-z\.]+)\s*(?<Chapter>\d+):(?<StartVerse>\d+)\s*(?:-\s*(?<EndChapter>\d+):(?<EndVerse>\d+)|-\s*(?<EndVerseOnly>\d+))?\s*$");
        var match = regex.Match(addressString);

        if (!match.Success)
        {
            throw new PassageAddressParsingException(bible.Id, addressString, PassageAddressParsingExceptionCode.InvalidAddress, "The address is invalid");
        }
        else
        {
            BookName = match.Groups["Book"].Value.Trim();
            FromChapterNumber = int.Parse(match.Groups["Chapter"].Value);
            FromVerseNumber = int.Parse(match.Groups["StartVerse"].Value);

            if (match.Groups["EndVerseOnly"].Success)
            {
                ToChapterNumber = FromChapterNumber;
                ToVerseNumber = int.Parse(match.Groups["EndVerseOnly"].Value);
            }
            else if (match.Groups["EndChapter"].Success)
            {
                ToChapterNumber = int.Parse(match.Groups["EndChapter"].Value);
                ToVerseNumber = int.Parse(match.Groups["EndVerse"].Value);
            }
            else
            {
                ToChapterNumber = FromChapterNumber;
                ToVerseNumber = FromVerseNumber;
            }
        }

        // resolve book name

        var bookNameInfo = BookNameDictionary.Get(bible.Classification, BookName);

        if (bookNameInfo.Key == 0)
            throw new PassageAddressParsingException(bible.Id, addressString, PassageAddressParsingExceptionCode.BookNameUnresolved, "The book name could not be found");

        BookNumber = bookNameInfo.Key;
        BookName = bookNameInfo.Value.Name;
    }

    public string GetAddressString(bool includeTranslation = false)
    {
        var addy = new StringBuilder($"{BookName} {FromChapterNumber}:{FromVerseNumber}");

        if (ToVerseNumber != FromVerseNumber & ToChapterNumber == FromChapterNumber)
            addy.Append($"-{ToVerseNumber}");
        else if (ToChapterNumber != FromChapterNumber)
            addy.Append($"-{ToChapterNumber}:{ToVerseNumber}");

        if (includeTranslation && !string.IsNullOrWhiteSpace(TranslationShortName))
            addy.Append($" ({TranslationShortName})");

        return addy.ToString();
    }

    public override string ToString()
        => GetAddressString();
}

public enum PassageAddressParsingExceptionCode
{
    InvalidAddress,
    BookNameUnresolved
}

public class PassageAddressParsingException : Exception
{
    public long BibleId { get; }
    public string Address { get; }
    public PassageAddressParsingExceptionCode Code { get; set; }

    public PassageAddressParsingException(long bibleId, string address, PassageAddressParsingExceptionCode code, string message) : base(message)
    {
        BibleId = bibleId;
        Address = address;
        Code = code;
    }
}
