using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Infrastructure.Parsing
{
    public class BibleDataParsingException : Exception
    {
        public BibleDataParsingException(string message) : base(message) { }
        public BibleDataParsingException(string message,  Exception innerException) : base(message, innerException) { }
    }
}
