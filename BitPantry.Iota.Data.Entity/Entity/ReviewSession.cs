using Azure;
using BitPantry.Iota.Common;
using System;
using System.Collections.Generic;
using System.IO;
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
        public string CardIdsToIgnore { get; set; }
        public string ReviewPath { get; set; }

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
            ReviewPath = null;
        }

        public List<long> GetCardsToIgnoreList()
            => CardIdsToIgnore == null
                ? []
                : CardIdsToIgnore.Split(',').Select(s => long.Parse(s)).ToList();

        public void AddCardToIgnore(long cardId)
        {
            if (string.IsNullOrWhiteSpace(CardIdsToIgnore))
                CardIdsToIgnore = cardId.ToString();
            else
                CardIdsToIgnore += $",{cardId}";
        }

        public void SetReviewPath(Dictionary<Tab, int> reviewPath)
        {
            ReviewPath = reviewPath.Select(i => $"{(int)i.Key}:{i.Value}").Aggregate((a, b) => $"{a},{b}");
        }

        public Dictionary<Tab, int> GetReviewPath()
            => ReviewPath == null 
                ? [] 
                : ReviewPath.Split(',')
                    .Select(part => part.Split(':'))
                    .ToDictionary(
                        parts => (Tab)int.Parse(parts[0]),
                        parts => int.Parse(parts[1]));
    }
}
