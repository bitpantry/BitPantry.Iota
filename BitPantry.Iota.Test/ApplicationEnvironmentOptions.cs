using BitPantry.Iota.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace BitPantry.Iota.Test
{
    public class ApplicationEnvironmentOptions
    {
        public WorkflowType WorkflowType { get; private set; } = WorkflowType.Basic;

        public ApplicationEnvironmentOptions UseWorkflow(WorkflowType workflowType)
        {
            WorkflowType = workflowType;
            return this;
        }
    }
}
