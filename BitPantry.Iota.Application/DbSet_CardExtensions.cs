using BitPantry.Iota.Common;
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
        public static async Task<int> GetNextAvailableOrder(this DbSet<Card> dbSet, long userId, Tab tab)
        {
            var maxOrder = await dbSet
                .Where(c => c.UserId == userId && c.Tab == tab)
                .Select(c => (int?)c.Order) 
                .MaxAsync();

            return (maxOrder ?? 0) + 1; 
        }
    }
}
