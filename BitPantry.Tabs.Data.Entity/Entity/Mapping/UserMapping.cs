using System;
using BitPantry.Tabs.Common;
using Microsoft.EntityFrameworkCore;

namespace BitPantry.Tabs.Data.Entity.Mapping;

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

        modelBuilder.Entity<User>()
            .HasMany(u => u.Cards)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasIndex(m => m.EmailAddress)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.WorkflowType);

        modelBuilder.Entity<User>()
            .Property(u => u.CliApiKey);
    }
}
