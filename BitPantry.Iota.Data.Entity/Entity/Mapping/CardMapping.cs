using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Data.Entity.Mapping
{
    internal class CardMapping
    {
        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Card>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Card>()
                .Property(c => c.AddedOn)
                .IsRequired();

            modelBuilder.Entity<Card>()
                .Property(c => c.LastMovedOn)
                .IsRequired();

            modelBuilder.Entity<Card>()
                .HasMany(c => c.Verses)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "CardVerse",
                    j => j.HasOne<Verse>()
                          .WithMany()
                          .HasForeignKey("VerseId")
                          .OnDelete(DeleteBehavior.Cascade),  // Cascade delete on Verse
                    j => j.HasOne<Card>()
                          .WithMany()
                          .HasForeignKey("CardId")
                          .OnDelete(DeleteBehavior.Cascade),  // Cascade delete on Card
                    j =>
                    {
                        j.HasKey("CardId", "VerseId");
                    });

            modelBuilder.Entity<Card>()
                .Property(c => c.Divider)
                .IsRequired();

            modelBuilder.Entity<Card>()
                .Property(c => c.Order)
                .IsRequired();
        }
    }
}
