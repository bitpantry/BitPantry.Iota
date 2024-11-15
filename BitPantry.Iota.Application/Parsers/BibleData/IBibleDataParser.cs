using BitPantry.Iota.Data.Entity;

namespace BitPantry.Iota.Application.Parsers.BibleData
{
    internal interface IBibleDataParser
    {
        public Bible Parse(string dataFilePath);
    }
}
