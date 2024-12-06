using BitPantry.Iota.Application;
using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Common;

namespace BitPantry.Iota.Web
{
    public class WebIdentityWorkflowServiceProvider : IWorkflowServiceProvider
    {
        private readonly UserIdentity _userIdentity;
        private readonly UserService _userSvc;
        private readonly IServiceProvider _serviceProvider;

        public WebIdentityWorkflowServiceProvider(UserIdentity userIdentity, UserService userSvc, IServiceProvider serviceProvider) 
        {
            _userIdentity = userIdentity;
            _userSvc = userSvc;
            _serviceProvider = serviceProvider;
        }

        public IWorkflowService GetWorkflowService()
        {
            if (_userIdentity.UserId == 0)
                return _serviceProvider.GetRequiredService<BasicWorkflowService>();

            var user = _userSvc.GetUser(_userIdentity.UserId).GetAwaiter().GetResult();

            if (!user.WorkflowType.HasValue)
                return _serviceProvider.GetRequiredService<BasicWorkflowService>();

            switch (user.WorkflowType)
            {
                case WorkflowType.Basic:
                    return _serviceProvider.GetRequiredService<BasicWorkflowService>();
                case WorkflowType.Advanced:
                    return _serviceProvider.GetRequiredService<AdvancedWorkflowService>();
                default:
                    throw new ArgumentOutOfRangeException($"WorkflowType {user.WorkflowType} is not defined for this sitch");
            }
        }
    }
}
