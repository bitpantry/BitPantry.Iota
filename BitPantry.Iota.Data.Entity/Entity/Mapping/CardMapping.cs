using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata;

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
                .Property(c => c.LastReviewedOn)
                .IsRequired(false);

            modelBuilder.Entity<Card>()
                .Property(c => c.Address)
                .IsRequired();

            modelBuilder.Entity<Card>()
                .HasOne<Bible>()
                .WithMany() 
                .HasForeignKey(card => card.BibleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Card>()
                .HasOne<Verse>()
                .WithMany()
                .HasForeignKey(card => card.StartVerseId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Card>()
                .HasOne<Verse>()
                .WithMany()
                .HasForeignKey(card => card.EndVerseId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Card>()
                .Property(c => c.Tab)
                .IsRequired();

            modelBuilder.Entity<Card>()
                .Property(c => c.Order)
                .IsRequired();



        }
    }
}
