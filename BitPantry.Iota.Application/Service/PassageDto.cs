using BitPantry.Iota.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.Service
{
    public class PassageDto
    {
        public long BibleId { get; internal set; }
        public string BookName { get; internal set; }
        public int FromChapterNumber { get; internal set; }
        public int FromVerseNumber { get; set; }
        public int ToChapterNumber { get; set; }
        public int ToVerseNumber { get; set; }
        public List<Verse> Verses { get; set; }

    }
}
