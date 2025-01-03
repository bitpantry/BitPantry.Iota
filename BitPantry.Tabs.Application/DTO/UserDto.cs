using BitPantry.Tabs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Tabs.Application.DTO
{
    public record UserDto(long Id, string EmailAddress, WorkflowType? WorkflowType)
    {
    }
}
