using BitPantry.Iota.Application;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace BitPantry.Iota.Web.Models;

public class CreateCardModel
{
    public string LastAction { get; set; }

    public bool IsValidAddress { get; set; }

    public long BibleId { get; set; }
    public string BookName { get; set; }
    public int FromChapterNumber { get; set; }
    public int FromVerseNumber { get; set; }
    public int ToChapterNumber { get; set; }
    public int ToVerseNumber { get; set; }

    public Dictionary<int, Dictionary<int, string>> Verses { get; set; }

    public List<SelectListItem> Bibles { get; set; }

    public string Address
        => PassageAddress.GetString(BookName, FromChapterNumber, FromVerseNumber, ToChapterNumber, ToVerseNumber);
}
