using BitPantry.Tabs.Application.DTO;
using BitPantry.Tabs.Application.Logic;
using BitPantry.Tabs.Common;
using BitPantry.Tabs.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace BitPantry.Tabs.Application.Service
{
    public class TabsService
    {
        private EntityDataContext _dbCtx;

        public TabsService(EntityDataContext dbCtx)
        {
            _dbCtx = dbCtx;
        }

        public async Task<int> GetCardCountForTab(long userId, Tab tab, CancellationToken cancellationToken = default)
            => await _dbCtx.Cards.CountAsync(c => c.UserId == userId && c.Tab == tab, cancellationToken);

        public async Task<List<CardDto>> GetCardsForTab(long userId, Tab tab, CancellationToken cancellationToken)
        {
            var cards = await _dbCtx.Cards
                .AsNoTracking()
                .Where(c => c.UserId == userId && c.Tab == tab)
                .OrderBy(c => c.NumberedCard.RowNumber)
                .ToListAsync(cancellationToken);

            if (cards.Count == 1)
                return [.. (await Task.WhenAll(cards.Select(c => c.ToDtoLoadVerses(_dbCtx, cancellationToken))))];
            else
                return cards.Select(c => c.ToDto()).ToList();
        }

        public async Task<Dictionary<Tab, int>> GetCardCountByTab(long userId, CancellationToken cancellationToken)
            => await _dbCtx.Cards
                .AsNoTracking()
                .Where(c => c.UserId == userId)
                .GroupBy(card => card.Tab)
                .ToDictionaryAsync(group => group.Key, group => group.Count(), cancellationToken: cancellationToken);
    }
}
