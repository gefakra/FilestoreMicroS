using Microsoft.EntityFrameworkCore;
using FilestoreMicroS.Models;

namespace FilestoreMicroS.Data
{
    public class FileStoreContext : DbContext
    {
        public FileStoreContext(DbContextOptions<FileStoreContext> opts) : base(opts) { }
        public DbSet<FileEntity> Files { get; set; } = null!;
        public DbSet<Owner> Owners { get; set; } = null!;
        public DbSet<FileOwner> FileOwners { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FileEntity>().HasKey(f => f.Hash);
            modelBuilder.Entity<FileEntity>().HasIndex(f => f.Hash).IsUnique();
            modelBuilder.Entity<FileOwner>()
            .HasOne(fo => fo.File)
            .WithMany(f => f.Owners)
            .HasForeignKey(fo => fo.FileHash)
            .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<FileOwner>()
            .HasOne(fo => fo.Owner)
            .WithMany(o => o.FileOwners)
            .HasForeignKey(fo => fo.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
            base.OnModelCreating(modelBuilder);
        }
    }
}
