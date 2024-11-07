using BitPantry.Iota.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.DTO
{
    public record PassageDto(
        long BibleId,
        long StartVerseId,
        long EndVerseId,
        string BookName,
        int FromChapterNumber,
        int FromVerseNumber,
        int ToChapterNumber,
        int ToVerseNumber,
        string TranslationShortName,
        Dictionary<int, Dictionary<int, string>> Verses)
    {

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

        public override string ToString() => GetAddressString();

    }
}
