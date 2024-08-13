using System;
using Microsoft.EntityFrameworkCore;

namespace BitPantry.Iota.Data.Entity.Entity.Mapping;

internal static class UserMapping
{
    public static void OnModelCreating(ModelBuilder modelBuilder)
    {
            modelBuilder.Entity<User>()
                .HasKey(m => m.Id);

            modelBuilder.Entity<User>()
                .Property(m => m.EmailAddress)
                .HasMaxLength(320)
                .IsRequired();
    }
}
