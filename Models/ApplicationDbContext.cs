using Microsoft.EntityFrameworkCore;

namespace FileStoreService.Models
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<StoredFile> StoredFiles { get; set; }
        public DbSet<Owner> Owners { get; set; }
        public DbSet<FileOwner> FileOwners { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<StoredFile>()
                .HasIndex(f => f.Hash)
                .IsUnique();

            builder.Entity<FileOwner>()
                .HasKey(fo => new { fo.StoredFileId, fo.OwnerId });

            builder.Entity<FileOwner>()
                .HasOne(fo => fo.StoredFile)
                .WithMany(f => f.Owners)
                .HasForeignKey(fo => fo.StoredFileId);

            builder.Entity<FileOwner>()
                .HasOne(fo => fo.Owner)
                .WithMany(o => o.Files)
                .HasForeignKey(fo => fo.OwnerId);
        }
    }
}
