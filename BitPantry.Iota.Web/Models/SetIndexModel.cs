namespace BitPantry.Iota.Web.Models
{
	public class SetIndexModel
    {
        public int QueueCardCount { get; set; }

        public SetCardModel Odd { get; set; }
        public SetCardModel Even { get; set; }

        public SetCardModel Sunday { get; set; }
        public SetCardModel Monday { get; set; }
        public SetCardModel Tuesday { get; set; }
        public SetCardModel Wednesday { get; set; }
        public SetCardModel Thursday { get; set; }
        public SetCardModel Friday { get; set; }
        public SetCardModel Saturday { get; set; }

        public int DaysOfTheMonthCardCount { get; set; }
    }
}
