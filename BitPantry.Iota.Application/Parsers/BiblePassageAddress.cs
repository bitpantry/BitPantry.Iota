using System;
using System.Text.RegularExpressions;

namespace BitPantry.Iota.Application.Parsers;

public class BiblePassageAddress
{
    public bool IsValid { get; private set; } = true;
    public string Book { get; private set; }
    public int Chapter { get; private set; }
    public int VerseStart { get; private set; }
    public int VerseEnd { get; private set; }


    public BiblePassageAddress(string addressString)
    {
        var regex = new Regex(@"^(?<Book>\d?\s?[A-Za-z\.]+)\s?(?<Chapter>\d+):(?<StartVerse>\d+)(?:-(?<EndVerse>\d+))?$");
        var match = regex.Match(addressString);

        if (!match.Success)
        {
            IsValid = false;
        }
        else
        {
            Book = match.Groups["Book"].Value.Trim();
            Chapter = int.Parse(match.Groups["Chapter"].Value);
            VerseStart = int.Parse(match.Groups["StartVerse"].Value);
            VerseEnd = match.Groups["EndVerse"].Success ? int.Parse(match.Groups["EndVerse"].Value) : 0;
        }
    }

    
}
