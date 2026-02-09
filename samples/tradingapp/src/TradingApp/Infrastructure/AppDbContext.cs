using Microsoft.EntityFrameworkCore;
using TradingApp.Domain;

namespace TradingApp.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<WatchlistItem> WatchlistItems => Set<WatchlistItem>();
    public DbSet<AlpacaSettings> AlpacaSettings => Set<AlpacaSettings>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WatchlistItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Symbol).IsRequired().HasMaxLength(32);
            e.HasIndex(x => x.Symbol).IsUnique();
        });

        modelBuilder.Entity<AlpacaSettings>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ApiKey).HasMaxLength(256);
            e.Property(x => x.ApiSecret).HasMaxLength(2048);
            e.Property(x => x.BaseUrl).HasMaxLength(512);
        });

        modelBuilder.Entity<Alert>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Symbol).IsRequired().HasMaxLength(32);
            e.Property(x => x.Payload).HasMaxLength(2000);
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Symbol).IsRequired().HasMaxLength(32);
            e.Property(x => x.Message).HasMaxLength(500);
        });
    }
}
