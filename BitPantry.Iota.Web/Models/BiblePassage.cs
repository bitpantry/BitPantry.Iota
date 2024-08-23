using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace BitPantry.Iota.Web.Models;

public class BiblePassage
{
    public bool IsValidAddress { get; set; }
    public string QueryAddress { get; set; }
    public long BibleId { get; set; }
    public string BookName { get; set; }
    public int ChapterNumber { get; set; }
    public int VerseNumberFrom { get; set; }
    public int VerseNumberTo { get; set; }
    public Dictionary<int, string> Verses { get; set; }


    public List<SelectListItem> Bibles { get; set; }

    public string Address
    {
        get
        {
            var address = $"{BookName} {ChapterNumber}:{VerseNumberFrom}";
            if (VerseNumberTo > VerseNumberFrom)
                address += $"-{VerseNumberTo}";

            return address;
        }
    }
}
