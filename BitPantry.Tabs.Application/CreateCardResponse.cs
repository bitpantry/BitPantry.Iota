using BitPantry.Tabs.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Tabs.Application
{
    public enum CreateCardResponseResult
    {
        Ok,
        InvalidAddress,
        BookNameUnresolved,
        CardAlreadyExists
    }

    public record CreateCardResponse(
        CreateCardResponseResult Result,
        CardDto Card)
    {
    }
}
