using BitPantry.Iota.Application.Archive;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.Service
{
    public class ArchiveService
    {
        private readonly EntityDataContext _dbCtx;
        private readonly BibleService _bibleSvc;
        private readonly CardService _cardSvc;
        private readonly ILogger<ArchiveService> _logger;

        public ArchiveService(ILogger<ArchiveService> logger, EntityDataContext dbCtx, BibleService bibleSvc, CardService cardSvc) 
        {
            _dbCtx = dbCtx;
            _bibleSvc = bibleSvc;
            _cardSvc = cardSvc;
            _logger = logger;
        }

        public async Task ArchiveUserData(string outputFilePath, long? userId = null, CancellationToken cancellationToken = default)
        {
            using (StreamWriter fileStream = new(outputFilePath, false, new UTF8Encoding(false)))
            using (var jsonWriter = new Utf8JsonWriter(fileStream.BaseStream, new JsonWriterOptions { Indented = true }))
            {
                var masterBibleIdList = new HashSet<long>();
                List<long> userIds = userId.HasValue ? new List<long>([userId.Value]) : await _dbCtx.Users.AsNoTracking().Select(u => u.Id).ToListAsync(cancellationToken);

                jsonWriter.WriteStartObject();

                jsonWriter.WriteStartArray("data");

                foreach (var id in userIds)
                {
                    jsonWriter.WriteStartObject();
                    var bibleIds = await WriteUser(id, jsonWriter, cancellationToken);
                    jsonWriter.WriteEndObject();

                    masterBibleIdList.UnionWith(bibleIds);
                }

                jsonWriter.WriteEndArray();
             

                await WriteBibleRefs(masterBibleIdList, jsonWriter, cancellationToken);

                jsonWriter.WriteEndObject();
            }
        }


        private async Task<List<long>> WriteUser(long userId, Utf8JsonWriter jsonWriter, CancellationToken cancellationToken)
        {
            await WriteHeader(userId, jsonWriter, cancellationToken);
            return await WriteCards(userId, jsonWriter, cancellationToken);
        }

        private async Task WriteHeader(long userId, Utf8JsonWriter jsonWriter, CancellationToken cancellationToken)
        {
            var user = await _dbCtx.Users
                .AsNoTracking()
                .Select(u => new 
                { 
                    u.Id, 
                    u.EmailAddress, 
                    u.LastLogin 
                })
                .SingleAsync(u => u.Id == userId, cancellationToken);

            jsonWriter.SerializeObject(user, "user");
        }

        private async Task<List<long>> WriteCards(long userId, Utf8JsonWriter jsonWriter, CancellationToken cancellationToken)
        {
            // begin card query

            var cardQuery = _dbCtx.Cards
                .AsNoTracking()
                .Where(c => c.UserId == userId)
                .Select(c => new
                {
                    c.BibleId,
                    c.Address,
                    c.StartVerseId,
                    c.EndVerseId,
                    c.AddedOn,
                    c.LastMovedOn,
                    c.LastReviewedOn,
                    c.Tab,
                    c.Order

                })
                .AsAsyncEnumerable();

            // write cards 

            var bibleIds = new List<long>();

            try
            {
                jsonWriter.WriteStartArray("cards");

                await foreach (var card in cardQuery.WithCancellation(cancellationToken))
                {
                    if (!bibleIds.Contains(card.BibleId))
                        bibleIds.Add(card.BibleId);

                    jsonWriter.SerializeObject(card);
                }

                jsonWriter.WriteEndArray();
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Operation canceled - the export file is incomplete");
            }

            // write bible reference dictionary

            return bibleIds;

        }
        private async Task WriteBibleRefs(HashSet<long> bibleIds, Utf8JsonWriter jsonWriter, CancellationToken cancellationToken)
        {
            var bibles = await _bibleSvc.GetBibleTranslations(cancellationToken);
            var bibleRefs = bibles.Where(b => bibleIds.Contains(b.Id)).ToDictionary(b => b.Id, b => b.ShortName);

            jsonWriter.SerializeObject(bibleRefs, "biblerefs");
        }

        public async Task ImportUserData(string inputFilePath, bool recreateCards = false, CancellationToken cancellationToken = default)
        {
            // load archive into memory

            DataArchiveModel model = null;

            using (var stream = File.OpenRead(inputFilePath)) {
                model = await System.Text.Json.JsonSerializer.DeserializeAsync<DataArchiveModel>(stream, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new NullableDateTimeConverter() }
                }, cancellationToken);
            }

            // validate bible references and create map

            var bibles = await _bibleSvc.GetBibleTranslations(cancellationToken);

            var bibleRefDict = new Dictionary<long, long>();

            foreach (var bref in model.BibleReferences)
            {
                var bible = bibles.Single(b => b.ShortName.Equals(bref.Value));
                if (bible == null)
                    throw new Exception($"Archive contains a bible reference '{bref.Value}' which doesn't exist in the database.");

                bibleRefDict.Add(bref.Key, bible.Id);
            }

            // process users

            foreach (var userData in model.Users)
            {
                // validate unique email address

                if(_dbCtx.Users.Any(u => u.EmailAddress.ToUpper().Equals(userData.User.EmailAddress.ToUpper())))
                    throw new Exception($"A duplicate user email address already exists :: {userData.User.EmailAddress}");

                var userEnt = userData.User.ToEntity();

                _dbCtx.Users.Add(userEnt);
                await _dbCtx.SaveChangesAsync(cancellationToken);

                _logger.LogDebug("Imported archive user {ArchiveUserId} as new user entity {EntityUserId}", userData.User.Id, userEnt.Id);

                Dictionary<Tab, List<int>> tabOrders = new Dictionary<Tab, List<int>>();

                foreach (var card in userData.Cards)
                {
                    if(!tabOrders.ContainsKey(card.Tab))
                        tabOrders.Add(card.Tab, new List<int>());

                    if (tabOrders[card.Tab].Contains(card.Order))
                        throw new Exception($"Order already taken for userId {userData.User.Id} and card {card.Address} with order {card.Order}");

                    tabOrders[card.Tab].Add(card.Order);

                    if (recreateCards)
                        await _cardSvc.CreateCard(userEnt.Id, bibleRefDict[card.BibleId], card.Address, card.Tab, card.Order, cancellationToken);
                    else
                        _dbCtx.Cards.Add(card.ToEntity(userEnt.Id));
                }

                await _dbCtx.SaveChangesAsync(cancellationToken);

                _logger.LogDebug("Cards imported for user {EntityUserId}", userEnt.Id);
            }
        }

    }


}
