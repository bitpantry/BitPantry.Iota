using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Tabs.Data.Entity.Mapping
{
    internal class ChapterMapping
    {
        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Chapter>()
                .HasKey(m => m.Id);

            modelBuilder.Entity<Chapter>()
                .Property(m => m.Number)
                .IsRequired();

            modelBuilder.Entity<Chapter>()
                .HasMany(c => c.Verses)
                .WithOne(v => v.Chapter)
                .HasForeignKey(v => v.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
