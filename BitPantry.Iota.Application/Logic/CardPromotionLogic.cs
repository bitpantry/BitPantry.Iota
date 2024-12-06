using BitPantry.Iota.Common;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.Logic
{
    public class CardPromotionLogic
    {
        public async Task<Tab> GetPromotionTabQuery(DbConnection dbConnection, long userId, Tab currentTab, WorkflowType workflowType)
        {
            switch (currentTab)
            {
                case Tab.Queue:

                    return Tab.Daily;

                case Tab.Daily:

                    return workflowType switch
                    {
                        WorkflowType.Basic => await GetSingleCardTabOfOldestCardQuery(dbConnection, userId, Tab.Odd, Tab.Even),
                        WorkflowType.Advanced => await GetMultipleCardTabOfLeastAndOldestCardsQuery(dbConnection, userId, Tab.Odd, Tab.Even),
                        _ => throw new ArgumentOutOfRangeException(nameof(workflowType), $"No case defined for {workflowType}"),
                    };

                case Tab.Odd:
                case Tab.Even:

                    return workflowType switch
                    {
                        WorkflowType.Basic => await GetSingleCardTabOfOldestCardQuery(dbConnection, userId, Tab.Sunday, Tab.Saturday),
                        WorkflowType.Advanced => await GetMultipleCardTabOfLeastAndOldestCardsQuery(dbConnection, userId, Tab.Sunday, Tab.Saturday),
                        _ => throw new ArgumentOutOfRangeException(nameof(workflowType), $"No case defined for {workflowType}"),
                    };

                case Tab.Sunday:
                case Tab.Monday:
                case Tab.Tuesday:
                case Tab.Wednesday:
                case Tab.Thursday:
                case Tab.Friday:
                case Tab.Saturday:

                    return await GetMultipleCardTabOfLeastAndOldestCardsQuery(dbConnection, userId, Tab.Day1, Tab.Day31);

                default:
                    throw new ArgumentOutOfRangeException(nameof(currentTab), currentTab, "No promotion path is defined for this tab");
            }
        }

        private async Task<Tab> GetSingleCardTabOfOldestCardQuery(DbConnection dbConnection, long userId, Tab start, Tab end)
        {

            // return the first empty tab if any

            var usedTabs = await dbConnection.QueryAsync<Tab>(
                @"SELECT DISTINCT Tab
                    FROM Cards
                    WHERE UserId = @UserId
                    AND Tab >= @Start
                    AND Tab <= @End",
                new
                {
                    UserId = userId,
                    Start = start,
                    End = end
                });

            var unusedTabs = Enumerable.Range((int)start, (int)end - (int)start + 1).Select(i => (Tab)i)
                .Except(usedTabs)
                .Order()
                .ToList();

            if (unusedTabs.Count != 0)
                return unusedTabs.First();

            // return the tab with the oldest moved date

            var tab = await dbConnection.QuerySingleAsync<Tab>(
                @"SELECT TOP 1 Tab
                    FROM Cards
                    WHERE UserId = @UserId
                    AND Tab >= @Start
                    AND Tab <= @End
                    ORDER BY LastMovedOn ASC",
                new
                {
                    UserId = userId,
                    Start = start,
                    End = end
                });


            return tab;

        }

        private async Task<Tab> GetMultipleCardTabOfLeastAndOldestCardsQuery(DbConnection dbConnection, long userId, Tab start, Tab end)
        {
            var tabCounts = await dbConnection.QueryAsync<(Tab Tab, int CardCount)>(
                @"SELECT Tab, COUNT(*) AS CardCount
                    FROM Cards
                    WHERE UserId = @UserId
                    AND Tab >= @Start
                    AND Tab <= @End
                    GROUP BY Tab
                    ORDER BY CardCount ASC, MIN(LastMovedOn) ASC",
                new
                {
                    UserId = userId,
                    Start = start,
                    End = end
                });

            var emptyTabs = Enumerable.Range((int)start, (int)end - (int)start + 1).Select(i => (Tab)i)
                .Except(tabCounts
                    .Select(dc => dc.Tab))
                    .ToList();

            return emptyTabs.Any() ? emptyTabs.First() : tabCounts.First().Tab;

        }

    }
}
