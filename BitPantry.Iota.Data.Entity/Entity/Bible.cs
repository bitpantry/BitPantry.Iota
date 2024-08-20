using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Data.Entity
{
    public enum BibleClassification : int
    {
        Protestant = 0,
        Catholic = 1
    }

    public class Bible : EntityBase<long>
    {
        public string TranslationShortName { get; set; }
        public string TranslationLongName { get; set; }
        public BibleClassification Classification { get; set; }
        public string Description { get; set; }
        public List<Testament> Testaments { get; set; }
    }
}
