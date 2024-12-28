using BitPantry.Iota.Application.DTO;
using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Common;
using Microsoft.Identity.Client;

namespace BitPantry.Iota.Web
{
    public class CurrentUser
    {
        private readonly UserService _userSvc;
        private readonly UserIdentity _userIdentity;
        private readonly UserTimeService _userTimeSvc;
        private UserDto _user = null;

        public long UserId => _userIdentity.UserId;
        public WorkflowType WorkflowType => GetUser()?.WorkflowType ?? WorkflowType.Basic;
        public DateTime CurrentUserLocalTime => _userTimeSvc.GetCurrentUserLocalTime();

        public CurrentUser(UserService userSvc, UserIdentity userIdentity, UserTimeService userTimeSvc)
        {
            _userSvc = userSvc;
            _userIdentity = userIdentity;
            _userTimeSvc = userTimeSvc;
        }

        private UserDto GetUser()
        {
            if (UserId == 0)
                return null;

            if(_user == null)
                _user = _userSvc.GetUser(UserId).GetAwaiter().GetResult();

            return _user;
        }

        public DateTime ConvertUtcToUserLocalTime(DateTime utcDateTime)
            => _userTimeSvc.ConvertUtcToUserLocalTime(utcDateTime);



    }
}
    