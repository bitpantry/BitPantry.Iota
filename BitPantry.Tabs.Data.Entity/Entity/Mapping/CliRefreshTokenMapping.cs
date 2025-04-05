using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Tabs.Data.Entity.Mapping
{
    internal class CliRefreshTokenMapping
    {
        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CliRefreshToken>()
                .HasKey(m => m.Id);

            modelBuilder.Entity<CliRefreshToken>()
                .Property(m => m.ClientId)
                .IsRequired();

            modelBuilder.Entity<CliRefreshToken>()
                .Property(m => m.RefreshToken)
                .IsRequired();
        }
    }
}
