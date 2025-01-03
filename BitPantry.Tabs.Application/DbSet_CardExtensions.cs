using BitPantry.Tabs.Common;
using BitPantry.Tabs.Data.Entity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using Thinktecture;

namespace BitPantry.Tabs.Application
{
    internal static class DbSet_CardExtensions
    {
        public static async Task<double> GetNewLastFractionalOrder(this DbSet<Card> dbSet, long userId, Tab tab, CancellationToken cancellationToken = default)
        {
            var maxFractionalOrder = await dbSet
                .AsNoTracking()
                .Where(c => c.UserId == userId && c.Tab == tab)
                .Select(c => (long?)c.FractionalOrder)
                .MaxAsync(cancellationToken);

            return (maxFractionalOrder ?? 0.0) + 1.0;
        }

        public static async Task<double> GetNewFirstFractionalOrder(this DbSet<Card> dbSet, long userId, Tab tab, CancellationToken cancellationToken = default)
        {
            var minFractionalOrder = await dbSet
                .AsNoTracking()
                .Where(c => c.UserId == userId && c.Tab == tab)
                .Select(c => (long?)c.FractionalOrder)
                .MinAsync(cancellationToken);

            return (minFractionalOrder ?? 0.0) - 1.0;
        }

        public static async Task<bool> DoesCardAlreadyExistForPassage(this DbSet<Card> dbSet, long userId, string parsedAddress, CancellationToken cancellationToken)
            => await dbSet.AnyAsync(c => c.Address.Equals(parsedAddress) && c.UserId == userId, cancellationToken: cancellationToken);



    }
}
