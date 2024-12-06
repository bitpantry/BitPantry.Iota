using BitPantry.Iota.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.DTO
{
    public record UserDto(long Id, string EmailAddress, WorkflowType? WorkflowType)
    {
    }
}
