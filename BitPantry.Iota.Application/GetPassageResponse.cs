using BitPantry.Iota.Application.DTO;
using BitPantry.Iota.Application.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application
{
    public record GetPassageResponse(PassageDto Passage, PassageAddressParsingException ParsingException)
    {
    }
}
