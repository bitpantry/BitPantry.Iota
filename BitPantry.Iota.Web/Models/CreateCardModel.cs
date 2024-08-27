using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace BitPantry.Iota.Web.Models;

public class CreateCardModel
{
    public bool IsHttpPost { get; set; }
    public bool IsSuccessfulCreate { get; set; }

    public bool IsValidAddress { get; set; }

    public long BibleId { get; set; }
    public string BookName { get; set; }
    public int ChapterNumber { get; set; }
    public int FromVerseNumber { get; set; }
    public int ToVerseNumber { get; set; }

    public Dictionary<int, string> Verses { get; set; }

    public List<SelectListItem> Bibles { get; set; }

    public string Address
    {
        get
        {
            var address = $"{BookName} {ChapterNumber}:{FromVerseNumber}";
            if (ToVerseNumber > FromVerseNumber)
                address += $"-{ToVerseNumber}";

            return address;
        }
    }
}
