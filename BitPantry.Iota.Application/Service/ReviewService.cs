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

namespace BitPantry.Iota.Application.Service
{
    public class ReviewService
    {
        private DbConnectionFactory _db;
        private ILogger<ReviewService> _logger;

        public ReviewService(ILogger<ReviewService> logger, DbConnectionFactory db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<Tuple<ReviewSession, bool>> GetReviewSession(EntityDataContext dbCtx, long userId, bool startNewSession = false)
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
                    currentSession.SetReviewPath(await BuildReviewPath(userId));
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

        private async Task<Dictionary<Divider, int>> BuildReviewPath(long userId)
        {
            var path = new Dictionary<Divider, int>();
            var currentDivider = Divider.Queue;

            do
            {
                currentDivider = GetNextReviewDivider(currentDivider);
                path.Add(currentDivider, 0);

            } while (currentDivider < Divider.Day1);

            // select the card count grouped by divider for the userId

            var dbConnection = _db.GetDbConnection();
            if (dbConnection.State == System.Data.ConnectionState.Closed)
                dbConnection.Open();

            var cardCounts = await dbConnection.QueryAsync(
                @"SELECT Divider, COUNT(*) AS CardCount
                    FROM Cards
                    WHERE UserId = @UserId
                    GROUP BY Divider",
                new { UserId = userId });

            foreach (var count in cardCounts)
                path[(Divider)count.Divider] = count.CardCount;          

            return path;
        }

        public async Task<CardHeader> GetNextCardForReview(EntityDataContext dbCtx, long userId, Divider? currentDivider, int currentCardIndex)
        {
            // initialize the last divider to the queue if it is null

            currentDivider ??= Divider.Queue;
            Divider nextDivider = currentDivider.Value;

            // load review session

            var session = await GetReviewSession(dbCtx, userId);

            return await GetNextCardForReview_RECURSIVE(dbCtx, userId, session.Item1, currentDivider.Value, currentCardIndex);
        }

        private async Task<CardHeader> GetNextCardForReview_RECURSIVE(EntityDataContext dbCtx, long userId, ReviewSession session, Divider lastDivider, int lastCardOrder)
        {
            Divider nextDivider = lastDivider;

            // if not a multi-card review divider, get the next review divider

            if (lastDivider <= Divider.Day1)
                nextDivider = GetNextReviewDivider(lastDivider);

            // if advancing divider, reset order, otherwise increment order for same divider

            var nextCardOrder = nextDivider != lastDivider ? 1 : lastCardOrder + 1;

            // see if the card id exists and is not in the ignore list for the session

            var nextCard = await dbCtx.Cards.AsNoTracking()
                .Where(c => c.UserId == userId && c.Divider == nextDivider && c.Order == nextCardOrder)
                .SingleOrDefaultAsync();

            if (nextCard != null)
            {
                if (!session.GetCardsToIgnoreList().Contains(nextCard.Id))
                    return new CardHeader(nextCard.Id, nextCard.AddedOn, nextCard.LastMovedOn, nextCard.LastReviewedOn, nextCard.Divider, nextCard.Order);
                else
                    _logger.LogDebug("Card id {Id} found in review session ignore list and will be skipped", nextCard.Id);
            }

            // if no card found, recursively call this function to increment to the next divider, or return null if at the end of the review path

            if (nextDivider < Divider.Day1)
                return await GetNextCardForReview(dbCtx, userId, nextDivider, lastCardOrder);
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
