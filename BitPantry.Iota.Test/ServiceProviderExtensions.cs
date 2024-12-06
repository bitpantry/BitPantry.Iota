using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Common;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Test
{
    internal static class ServiceProviderExtensions
    {
        public static IWorkflowService GetWorkflowService(this IServiceProvider serviceProvider, WorkflowType workflowType)
        {
            switch (workflowType)
            {
                case WorkflowType.Basic:
                    return serviceProvider.GetService<BasicWorkflowService>();
                case WorkflowType.Advanced:
                    return serviceProvider.GetService<AdvancedWorkflowService>();
                default:
                    throw new ArgumentOutOfRangeException($"WorkflowType {workflowType} is not defined for this switch.");
            }
        }
    }
}
