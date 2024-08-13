using BitPantry.Iota.Application.CRQS.Identity.Common;

namespace BitPantry.Iota.Web.Identity
{
    public class IotaWebUser
    {
        public long Id { get; }
        public string EmailAddress { get; private set; }
        public string NormalizedEmailAddress { get; private set; }
        public string Password { get; private set; }
        public string PasswordHash { get; private set; }
        public bool IsEmailConfirmed { get; set; }

        public IotaWebUser(FindUserResponse response)
        {
            Id = response.Id;
            EmailAddress = response.EmailAddress;
            NormalizedEmailAddress = response.NormalizedEmailAddress;
            Password = response.Password;
            PasswordHash = response.PasswordHash;
            IsEmailConfirmed = response.IsEmailConfirmed;
        }

        public void SetPassword(string password, string passwordHash)
        {
            Password = password;
            PasswordHash = passwordHash;
        }

        public void SetEmailAddress(string emailAddress)
        {
            EmailAddress = emailAddress;
            NormalizedEmailAddress = emailAddress.ToUpper();
        }
    }
}
