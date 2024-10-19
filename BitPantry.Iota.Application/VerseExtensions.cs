using BitPantry.Iota.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application
{
    public static class VerseExtensions
    {
        public static Dictionary<int, Dictionary<int, string>> ToVerseDictionary(this List<Verse> verses)
        {
            return verses
                .GroupBy(v => v.Chapter.Number)
                .ToDictionary(g => g.Key, g => g.ToDictionary(v => v.Number, v => v.Text));
        }
    }
}
