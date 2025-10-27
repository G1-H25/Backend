using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GpsApp.Domain.Aggregates;
using GpsApp.Domain.ValueObjects;

namespace GpsApp.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for PackageMeasurement aggregate
/// </summary>
public class PackageMeasurementConfiguration : IEntityTypeConfiguration<PackageMeasurement>
{
    public void Configure(EntityTypeBuilder<PackageMeasurement> builder)
    {
        builder.ToTable("PackageMeasurements");

        // Primary key
        builder.HasKey(pm => pm.Id);
        builder.Property(pm => pm.Id)
            .HasConversion(id => id.Value, value => new PackageMeasurementId(value))
            .HasColumnName("Id");

        // PackageId foreign key
        builder.Property(pm => pm.PackageId)
            .HasConversion(id => id.Value, value => new PackageId(value))
            .HasColumnName("PackageId");

        // DeliveryLeg as owned entity
        builder.OwnsOne(pm => pm.DeliveryLeg, deliveryLeg =>
        {
            deliveryLeg.Property(dl => dl.DeliveryLegId)
                .HasConversion(id => id.Value, value => new DeliveryLegId(value))
                .HasColumnName("DeliveryLegId");

            deliveryLeg.OwnsOne(dl => dl.StartAddress, address =>
            {
                address.Property(a => a.Street).HasColumnName("StartStreet");
                address.Property(a => a.City).HasColumnName("StartCity");
                address.Property(a => a.PostalCode).HasColumnName("StartPostalCode");
                address.Property(a => a.Country).HasColumnName("StartCountry");
            });

            deliveryLeg.OwnsOne(dl => dl.EndAddress, address =>
            {
                address.Property(a => a.Street).HasColumnName("EndStreet");
                address.Property(a => a.City).HasColumnName("EndCity");
                address.Property(a => a.PostalCode).HasColumnName("EndPostalCode");
                address.Property(a => a.Country).HasColumnName("EndCountry");
            });

            deliveryLeg.Property(dl => dl.Status)
                .HasConversion(status => status.ToString(), value => Enum.Parse<DeliveryLegStatus>(value))
                .HasColumnName("DeliveryLegStatus");

            deliveryLeg.Property(dl => dl.GatewayId)
                .HasConversion(id => id!.Value, value => new GatewayId(value))
                .HasColumnName("GatewayId");

            deliveryLeg.Property(dl => dl.StartedAt)
                .HasColumnName("StartedAt");

            deliveryLeg.Property(dl => dl.CompletedAt)
                .HasColumnName("CompletedAt");
        });

        // Expected ranges as owned entities
        builder.OwnsOne(pm => pm.ExpectedTemperatureRange, range =>
        {
            range.Property(r => r.Min)
                .HasConversion(temp => temp.Value, value => new Temperature(value))
                .HasColumnName("ExpectedTempMin");
            range.Property(r => r.Max)
                .HasConversion(temp => temp.Value, value => new Temperature(value))
                .HasColumnName("ExpectedTempMax");
        });

        builder.OwnsOne(pm => pm.ExpectedHumidityRange, range =>
        {
            range.Property(r => r.Min)
                .HasConversion(humidity => humidity.Value, value => new Humidity(value))
                .HasColumnName("ExpectedHumidityMin");
            range.Property(r => r.Max)
                .HasConversion(humidity => humidity.Value, value => new Humidity(value))
                .HasColumnName("ExpectedHumidityMax");
        });

        // Timestamps
        builder.Property(pm => pm.SessionStartTime)
            .HasColumnName("SessionStartTime");
        builder.Property(pm => pm.SessionEndTime)
            .HasColumnName("SessionEndTime");

        // Measurements as owned collection
        builder.OwnsMany(pm => pm.Readings, reading =>
        {
            reading.WithOwner()
                .HasForeignKey("PackageMeasurementId");

            reading.Property(r => r.SensorId)
                .HasConversion(id => id.Value, value => new SensorId(value))
                .HasColumnName("SensorId");

            reading.Property(r => r.Timestamp)
                .HasColumnName("Timestamp");

            reading.Property(r => r.Temperature)
                .HasConversion(temp => temp.Value, value => new Temperature(value))
                .HasColumnName("Temperature");

            reading.Property(r => r.Humidity)
                .HasConversion(humidity => humidity.Value, value => new Humidity(value))
                .HasColumnName("Humidity");

            reading.OwnsOne(r => r.GpsCoordinate, gps =>
            {
                gps.Property(g => g.Latitude).HasColumnName("Latitude");
                gps.Property(g => g.Longitude).HasColumnName("Longitude");
            });
        });
    }
}