using BitPantry.Iota.Data.Entity;
using System.Xml.Linq;

namespace BitPantry.Iota.Application.Parsers.BibleData
{
    public interface IBibleDataParser
    {
        public Bible Parse(string dataFilePath);
        public Bible Parse(Stream stream);
    }
}
