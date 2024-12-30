using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;

namespace BitPantry.Iota.Application.DTO
{
    public class CardDto
    {
        public long Id { get; }
        public string Address { get; }
        public DateTime AddedOn { get; }
        public DateTime LastMovedOn { get; }
        public DateTime? LastReviewedOn { get; }
        public Tab Tab { get; }
        public long RowNumber { get; }
        public int ReviewCount { get; }
        public PassageDto Passage { get; }

        public CardDto(
            long id, 
            string address,
            DateTime addedOn, 
            DateTime lastMovedOn, 
            DateTime? lastReviewedOn, 
            Tab tab, 
            long rowNumber,
            int reviewCount,
            List<Verse> verses = null)
        {
            Id = id;
            Address = address;
            AddedOn = addedOn;
            LastMovedOn = lastMovedOn;
            LastReviewedOn = lastReviewedOn;
            Tab = tab;
            ReviewCount = reviewCount;
            RowNumber = rowNumber;

            if (verses != null)
                Passage = verses.ToPassageDto();
        }

    }
}