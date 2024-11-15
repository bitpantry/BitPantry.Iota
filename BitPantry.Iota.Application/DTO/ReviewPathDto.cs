using BitPantry.Iota.Common;

namespace BitPantry.Iota.Application.DTO
{
    public class ReviewPathDto
    {
        public long UserId { get; private set; }
        public Dictionary<Tab, int> Path { get; private set; }

        public ReviewPathDto(long userId, Dictionary<Tab, int> path)
        {
            UserId = userId;
            Path = path;
        }

        public int CardsToReviewCount => Path.Sum(p => p.Value);
            
    }
}
