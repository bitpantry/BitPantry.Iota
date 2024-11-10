//using BitPantry.Iota.Application.DTO;
//using BitPantry.Iota.Common;
//using BitPantry.Iota.Data.Entity;
//using Dapper;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Data.Common;
//using System.Linq;
//using System.Net.WebSockets;
//using System.Text;
//using System.Threading.Tasks;

//namespace BitPantry.Iota.Application.Logic
//{
//    public class ReviewLogic
//    {
//        private ILogger<ReviewLogic> _logger;

//        public ReviewLogic(ILogger<ReviewLogic> logger)
//        {
//            _logger = logger;
//        }



//        public async Task<CardDto> GetNextCardForReviewQuery(EntityDataContext dbCtx, long userId, Tab? currentTab, int currentCardIndex)
//        {
//            // initialize the last tab to the queue if it is null

//            currentTab ??= Tab.Queue;
//            Tab nextTab = currentTab.Value;

//            // load review session

//            return await GetNextCardForReviewQuery_RECURSIVE(dbCtx, userId, currentTab.Value, currentCardIndex);
//        }

//        private async Task<CardDto> GetNextCardForReviewQuery_RECURSIVE(EntityDataContext dbCtx, long userId, Tab lastTab, int lastCardOrder)
//        {
//            Tab nextTab = lastTab;

//            // if not a multi-card review tab, get the next review tab

//            if (lastTab < Tab.Day1)
//                nextTab = GetNextReviewTab(lastTab);

//            // if advancing tab, reset order, otherwise increment order for same tab

//            var nextCardOrder = nextTab != lastTab ? 1 : lastCardOrder + 1;

//            // return card if found

//            var nextCard = await dbCtx.Cards.AsNoTracking()
//                .Where(c => c.UserId == userId && c.Tab == nextTab && c.Order == nextCardOrder)
//                .SingleOrDefaultAsync();

//            if (nextCard != null)
//                return nextCard.ToDto();

//            // if no card found, recursively call this function to increment to the next tab, or return null if at the end of the review path

//            if (nextTab < Tab.Day1)
//                return await GetNextCardForReviewQuery(dbCtx, userId, nextTab, lastCardOrder);
//            else
//                return null;
//        }


//    }
//}
