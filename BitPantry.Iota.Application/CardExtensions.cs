using BitPantry.Iota.Application.CRQS.Set.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application
{
	public static class CardExtensions
	{
		public static CardSummaryInfo ToCardSummaryInfo(this Data.Entity.Card card)
		{
			var bible = card.Verses.First().Chapter.Book.Testament.Bible;
			var book = card.Verses.First().Chapter.Book;

			var address = PassageAddress.GetString(
				BookNameDictionary.Get(bible.Classification)[book.Number].Name,
				card.Verses.First().Chapter.Number,
				card.Verses.First().Number,
				card.Verses.Last().Chapter.Number,
				card.Verses.Last().Number,
				card.Verses.First().Chapter.Book.Testament.Bible.TranslationShortName
				);

			return new CardSummaryInfo(card.Id, address, card.Order);
		}
	}
}
