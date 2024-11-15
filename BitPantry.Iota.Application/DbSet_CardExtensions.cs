using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace BitPantry.Iota.Application
{
    internal static class DbSet_CardExtensions
    {
        public static async Task<int> GetNextAvailableOrder(this DbSet<Card> dbSet, long userId, Tab tab, CancellationToken cancellationToken)
        {
            var maxOrder = await dbSet
                .Where(c => c.UserId == userId && c.Tab == tab)
                .Select(c => (int?)c.Order) 
                .MaxAsync(cancellationToken);

            return (maxOrder ?? 0) + 1; 
        }

        public static async Task<bool> DoesCardAlreadyExistForPassage(this DbSet<Card> dbSet, long userId, string parsedAddress, CancellationToken cancellationToken)
            => await dbSet.AnyAsync(c => c.Address.Equals(parsedAddress) && c.UserId == userId, cancellationToken: cancellationToken);
    }
}
