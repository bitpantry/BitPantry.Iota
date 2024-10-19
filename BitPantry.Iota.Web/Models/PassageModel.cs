using BitPantry.Iota.Application;

namespace BitPantry.Iota.Web.Models
{
    public record PassageModel(
        long BibleId = 0,
        string BookName = null,
        int FromChapterNumber = 0,
        int FromVerseNumber = 0,
        int ToChapterNumber = 0,
        int ToVerseNumber = 0,
        string Address = null,
        Dictionary<int, Dictionary<int, string>> Verses = null)
    {
    }
}
