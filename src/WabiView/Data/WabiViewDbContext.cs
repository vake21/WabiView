using Microsoft.EntityFrameworkCore;
using WabiView.Models;

namespace WabiView.Data;

public class WabiViewDbContext : DbContext
{
    public WabiViewDbContext(DbContextOptions<WabiViewDbContext> options) : base(options)
    {
    }

    public DbSet<Coordinator> Coordinators => Set<Coordinator>();
    public DbSet<Round> Rounds => Set<Round>();
    public DbSet<CoinjoinTransaction> CoinjoinTransactions => Set<CoinjoinTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Coordinator>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Url).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Url).HasMaxLength(500);
        });

        modelBuilder.Entity<Round>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.RoundId);
            entity.HasIndex(e => new { e.CoordinatorId, e.RoundId }).IsUnique();
            entity.Property(e => e.RoundId).HasMaxLength(100);
            entity.Property(e => e.TxId).HasMaxLength(64);
            entity.Property(e => e.FailureReason).HasMaxLength(500);

            entity.HasOne(e => e.Coordinator)
                .WithMany(c => c.Rounds)
                .HasForeignKey(e => e.CoordinatorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CoinjoinTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TxId).IsUnique();
            entity.HasIndex(e => e.BlockHeight);
            entity.Property(e => e.TxId).HasMaxLength(64);
            entity.Property(e => e.BlockHash).HasMaxLength(64);
            entity.Property(e => e.RoundId).HasMaxLength(100);

            entity.HasOne(e => e.Coordinator)
                .WithMany(c => c.Coinjoins)
                .HasForeignKey(e => e.CoordinatorId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
