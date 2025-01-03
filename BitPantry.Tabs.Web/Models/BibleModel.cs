namespace BitPantry.Tabs.Web.Models
{
    public record BibleModel(long Id, string LongName, string ShortName) 
    { 
        public string DisplayName => $"{LongName} ({ShortName})";
    }
}
