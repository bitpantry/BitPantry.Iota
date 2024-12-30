using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Data.Entity.Mapping
{
    public class NumberedCardMapping
    {
        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NumberedCard>()
                .ToView("NumberedCardView")  
                .HasKey(c => new { c.CardId, c.RowNumber });

            modelBuilder.Entity<NumberedCard>()
                .Property(c => c.RowNumber)
                .IsRequired();

            modelBuilder.Entity<NumberedCard>()
                .HasOne(c => c.Card)
                .WithOne(c => c.NumberedCard)
                .HasForeignKey<NumberedCard>(c => c.CardId);                
        }
    }
}
