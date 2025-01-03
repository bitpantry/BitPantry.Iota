using BitPantry.Tabs.Application;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Tabs.Test
{
    public static class IServiceScopeExtensions
    {
        public static void UseAdvancedWorkflow(this IServiceScope scope)
        {
            scope.ServiceProvider.GetRequiredService<WorkflowServiceSwitch>().WorkflowServiceType = Common.WorkflowType.Advanced;
        }
    }
}
