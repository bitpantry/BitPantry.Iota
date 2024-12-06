using BitPantry.Iota.Common;

namespace BitPantry.Iota.Web.Models
{
    public class TabsModel
    {
        public Tab ActiveTab { get; set; }
        public Tab[] WeekdaysWithData { get; set; }
        public Tab[] DaysOfMonthWithData { get; set; }
        public List<CardModel> Cards { get; set; }
        public string BackUrl { get; set; }
    }
}
