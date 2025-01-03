using BitPantry.Tabs.Common;

namespace BitPantry.Tabs.Test.Application
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
