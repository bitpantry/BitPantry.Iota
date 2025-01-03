using BitPantry.Tabs.Application.DTO;
using BitPantry.Tabs.Data.Entity;

namespace BitPantry.Tabs.Application
{
    public static class VerseExtensions
    {
        public static Dictionary<int, Dictionary<int, string>> ToVerseDictionary(this List<Verse> verses)
        {
            return verses
                .GroupBy(v => v.Chapter.Number)
                .ToDictionary(g => g.Key, g => g.ToDictionary(v => v.Number, v => v.Text));
        }

        public static PassageDto ToPassageDto(this List<Verse> verses)
        {
            var bookName = BookNameDictionary.Get(
                verses.First().Chapter.Book.Testament.Bible.Classification,
                verses.First().Chapter.Book.Number);

            var startVerse = verses.First();
            var endVerse = verses.Last();
            var bible = startVerse.Chapter.Book.Testament.Bible;

            return new PassageDto(
                bible.Id,
                startVerse.Id,
                endVerse.Id,
                bookName.Value.Name,
                startVerse.Chapter.Number,
                startVerse.Number,
                endVerse.Chapter.Number,
                endVerse.Number,
                bible.TranslationShortName,
                verses.ToVerseDictionary());
        }




    }
}
