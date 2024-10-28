using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Data.Entity
{
    public class ReviewSession : EntityBase<long>
    {
        public long UserId { get; set; }
        public User User { get; set; }

        public DateTime StartedOn { get; set; }
        public DateTime LastAccessed { get; set; }
        public String CardIdsToIgnore { get; set; }

        public ReviewSession()
        {
            Reset();
        }

        public ReviewSession(long userId)
        {
            UserId = userId;
            Reset();
        }

        public bool IsExpired()
            => DateTime.UtcNow.Subtract(LastAccessed).TotalMinutes > 30;

        public void Reset()
        {
            StartedOn = DateTime.UtcNow;
            LastAccessed = DateTime.UtcNow;
            CardIdsToIgnore = null;
        }

        public List<long> GetCardsToIgnoreList()
        {
            if (string.IsNullOrWhiteSpace(CardIdsToIgnore))
                return new List<long>();

            return CardIdsToIgnore.Split(',').Select(s => long.Parse(s)).ToList();
        }

        public void AddCardToIgnore(long cardId)
        {
            if (string.IsNullOrWhiteSpace(CardIdsToIgnore))
                CardIdsToIgnore = cardId.ToString();
            else
                CardIdsToIgnore += $",{cardId}";
        }


    }
}
