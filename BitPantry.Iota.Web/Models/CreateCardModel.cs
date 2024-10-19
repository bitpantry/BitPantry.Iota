using BitPantry.Iota.Application;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace BitPantry.Iota.Web.Models;

public class CreateCardModel
{
    public string LastAction { get; set; }

    public bool IsValidAddress { get; set; }

    public bool IsCardAlreadyCreated { get; set; }

    public PassageModel Passage { get; set; }

    public List<SelectListItem> Bibles { get; set; }

    public string Address
        => PassageAddress.GetString(
            Passage.BookName, 
            Passage.FromChapterNumber, 
            Passage.FromVerseNumber, 
            Passage.ToChapterNumber, 
            Passage.ToVerseNumber);
}
