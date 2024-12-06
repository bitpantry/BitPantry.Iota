using BitPantry.Iota.Application.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application
{
    public interface IWorkflowServiceProvider
    {
        public IWorkflowService GetWorkflowService();
    }
}
