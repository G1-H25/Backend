using Microsoft.EntityFrameworkCore;
using GpsApp.Domain.Aggregates;
using GpsApp.Domain.Events;
using GpsApp.Infrastructure.Data.Configurations;

namespace GpsApp.Infrastructure.Data;

/// <summary>
/// Entity Framework Core DbContext for the GPS application
/// Manages persistence of domain aggregates and value objects
/// </summary>
public class GpsAppDbContext : DbContext
{
    public GpsAppDbContext(DbContextOptions<GpsAppDbContext> options) : base(options)
    {
    }

    // Domain aggregates
    public DbSet<Shipment> Shipments { get; set; } = null!;
    public DbSet<Package> Packages { get; set; } = null!;
    public DbSet<PackageMeasurement> PackageMeasurements { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new ShipmentConfiguration());
        modelBuilder.ApplyConfiguration(new PackageConfiguration());
        modelBuilder.ApplyConfiguration(new PackageMeasurementConfiguration());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // This is only used for design-time operations like migrations
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=GpsAppDb;Trusted_Connection=true;MultipleActiveResultSets=true");
        }
    }
}
