using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.Service
{
    public class CardReviewService
    {
        private CardService _cardSvc;

        public CardReviewService(CardService cardSvc)
        {
            _cardSvc = cardSvc;
        }

        public async Task<GetCardResult> GetNextCardForReview(long userId, Divider? lastDivider, int lastCardOrder)
        {
            // initialize the last divider to the queue if it is null

            lastDivider ??= Divider.Queue;

            Divider nextDivider = lastDivider.Value;

            // if not a multi-card review divider, get the next review divider

            if (lastDivider <= Divider.Day1)
                nextDivider = GetNextReviewDivider(lastDivider);

            // if advanced divider, reset order, otherwise increment order for same divider

            if (nextDivider != lastDivider)
                lastCardOrder = 1;
            else
                lastCardOrder++;

            // try to get the card and return if card found

            var card = await _cardSvc.TryGetCard(userId, nextDivider, lastCardOrder);

            if (card != null)
                return card;

            // if no card found, recursively call this function to increment to the next divider, or return null if at the end of the review path

            if (nextDivider < Divider.Day1)
                return await GetNextCardForReview(userId, nextDivider, lastCardOrder);
            else
                return null;
        }

        private Divider GetNextReviewDivider(Divider? lastDivider)
        {
            return lastDivider switch
            {
                Divider.Queue => Divider.Daily,
                Divider.Daily => DateTime.Today.Day % 2 == 0 ? Divider.Even : Divider.Odd,
                Divider.Odd or Divider.Even => DateTime.Today.DayOfWeek switch
                {
                    DayOfWeek.Sunday => Divider.Sunday,
                    DayOfWeek.Monday => Divider.Monday,
                    DayOfWeek.Tuesday => Divider.Tuesday,
                    DayOfWeek.Wednesday => Divider.Wednesday,
                    DayOfWeek.Thursday => Divider.Thursday,
                    DayOfWeek.Friday => Divider.Friday,
                    DayOfWeek.Saturday => Divider.Saturday,
                    _ => throw new ArgumentOutOfRangeException("DateTime.Today.DayOfWeek", DateTime.Today.DayOfWeek, "A divider is not defined for this day of the week")
                },
                Divider.Sunday or Divider.Monday or Divider.Tuesday or Divider.Wednesday or Divider.Thursday or Divider.Friday or Divider.Saturday => DateTime.Today.Day + Divider.Saturday,
                _ => throw new ArgumentOutOfRangeException(nameof(lastDivider), lastDivider.Value, "No review path is defined for this divider")
            };
        }
    }
}
