using BitPantry.Iota.Application.DTO;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.Logic
{
    public class ReviewLogic
    {
        private ILogger<ReviewLogic> _logger;

        public ReviewLogic(ILogger<ReviewLogic> logger)
        {
            _logger = logger;
        }

        public async Task<Tuple<ReviewSession, bool>> GetReviewSessionCommand(EntityDataContext dbCtx, long userId, bool startNewSession = false)
        {
            // see if active session is available
            var currentSession = await dbCtx.ReviewSessions.SingleOrDefaultAsync(r => r.UserId == userId);
            bool isNew = true;

            // Check if the session has been accessed within the last 30 minutes
            if (currentSession != null)
            {
                if (startNewSession || currentSession.IsExpired())
                {
                    _logger.LogDebug("Resetting review session");
                    currentSession.Reset();
                    currentSession.SetReviewPath(await BuildReviewPathQuery(dbCtx, userId));
                }
                else
                {
                    currentSession.LastAccessed = DateTime.UtcNow;
                    isNew = false;
                }

            }
            else
            {
                currentSession = new ReviewSession(userId);
                dbCtx.ReviewSessions.Add(currentSession);
            }

            return new Tuple<ReviewSession, bool>(currentSession, isNew);
        }

        // ...

        private async Task<Dictionary<Tab, int>> BuildReviewPathQuery(EntityDataContext dbCtx, long userId)
        {
            var path = new Dictionary<Tab, int>();
            var currentTab = Tab.Queue;

            do
            {
                currentTab = GetNextReviewTab(currentTab);
                path.Add(currentTab, 0);

            } while (currentTab < Tab.Day1);

            // select the card count grouped by tab for the userId

            var cardCounts = await dbCtx.Cards
                .Where(card => card.UserId == userId)
                .GroupBy(card => card.Tab)
                .Select(group => new
                {
                    Tab = group.Key,
                    CardCount = group.Count()
                })
                .ToListAsync();

            foreach (var count in cardCounts)
                path[(Tab)count.Tab] = count.CardCount;          

            return path;
        }

        public async Task<CardDto> GetNextCardForReviewCommand(EntityDataContext dbCtx, long userId, Tab? currentTab, int currentCardIndex)
        {
            // initialize the last tab to the queue if it is null

            currentTab ??= Tab.Queue;
            Tab nextTab = currentTab.Value;

            // load review session

            var session = await GetReviewSessionCommand(dbCtx, userId);

            return await GetNextCardForReviewCommand_RECURSIVE(dbCtx, userId, session.Item1, currentTab.Value, currentCardIndex);
        }

        private async Task<CardDto> GetNextCardForReviewCommand_RECURSIVE(EntityDataContext dbCtx, long userId, ReviewSession session, Tab lastTab, int lastCardOrder)
        {
            Tab nextTab = lastTab;

            // if not a multi-card review tab, get the next review tab

            if (lastTab < Tab.Day1)
                nextTab = GetNextReviewTab(lastTab);

            // if advancing tab, reset order, otherwise increment order for same tab

            var nextCardOrder = nextTab != lastTab ? 1 : lastCardOrder + 1;

            // see if the card id exists and is not in the ignore list for the session

            var nextCard = await dbCtx.Cards.AsNoTracking()
                .Where(c => c.UserId == userId && c.Tab == nextTab && c.Order == nextCardOrder)
                .SingleOrDefaultAsync();

            if (nextCard != null)
            {
                if (!session.GetCardsToIgnoreList().Contains(nextCard.Id))
                    return nextCard.ToDto();
                else
                    _logger.LogDebug("Card id {Id} found in review session ignore list and will be skipped", nextCard.Id);
            }

            // if no card found, recursively call this function to increment to the next tab, or return null if at the end of the review path

            if (nextTab < Tab.Day1)
                return await GetNextCardForReviewCommand(dbCtx, userId, nextTab, lastCardOrder);
            else
                return null;
        }

        private Tab GetNextReviewTab(Tab? lastTab)
        {
            return lastTab switch
            {
                Tab.Queue => Tab.Daily,
                Tab.Daily => DateTime.Today.Day % 2 == 0 ? Tab.Even : Tab.Odd,
                Tab.Odd or Tab.Even => DateTime.Today.DayOfWeek switch
                {
                    DayOfWeek.Sunday => Tab.Sunday,
                    DayOfWeek.Monday => Tab.Monday,
                    DayOfWeek.Tuesday => Tab.Tuesday,
                    DayOfWeek.Wednesday => Tab.Wednesday,
                    DayOfWeek.Thursday => Tab.Thursday,
                    DayOfWeek.Friday => Tab.Friday,
                    DayOfWeek.Saturday => Tab.Saturday,
                    _ => throw new ArgumentOutOfRangeException("DateTime.Today.DayOfWeek", DateTime.Today.DayOfWeek, "A tab is not defined for this day of the week")
                },
                Tab.Sunday or Tab.Monday or Tab.Tuesday or Tab.Wednesday or Tab.Thursday or Tab.Friday or Tab.Saturday => DateTime.Today.Day + Tab.Saturday,
                _ => throw new ArgumentOutOfRangeException(nameof(lastTab), lastTab.Value, "No review path is defined for this tab")
            };
        }
    }
}
