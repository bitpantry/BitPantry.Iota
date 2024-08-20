using BitPantry.Iota.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Infrastructure.Parsing
{
    public interface IBibleParser
    {
        public Bible Parse(string dataFilePath);
    }
}
