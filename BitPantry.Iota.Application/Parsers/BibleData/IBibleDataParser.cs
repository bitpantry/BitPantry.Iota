using BitPantry.Iota.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.Parsers.BibleData
{
    internal interface IBibleDataParser
    {
        public Bible Parse(string dataFilePath);
    }
}
