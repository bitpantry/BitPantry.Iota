namespace BitPantry.Iota.Application.Parsers.BibleData
{
    public class BibleDataParsingException : Exception
    {
        public BibleDataParsingException(string message) : base(message) { }
        public BibleDataParsingException(string message,  Exception innerException) : base(message, innerException) { }
    }
}
