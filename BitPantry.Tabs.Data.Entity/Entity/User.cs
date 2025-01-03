using BitPantry.Tabs.Common;
using System.Data.Common;

namespace BitPantry.Tabs.Data.Entity
{
    public class User : EntityBase<long>
    {
        public string EmailAddress { get; set; }
        public DateTime LastLogin { get; set; }
        public List<Card> Cards { get; set; }
        public WorkflowType? WorkflowType { get; set; }
    }
}
