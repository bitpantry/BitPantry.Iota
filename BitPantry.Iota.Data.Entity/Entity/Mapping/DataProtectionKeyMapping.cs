using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Data.Entity.Mapping
{
    internal class DataProtectionKeyMapping
    {
        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DataProtectionKey>()
                .HasKey(m => m.Id);

            modelBuilder.Entity<DataProtectionKey>()
                .Property(m => m.Xml)
                .IsRequired();

            modelBuilder.Entity<DataProtectionKey>()
                .Property(m => m.CreatedOn)
                .IsRequired();
        }
    }
}
