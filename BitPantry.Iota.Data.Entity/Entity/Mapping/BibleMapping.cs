using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Data.Entity.Mapping
{
    internal static class BibleMapping
    {
        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bible>()
                .HasKey(m => m.Id);

            modelBuilder.Entity<Bible>()
                .Property(m => m.TranslationShortName)
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<Bible>()
                .Property(m => m.TranslationLongName)
                .HasMaxLength(150)
                .IsRequired();

            modelBuilder.Entity<Bible>()
                .Property(m => m.Classification)
                .IsRequired();

            modelBuilder.Entity<Bible>()
                .Property(m => m.Description)
                .IsRequired(false);

            modelBuilder.Entity<Bible>()
                .HasMany(b => b.Testaments)
                .WithOne(t => t.Bible)
                .HasForeignKey(t => t.BibleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
