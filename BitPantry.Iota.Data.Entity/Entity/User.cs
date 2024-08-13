namespace BitPantry.Iota.Data.Entity
{
    public class User : EntityBase<long>
    {
        public string EmailAddress { get; set; }
        public string NormalizedEmailAddress { get; set; }
        public string Password { get; set; }
        public string PasswordHash { get; set; }
        public bool IsEmailConfirmed { get; set; }
    }
}
