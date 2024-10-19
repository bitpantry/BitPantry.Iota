namespace BitPantry.Iota.Web
{
    public class ReviewProgress
    {
        public List<long> CardIdsPromoted { get; set; } = new List<long>();
        
        private long _startTime = DateTime.Now.Ticks;
        public TimeSpan TimeElapsed => new TimeSpan(DateTime.Now.Ticks - _startTime);
    }
}
