using BitPantry.Iota.Data.Entity.Mapping;
using Microsoft.EntityFrameworkCore;

namespace BitPantry.Iota.Data.Entity
{
    public class EntityDataContext : DbContext
    {
        public EntityDataContext(DbContextOptions<EntityDataContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            UserMapping.OnModelCreating(modelBuilder);

            DataProtectionKeyMapping.OnModelCreating(modelBuilder);

            BibleMapping.OnModelCreating(modelBuilder);
            TestamentMapping.OnModelCreating(modelBuilder);
            BookMapping.OnModelCreating(modelBuilder);
            ChapterMapping.OnModelCreating(modelBuilder);
            VerseMapping.OnModelCreating(modelBuilder);

            CardMapping.OnModelCreating(modelBuilder);
            NumberedCardMapping.OnModelCreating(modelBuilder);

        }

        public DbSet<User> Users { get; set; }
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        public DbSet<Bible> Bibles { get; set; }
        public DbSet<Testament> Testaments { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Verse> Verses { get; set; }

        public DbSet<Card> Cards { get; set; }
        public DbSet<NumberedCard> NumberedCards { get; set; }

    }

}
