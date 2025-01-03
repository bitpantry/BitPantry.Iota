using BitPantry.Tabs.Common;

namespace BitPantry.Tabs.Web.Models
{
    public class TabsModel
    {
        public Tab ActiveTab { get; set; }
        public Tab[] WeekdaysWithData { get; set; }
        public Tab[] DaysOfMonthWithData { get; set; }
        public List<CardModel> Cards { get; set; }
        public string BackUrl { get; set; }
        public WorkflowType WorkflowType { get; set; }

        public CardMaintenanceModel TopCardMaintenanceModel => new CardMaintenanceModel(Cards.FirstOrDefault(), WorkflowType);

    }
}
