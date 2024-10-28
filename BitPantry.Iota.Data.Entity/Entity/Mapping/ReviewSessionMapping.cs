using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Data.Entity.Entity.Mapping
{
    internal class ReviewSessionMapping
    {
        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReviewSession>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.StartedOn)
                    .IsRequired();

                entity.Property(e => e.LastAccessed)
                    .IsRequired();

                entity.Property(e => e.CardIdsToIgnore)
                    .IsRequired(false);

            });

        }
    }
}
