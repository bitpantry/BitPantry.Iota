namespace BitPantry.Tabs.Application.Service
{
    public enum CreateCardExceptionCode
    {
        CardAlreadyExists
    }
    public class CreateCardException : Exception
    {
        public CreateCardExceptionCode Code { get; }
        public long BibleId { get; }
        public string AddressString { get; }

        public CreateCardException(CreateCardExceptionCode code, long bibleId, string addressString, string message) : base(message)
        {
            Code = code; 
            BibleId = bibleId;
            AddressString = addressString;
        }
    }
}
