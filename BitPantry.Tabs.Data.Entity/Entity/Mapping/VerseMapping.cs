using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Tabs.Data.Entity.Mapping
{
    internal class VerseMapping
    {
        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Verse>()
                .HasKey(m => m.Id);

            modelBuilder.Entity<Verse>()
                .Property(m => m.Number)
                .IsRequired();
        }
    }
}
