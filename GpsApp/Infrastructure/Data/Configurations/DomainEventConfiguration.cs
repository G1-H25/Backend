using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GpsApp.Domain.Events;

namespace GpsApp.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for Domain Events
/// Simplified configuration for now
/// </summary>
public class PackageCreatedEventConfiguration : IEntityTypeConfiguration<PackageCreatedEvent>
{
    public void Configure(EntityTypeBuilder<PackageCreatedEvent> builder)
    {
        DomainEventConfigurationHelper.ConfigureBaseEvent(builder);
        builder.ToTable("DomainEvents");
    }
}

public class SensorAttachedEventConfiguration : IEntityTypeConfiguration<SensorAttachedEvent>
{
    public void Configure(EntityTypeBuilder<SensorAttachedEvent> builder)
    {
        DomainEventConfigurationHelper.ConfigureBaseEvent(builder);
        builder.ToTable("DomainEvents");
    }
}

public class ExpectedTemperatureRangeSetEventConfiguration : IEntityTypeConfiguration<ExpectedTemperatureRangeSetEvent>
{
    public void Configure(EntityTypeBuilder<ExpectedTemperatureRangeSetEvent> builder)
    {
        DomainEventConfigurationHelper.ConfigureBaseEvent(builder);
        builder.ToTable("DomainEvents");
    }
}

public class ExpectedHumidityRangeSetEventConfiguration : IEntityTypeConfiguration<ExpectedHumidityRangeSetEvent>
{
    public void Configure(EntityTypeBuilder<ExpectedHumidityRangeSetEvent> builder)
    {
        DomainEventConfigurationHelper.ConfigureBaseEvent(builder);
        builder.ToTable("DomainEvents");
    }
}

public class ShipmentCreatedEventConfiguration : IEntityTypeConfiguration<ShipmentCreatedEvent>
{
    public void Configure(EntityTypeBuilder<ShipmentCreatedEvent> builder)
    {
        DomainEventConfigurationHelper.ConfigureBaseEvent(builder);
        builder.ToTable("DomainEvents");
    }
}

public class PackageAddedToShipmentEventConfiguration : IEntityTypeConfiguration<PackageAddedToShipmentEvent>
{
    public void Configure(EntityTypeBuilder<PackageAddedToShipmentEvent> builder)
    {
        DomainEventConfigurationHelper.ConfigureBaseEvent(builder);
        builder.ToTable("DomainEvents");
    }
}

public class DeliveryLegAddedToShipmentEventConfiguration : IEntityTypeConfiguration<DeliveryLegAddedToShipmentEvent>
{
    public void Configure(EntityTypeBuilder<DeliveryLegAddedToShipmentEvent> builder)
    {
        DomainEventConfigurationHelper.ConfigureBaseEvent(builder);
        builder.ToTable("DomainEvents");
    }
}

public class GatewayConnectedToDeliveryLegEventConfiguration : IEntityTypeConfiguration<GatewayConnectedToDeliveryLegEvent>
{
    public void Configure(EntityTypeBuilder<GatewayConnectedToDeliveryLegEvent> builder)
    {
        DomainEventConfigurationHelper.ConfigureBaseEvent(builder);
        builder.ToTable("DomainEvents");
    }
}

public class ShipmentStatusChangedEventConfiguration : IEntityTypeConfiguration<ShipmentStatusChangedEvent>
{
    public void Configure(EntityTypeBuilder<ShipmentStatusChangedEvent> builder)
    {
        DomainEventConfigurationHelper.ConfigureBaseEvent(builder);
        builder.ToTable("DomainEvents");
    }
}

public class DeliveryLegStartedEventConfiguration : IEntityTypeConfiguration<DeliveryLegStartedEvent>
{
    public void Configure(EntityTypeBuilder<DeliveryLegStartedEvent> builder)
    {
        DomainEventConfigurationHelper.ConfigureBaseEvent(builder);
        builder.ToTable("DomainEvents");
    }
}

public class DeliveryLegCompletedEventConfiguration : IEntityTypeConfiguration<DeliveryLegCompletedEvent>
{
    public void Configure(EntityTypeBuilder<DeliveryLegCompletedEvent> builder)
    {
        DomainEventConfigurationHelper.ConfigureBaseEvent(builder);
        builder.ToTable("DomainEvents");
    }
}

/// <summary>
/// Helper method to configure common domain event properties
/// </summary>
public static class DomainEventConfigurationHelper
{
    public static void ConfigureBaseEvent<T>(EntityTypeBuilder<T> builder) where T : DomainEvent
    {
        // Primary key
        builder.HasKey(e => e.EventId);
        
        // EventId
        builder.Property(e => e.EventId)
            .IsRequired();

        // OccurredAt
        builder.Property(e => e.OccurredAt)
            .IsRequired()
            .HasColumnType("datetime2");

        // Aggregate ID (for event sourcing)
        builder.Property(e => e.AggregateId)
            .HasMaxLength(50)
            .IsRequired();

        // Aggregate type
        builder.Property(e => e.AggregateType)
            .HasMaxLength(100)
            .IsRequired();

        // Version for optimistic concurrency
        builder.Property(e => e.Version)
            .IsRequired();

        // Indexes for performance
        builder.HasIndex(e => e.AggregateId);
        builder.HasIndex(e => e.AggregateType);
        builder.HasIndex(e => e.OccurredAt);
    }
}
