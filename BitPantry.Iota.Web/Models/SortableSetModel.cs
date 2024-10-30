using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using System.Net;

namespace BitPantry.Iota.Web.Models
{
    public record SortableSetModel(string SetName, Tab Tab, List<SetCardModel> Cards, string BackAction, string CardBackUrl) { }
}
