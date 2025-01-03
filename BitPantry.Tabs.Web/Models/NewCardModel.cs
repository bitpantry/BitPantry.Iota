using BitPantry.Tabs.Common;

namespace BitPantry.Tabs.Web.Models;

public class NewCardModel
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
    public WorkflowType WorkflowType { get; set; }
}
