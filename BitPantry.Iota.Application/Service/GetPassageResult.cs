using BitPantry.Iota.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.Service
{
    public enum GetPassageResultCode : int
    {
        Ok = 0,
        CannotParseAddress = 1,
        BookNotFound = 2
    }

    public class GetPassageResult
    {
        public GetPassageResultCode Code { get; set; }
        public PassageDto Passage { get; set; }

    }

    

    
}
