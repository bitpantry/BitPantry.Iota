using BitPantry.Iota.Application;
using BitPantry.Iota.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace BitPantry.Iota.Web.Models;

public class CreateCardModel
{

    public bool IsValidAddress { get; set; }

    public bool IsCardAlreadyCreated { get; set; }

    public PassageModel Passage { get; set; }

    public List<BibleModel> Bibles { get; set; }

    public long BibleId { get; set; }
    public long CreatedCardId { get; internal set; }
    public Tab? CreatedToTab { get; internal set; }
    public string AddressQuery { get; internal set; }
    public string CreatedAddress { get; internal set; }
}
