using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using GpsApp.Domain.Aggregates;
using GpsApp.Domain.ValueObjects;

namespace GpsApp.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for Package aggregate
/// </summary>
public class PackageConfiguration : IEntityTypeConfiguration<Package>
{
    public void Configure(EntityTypeBuilder<Package> builder)
    {
        // Primary key
        builder.HasKey(p => p.PackageId);
        
        // PackageId conversion
        builder.Property(p => p.PackageId)
            .HasConversion(
                id => id.Value,
                value => new PackageId(value))
            .HasColumnName("PackageId");

        // Sender as owned type
        builder.OwnsOne(p => p.Sender, sender =>
        {
            sender.Property(s => s.Street).HasMaxLength(200).IsRequired();
            sender.Property(s => s.City).HasMaxLength(100).IsRequired();
            sender.Property(s => s.PostalCode).HasMaxLength(20).IsRequired();
            sender.Property(s => s.Country).HasMaxLength(100).IsRequired();
        });

        // Recipient as owned type
        builder.OwnsOne(p => p.Recipient, recipient =>
        {
            recipient.Property(r => r.Street).HasMaxLength(200).IsRequired();
            recipient.Property(r => r.City).HasMaxLength(100).IsRequired();
            recipient.Property(r => r.PostalCode).HasMaxLength(20).IsRequired();
            recipient.Property(r => r.Country).HasMaxLength(100).IsRequired();
        });

        // Convert SensorId to its underlying Guid for EF Core
        var sensorIdConverter = new ValueConverter<SensorId, Guid>(
            id => id.Value,       
            value => new SensorId(value)); 

        builder.Property(p => p.SensorId)
            .HasConversion(sensorIdConverter)
            .HasColumnName("SensorId");


        // ExpectedTemperatureRange as owned type
        builder.OwnsOne(p => p.ExpectedTemperatureRange, tempRange =>
        {
            tempRange.Property(tr => tr.Minimum)
                .HasConversion(
                    temp => temp.Value,
                    value => new Temperature(value))
                .HasColumnType("decimal(5,2)");
            
            tempRange.Property(tr => tr.Maximum)
                .HasConversion(
                    temp => temp.Value,
                    value => new Temperature(value))
                .HasColumnType("decimal(5,2)");
        });

        // ExpectedHumidityRange as owned type
        builder.OwnsOne(p => p.ExpectedHumidityRange, humidRange =>
        {
            humidRange.Property(hr => hr.Minimum)
                .HasConversion(
                    humid => humid.Value,
                    value => new Humidity(value))
                .HasColumnType("decimal(5,2)");
            
            humidRange.Property(hr => hr.Maximum)
                .HasConversion(
                    humid => humid.Value,
                    value => new Humidity(value))
                .HasColumnType("decimal(5,2)");
        });

        // DateTime properties
        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(p => p.SensorAttachedAt)
            .HasColumnType("datetime2");

        // Ignore domain events (they're handled separately)
        builder.Ignore(p => p.DomainEvents);

        // Configure table name
        builder.ToTable("Packages");
    }
}
