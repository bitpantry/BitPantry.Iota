using BitPantry.Tabs.Application.Service;
using BitPantry.Tabs.Common;
using Microsoft.Extensions.DependencyInjection;

namespace BitPantry.Tabs.Test
{
    public static class ServiceProviderExtensions
    {
        public static IWorkflowService GetWorkflowService(this IServiceProvider serviceProvider, WorkflowType workflowType)
        {
            switch (workflowType)
            {
                case WorkflowType.Basic:
                    return serviceProvider.GetRequiredService<BasicWorkflowService>();
                case WorkflowType.Advanced:
                    return serviceProvider.GetRequiredService<AdvancedWorkflowService>();
                default:
                    throw new ArgumentOutOfRangeException($"WorkflowType {workflowType} is not defined for this switch.");
            }
        }
    }
}
