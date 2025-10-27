using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GpsApp.Domain.Aggregates;
using GpsApp.Domain.ValueObjects;

namespace GpsApp.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for Shipment aggregate
/// </summary>
public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        // Primary key
        builder.HasKey(s => s.ShipmentId);
        
        // ShipmentId conversion
        builder.Property(s => s.ShipmentId)
            .HasConversion(
                id => id.Value,
                value => new ShipmentId(value))
            .HasColumnName("ShipmentId");

        // Basic properties
        builder.Property(s => s.ShipmentDate)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Configure owned collection of DeliveryLegs
        builder.OwnsMany(s => s.DeliveryLegs, dl =>
        {
            dl.WithOwner().HasForeignKey("ShipmentId");
            dl.Property<int>("Id").ValueGeneratedOnAdd();
            dl.HasKey("Id");

            // DeliveryLeg properties
            dl.Property(d => d.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            // GatewayId conversion
            dl.Property(d => d.GatewayId)
                .HasConversion(
                    id => id != null ? id.Value : (Guid?)null,
                    value => value.HasValue ? new GatewayId(value.Value) : null)
                .HasColumnName("GatewayId");

            // StartAddress as owned type
            dl.OwnsOne(d => d.StartAddress, address =>
            {
                address.Property(a => a.Street).HasMaxLength(200).IsRequired();
                address.Property(a => a.City).HasMaxLength(100).IsRequired();
                address.Property(a => a.PostalCode).HasMaxLength(20).IsRequired();
                address.Property(a => a.Country).HasMaxLength(100).IsRequired();
            });

            // EndAddress as owned type
            dl.OwnsOne(d => d.EndAddress, address =>
            {
                address.Property(a => a.Street).HasMaxLength(200).IsRequired();
                address.Property(a => a.City).HasMaxLength(100).IsRequired();
                address.Property(a => a.PostalCode).HasMaxLength(20).IsRequired();
                address.Property(a => a.Country).HasMaxLength(100).IsRequired();
            });
        });

        // Configure many-to-many relationship with Packages
        builder.HasMany(s => s.Packages)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "ShipmentPackages",
                j => j.HasOne<Package>().WithMany().HasForeignKey("PackageId"),
                j => j.HasOne<Shipment>().WithMany().HasForeignKey("ShipmentId"),
                j => j.HasKey("ShipmentId", "PackageId"));

        // Ignore domain events (they're handled separately)
        builder.Ignore(s => s.DomainEvents);

        // Configure table name
        builder.ToTable("Shipments");
    }
}
