using System.Text;
using BitPantry.Iota.Application.DTO;
using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using Microsoft.Extensions.DependencyInjection;

namespace BitPantry.Iota.Test
{
    public static class IHaveServiceProviderExtensions
    {
        private static object _lock = new object();
        public static int _testUserIndex = 0;

        public static async Task<long> CreateUser(this IHaveServiceProvider env, WorkflowType workflowType = WorkflowType.Basic, string emailAddress = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(emailAddress))
            {
                lock (_lock)
                {
                    _testUserIndex++;
                    emailAddress = $"testUser{_testUserIndex}@test.com";
                }
            }

            using (var scope = env.ServiceProvider.CreateScope())
            {
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

                var user = new User { EmailAddress = emailAddress, WorkflowType = workflowType };
                dbCtx.Users.Add(user);
                await dbCtx.SaveChangesAsync(cancellationToken);

                return user.Id;
            }

        }

        public static async Task<long> InstallBibles(this IHaveServiceProvider env, CancellationToken cancellationToken = default)
        {
            using (var scope = env.ServiceProvider.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<BibleService>();
                var id = await svc.Install(new MemoryStream(Resources.ESV), cancellationToken);
                _ = await svc.Install(new MemoryStream(Resources.MSG), cancellationToken);
                return id;
            }
        }

        public static async Task<List<CardDto>> CreateCards(this IHaveServiceProvider env, long userId, long bibleId, CancellationToken cancellationToken = default)
        {
            using (var scope = env.ServiceProvider.CreateScope())
            {
                var cardDtos = new List<CardDto>();

                var svc = scope.ServiceProvider.GetRequiredService<CardService>();

                cardDtos.Add((await svc.CreateCard(userId, bibleId, "matt 7:3-5", Tab.Daily, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "matt 7:24-26", Tab.Odd, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "mark 10:42-45", Tab.Even, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "1 tim 4:7", Tab.Sunday, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "2 pet 3:9", Tab.Monday, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 4:19", Tab.Tuesday, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 8:10-11", Tab.Wednesday, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 9:6", Tab.Thursday, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 9:7", Tab.Friday, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 10:19", Tab.Saturday, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 11:24", Tab.Day1, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 13:20", Tab.Day2, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 14:16", Tab.Day3, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 15:12", Tab.Day4, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 15:21", Tab.Day5, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 15:22", Tab.Day6, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 16:3", Tab.Day7, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 16:25", Tab.Day8, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 16:32", Tab.Day9, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 17:9", Tab.Day10, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 18:2", Tab.Day11, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 18:13", Tab.Day12, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 18:17", Tab.Day13, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 19:11", Tab.Day14, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 22:4", Tab.Day15, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 22:17-18", Tab.Day16, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 24:11", Tab.Day17, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 24:13-14", Tab.Day18, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 25:28", Tab.Day19, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 26:2", Tab.Day20, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 26:18-19", Tab.Day21, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 27:20", Tab.Day22, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 30:5", Tab.Day23, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 30:7-9", Tab.Day24, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 30:12", Tab.Day25, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "prov 31:4-5", Tab.Day26, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "ps 13:1-2", Tab.Day27, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "ps 19:14", Tab.Day28, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "ps 101:3", Tab.Day29, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "ps 119:9", Tab.Day30, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "ps 119:11", Tab.Day31, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "ps 119:32", Tab.Queue, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "ps 119:89-90", Tab.Queue, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "ps 119:105", Tab.Queue, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "ps 127:1-2", Tab.Queue, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "ps 135:6", Tab.Queue, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "ps 141:4", Tab.Queue, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "gen 2:1", Tab.Queue, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "gen 2:2", Tab.Queue, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "gen 2:3", Tab.Queue, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "gen 2:4", Tab.Queue, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "gen 2:5", Tab.Queue, cancellationToken)).Card);
                cardDtos.Add((await svc.CreateCard(userId, bibleId, "gen 2:6", Tab.Queue, cancellationToken)).Card);


                return cardDtos;
            }
        }
    }
}
