namespace BitPantry.Tabs.Web
{
    public class SessionStateException : Exception
    {
        public SessionStateException(string message, Exception innerException) : base(message, innerException) { }
    }
}
