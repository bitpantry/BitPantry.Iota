using BitPantry.Iota.Application.DTO;
using BitPantry.Iota.Application.Logic;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace BitPantry.Iota.Application.Service
{
    public class TabsService
    {
        private EntityDataContext _dbCtx;
        private CardLogic _cardLogc;

        public TabsService(EntityDataContext dbCtx, CardLogic cardLogic)
        {
            _dbCtx = dbCtx;
            _cardLogc = cardLogic;
        }

        public async Task<List<CardDto>> GetCardsForTab(long userId, Tab tab, CancellationToken cancellationToken)
        {
            var cards = await _dbCtx.Cards
                .AsNoTracking()
                .Where(c => c.UserId == userId && c.Tab == tab)
                .OrderBy(c => c.Order)
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
