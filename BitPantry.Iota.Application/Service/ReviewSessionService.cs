using Azure.Core;
using BitPantry.Iota.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.Service
{
    public class ReviewSessionService
    {
        private ILogger<ReviewSessionService> _logger;

        public ReviewSessionService(ILogger<ReviewSessionService> logger) 
        {
            _logger = logger;
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
                }
                else
                {
                    currentSession.LastAccessed = DateTime.UtcNow;
                    isNew = false;
                }

            }
            else
            {
                currentSession = new Data.Entity.ReviewSession(userId);
                dbCtx.ReviewSessions.Add(currentSession);
            }

            return new Tuple<ReviewSession, bool>(currentSession, isNew);
        }
    }
}
