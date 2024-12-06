using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Test
{
    public interface IHaveServiceProvider
    {
        public IServiceProvider ServiceProvider { get; }
    }
}
    