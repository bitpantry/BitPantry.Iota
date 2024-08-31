using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application
{
    public static class PassageAddress
    {
        public static string GetString(string bookName, int fromChapter, int fromVerse, int toChapter, int toVerse, string translation = null)
        {
            var addy = new StringBuilder($"{bookName} {fromChapter}:{fromVerse}");

            if(toVerse != fromVerse & toChapter == fromChapter)
                addy.Append($"-{toVerse}");
            else if(toChapter != fromChapter)
                addy.Append($"-{toChapter}:{toVerse}");

            if (!string.IsNullOrWhiteSpace(translation))
                addy.Append($" ({translation})");

            return addy.ToString();
        }
    }
}
