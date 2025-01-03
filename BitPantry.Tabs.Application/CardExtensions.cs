using BitPantry.Tabs.Application.DTO;
using BitPantry.Tabs.Data.Entity;

namespace BitPantry.Tabs.Application
{
    public static class CardExtensions
	{
		public static CardDto ToDto(this Card card, List<Verse> verses = null)
            => new(
                card.Id,
                card.Address,
                card.AddedOn,
                card.LastMovedOn,
                card.LastReviewedOn,
                card.Tab,
                card.NumberedCard.RowNumber,
                card.ReviewCount,
                verses);

        public static async Task<CardDto> ToDtoLoadVerses(this Card card, EntityDataContext dbCtx, CancellationToken cancellationToken)
             => card.ToDto(await dbCtx.Verses.ToListAsync(card.StartVerseId, card.EndVerseId, cancellationToken));

    }
}
