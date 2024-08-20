using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Data.Entity.Mapping
{
    internal class TestamentMapping
    {
        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Testament>()
                .HasKey(m => m.Id);

            modelBuilder.Entity<Testament>()
                .Property(m => m.Name)
                .IsRequired();

            modelBuilder.Entity<Testament>()
                .HasMany(t => t.Books)
                .WithOne(b => b.Testament)
                .HasForeignKey(b => b.TestamentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
