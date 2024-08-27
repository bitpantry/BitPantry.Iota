using BitPantry.Iota.Data.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application
{
    internal static class DbSet_CardExtensions
    {
        public static async Task<int> GetNextAvailableOrder(this DbSet<Card> dbSet, long userId, Divider divider)
        {
            var maxOrder = await dbSet
                .Where(c => c.UserId == userId && c.Divider == divider)
                .Select(c => (int?)c.Order) 
                .MaxAsync();

            return (maxOrder ?? 0) + 1; 
        }
    }
}
