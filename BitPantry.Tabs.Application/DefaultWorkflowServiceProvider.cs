using BitPantry.Tabs.Application;
using BitPantry.Tabs.Application.Service;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Tabs.Test
{
    public class DefaultWorkflowServiceProvider : IWorkflowServiceProvider
    {
        private readonly WorkflowServiceSwitch _svcSwitch;
        private readonly IServiceProvider _serviceProvider;

        public DefaultWorkflowServiceProvider(WorkflowServiceSwitch svcSwitch, IServiceProvider serviceProvider) 
        { 
            _svcSwitch = svcSwitch;
            _serviceProvider = serviceProvider;
        }

        public IWorkflowService GetWorkflowService()
        {
            switch (_svcSwitch.WorkflowServiceType)
            {
                case Common.WorkflowType.Basic:
                    return _serviceProvider.GetRequiredService<BasicWorkflowService>();
                case Common.WorkflowType.Advanced:
                    return _serviceProvider.GetRequiredService<AdvancedWorkflowService>();
                default:
                    throw new ArgumentException($"The workflow type, '{_svcSwitch.WorkflowServiceType}' is not defined for this switch");
            }
        }
    }
}
