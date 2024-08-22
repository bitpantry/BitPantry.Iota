using System;

namespace BitPantry.Iota.Web.Models;

public class BiblePassage
{
    public bool IsValid { get; set; }
    public long BibleId { get; set; }
    public string TranslationShortName { get; set; }
    public string TranslationLongName { get; set; }
    public int BookNumber { get; set; }
    public string BookName { get; set; }
    public int ChapterNumber { get; set; }
    public int VerseNumberFrom { get; set; }
    public int VerseNumberTo { get; set; }
    public Dictionary<int, string> Verses { get; set; }
}
