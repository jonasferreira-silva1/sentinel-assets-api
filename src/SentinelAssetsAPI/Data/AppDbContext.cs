using Microsoft.EntityFrameworkCore;
using SentinelAssetsAPI.Models;

namespace SentinelAssetsAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Asset> Assets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asset>()
            .HasIndex(a => a.IPAddress)
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}

