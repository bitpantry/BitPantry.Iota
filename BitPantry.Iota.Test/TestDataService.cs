using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Test
{
    public class TestDataService
    {
        private ILogger<TestDataService> _logger;
        private EntityDataContext _dbCtx;
        private BibleService _bibleSvc;
        private CardService _cardSvc;

        public TestDataService(ILogger<TestDataService> logger, EntityDataContext dbCtx, BibleService bibleSvc, CardService cardSvc)
        {
            _logger = logger;
            _dbCtx = dbCtx;
            _bibleSvc = bibleSvc;
            _cardSvc = cardSvc;
        }

        public async Task Install(bool installTestData = false, CancellationToken cancellationToken = default)
        {
            // apply migrations

            await _dbCtx.Database.MigrateAsync(cancellationToken);

            if (installTestData)
            {

                // install bibles

                _logger.LogDebug("Installing bibles");

                var bibleId = await _bibleSvc.Install(new MemoryStream(Encoding.UTF8.GetBytes(Resource.Bible_ESV)), cancellationToken);
                await _bibleSvc.Install(new MemoryStream(Encoding.UTF8.GetBytes(Resource.Bible_MSG)), cancellationToken);

                // add user

                _logger.LogDebug("Creating users");

                var user = new User
                {
                    EmailAddress = "testuser@test.com"
                };

                _dbCtx.Users.Add(user);
                await _dbCtx.SaveChangesAsync(cancellationToken);

                // add cards

                _logger.LogDebug("Creating cards");

                await _cardSvc.CreateCard(user.Id, bibleId, "matt 7:3-5", Tab.Daily, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "matt 7:24-26", Tab.Odd, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "mark 10:42-45", Tab.Even, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "1 tim 4:7", Tab.Sunday, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "2 pet 3:9", Tab.Monday, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 4:19", Tab.Tuesday, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 8:10-11", Tab.Wednesday, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 9:6", Tab.Thursday, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 9:7", Tab.Friday, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 10:19", Tab.Saturday, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 11:24", Tab.Day1, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 13:20", Tab.Day2, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 14:16", Tab.Day3, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 15:12", Tab.Day4, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 15:21", Tab.Day5, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 15:22", Tab.Day6, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 16:3", Tab.Day7, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 16:25", Tab.Day8, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 16:32", Tab.Day9, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 17:9", Tab.Day10, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 18:2", Tab.Day11, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 18:13", Tab.Day12, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 18:17", Tab.Day13, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 19:11", Tab.Day14, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 22:4", Tab.Day15, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 22:17-18", Tab.Day16, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 24:11", Tab.Day17, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 24:13-14", Tab.Day18, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 25:28", Tab.Day19, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 26:2", Tab.Day20, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 26:18-19", Tab.Day21, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 27:20", Tab.Day22, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 30:5", Tab.Day23, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 30:7-9", Tab.Day24, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 30:12", Tab.Day25, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "prov 31:4-5", Tab.Day26, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "ps 13:1-2", Tab.Day27, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "ps 19:14", Tab.Day28, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "ps 101:3", Tab.Day29, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "ps 119:9", Tab.Day30, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "ps 119:11", Tab.Day31, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "ps 119:32", Tab.Queue, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "ps 119:89-90", Tab.Queue, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "ps 119:105", Tab.Queue, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "ps 127:1-2", Tab.Queue, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "ps 135:6", Tab.Queue, cancellationToken);
                await _cardSvc.CreateCard(user.Id, bibleId, "ps 141:4", Tab.Queue, cancellationToken);

            }

        }

    }
}
