using BitPantry.Tabs.Application.DTO;
using BitPantry.Tabs.Application.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Tabs.Application
{
    public record GetPassageResponse(PassageDto Passage, PassageAddressParsingException ParsingException)
    {
    }
}
