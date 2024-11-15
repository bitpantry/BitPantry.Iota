namespace BitPantry.Iota.Application.Parsers.BibleData
{
    internal class BibleDataParsingException : Exception
    {
        public BibleDataParsingException(string message) : base(message) { }
        public BibleDataParsingException(string message,  Exception innerException) : base(message, innerException) { }
    }
}
