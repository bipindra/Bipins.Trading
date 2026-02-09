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
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

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
            e.Property(x => x.Threshold).HasColumnType("TEXT");
            e.Property(x => x.ComparisonType).HasConversion<int>();
            e.Property(x => x.Timeframe).HasMaxLength(16);
            e.Property(x => x.OrderQuantity).HasColumnType("TEXT");
            e.Property(x => x.OrderType).HasConversion<int>();
            e.Property(x => x.OrderSideOverride).HasConversion<int>();
            e.Property(x => x.OrderLimitPrice).HasColumnType("TEXT");
            e.Property(x => x.OrderStopPrice).HasColumnType("TEXT");
            e.Property(x => x.OrderTimeInForce).HasConversion<int>();
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Symbol).IsRequired().HasMaxLength(32);
            e.Property(x => x.Message).HasMaxLength(500);
        });

        modelBuilder.Entity<ActivityLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Level).IsRequired().HasMaxLength(20);
            e.Property(x => x.Category).IsRequired().HasMaxLength(50);
            e.Property(x => x.Symbol).HasMaxLength(32);
            e.Property(x => x.AlertId).HasMaxLength(50);
            e.Property(x => x.Message).IsRequired().HasMaxLength(1000);
            e.Property(x => x.Details).HasMaxLength(5000);
            e.HasIndex(x => x.Timestamp);
            e.HasIndex(x => x.Category);
        });
    }
}
