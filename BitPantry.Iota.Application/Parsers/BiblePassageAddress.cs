using System;
using System.Text.RegularExpressions;

namespace BitPantry.Iota.Application.Parsers;

public class BiblePassageAddress
{
    public bool IsValid { get; private set; } = true;
    public string Book { get; private set; }
    public int FromChapterNumber { get; private set; }
    public int FromVerseNumber { get; private set; }
    public int ToChapterNumber { get; set; }
    public int ToVerseNumber { get; private set; }


    public BiblePassageAddress(string addressString)
    {
        var regex = new Regex(@"^\s*(?<Book>\d?\s?[A-Za-z\.]+)\s*(?<Chapter>\d+):(?<StartVerse>\d+)\s*(?:-\s*(?<EndChapter>\d+):(?<EndVerse>\d+)|-\s*(?<EndVerseOnly>\d+))?\s*$");
        var match = regex.Match(addressString);

        if (!match.Success)
        {
            IsValid = false;
        }
        else
        {
            Book = match.Groups["Book"].Value.Trim();
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
    }

    
}
