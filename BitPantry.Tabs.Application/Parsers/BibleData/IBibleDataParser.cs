using BitPantry.Tabs.Data.Entity;
using System.Xml.Linq;

namespace BitPantry.Tabs.Application.Parsers.BibleData
{
    public interface IBibleDataParser
    {
        public Bible Parse(string dataFilePath);
        public Bible Parse(Stream stream);
    }
}
