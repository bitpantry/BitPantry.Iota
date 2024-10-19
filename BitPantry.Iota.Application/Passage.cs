using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application
{
    public record Passage(
        long BibleId = 0,
        string BookName = null,
        int FromChapterNumber = 0,
        int FromVerseNumber = 0,
        int ToChapterNumber = 0,
        int ToVerseNumber = 0,
        Dictionary<int, Dictionary<int, string>> Verses = null)
    {

        public string Address
            => PassageAddress.GetString(
                BookName,
                FromChapterNumber,
                FromVerseNumber,
                ToChapterNumber,
                ToVerseNumber);
    }
}
