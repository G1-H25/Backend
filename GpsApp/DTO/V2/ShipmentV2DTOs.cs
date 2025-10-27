using GpsApp.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace GpsApp.DTO.V2;

/// <summary>
/// V2 API DTOs for shipment workflow operations
/// </summary>

// Shipment Management DTOs
public class CreateShipmentV2Request
{
    [Required]
    public DateTime ShipmentDate { get; set; }
    
    [Required]
    [MinLength(1)]
    public List<PackageV2Request> Packages { get; set; } = new();
    
    [Required]
    [MinLength(1)]
    public List<DeliveryLegV2Request> DeliveryLegs { get; set; } = new();
}

public class ShipmentV2Response
{
    public Guid ShipmentId { get; set; }
    public DateTime ShipmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<PackageV2Response> Packages { get; set; } = new();
    public List<DeliveryLegV2Response> DeliveryLegs { get; set; } = new();
    public AddressV2Response? StartingAddress { get; set; }
    public AddressV2Response? EndingAddress { get; set; }
}

// Package Management DTOs
public class PackageV2Request
{
    public AddressV2Request Sender { get; set; } = new();
    public AddressV2Request Recipient { get; set; } = new();
    public TemperatureRangeV2Request? ExpectedTemperatureRange { get; set; }
    public HumidityRangeV2Request? ExpectedHumidityRange { get; set; }
}

public class PackageV2Response
{
    public Guid PackageId { get; set; }
    public AddressV2Response Sender { get; set; } = new();
    public AddressV2Response Recipient { get; set; } = new();
    public TemperatureRangeV2Response? ExpectedTemperatureRange { get; set; }
    public HumidityRangeV2Response? ExpectedHumidityRange { get; set; }
    public Guid? SensorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SensorAttachedAt { get; set; }
    public bool HasSensor { get; set; }
    public bool IsReadyForShipping { get; set; }
}

public class AttachSensorV2Request
{
    public Guid PackageId { get; set; }
    public Guid SensorId { get; set; }
}

// Delivery Leg Management DTOs
public class DeliveryLegV2Request
{
    public AddressV2Request StartAddress { get; set; } = new();
    public AddressV2Request EndAddress { get; set; } = new();
}

public class DeliveryLegV2Response
{
    public Guid DeliveryLegId { get; set; }
    public AddressV2Response StartAddress { get; set; } = new();
    public AddressV2Response EndAddress { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public Guid? GatewayId { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class ConnectGatewayV2Request
{
    public Guid DeliveryLegId { get; set; }
    public Guid GatewayId { get; set; }
}

public class StartDeliveryLegV2Request
{
    public Guid DeliveryLegId { get; set; }
}

public class CompleteDeliveryLegV2Request
{
    public Guid DeliveryLegId { get; set; }
}

// Gateway Operations DTOs
public class GatewaySensorListV2Response
{
    public Guid GatewayId { get; set; }
    public Guid DeliveryLegId { get; set; }
    public List<Guid> ExpectedSensorIds { get; set; } = new();
    public List<Guid> PresentSensorIds { get; set; } = new();
    public List<Guid> MissingSensorIds { get; set; } = new();
}

public class SensorPresenceV2Request
{
    public Guid DeliveryLegId { get; set; }
    public List<Guid> PresentSensorIds { get; set; } = new();
}

public class MeasurementBatchV2Request
{
    public Guid DeliveryLegId { get; set; }
    public List<MeasurementReadingV2Request> Readings { get; set; } = new();
}

public class MeasurementReadingV2Request
{
    public Guid SensorId { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal Temperature { get; set; }
    public decimal Humidity { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

public class MeasurementReadingV2Response
{
    public Guid SensorId { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal Temperature { get; set; }
    public decimal Humidity { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool IsTemperatureInRange { get; set; }
    public bool IsHumidityInRange { get; set; }
    public string? ViolationType { get; set; }
}

// Measurement Summary DTOs
public class MeasurementSummaryV2Response
{
    public Guid PackageId { get; set; }
    public Guid DeliveryLegId { get; set; }
    public DateTime SessionStartTime { get; set; }
    public DateTime? SessionEndTime { get; set; }
    public int ReadingCount { get; set; }
    public TemperatureStatsV2Response? TemperatureStats { get; set; }
    public HumidityStatsV2Response? HumidityStats { get; set; }
    public List<string> Violations { get; set; } = new();
}

public class TemperatureStatsV2Response
{
    public decimal Min { get; set; }
    public decimal Max { get; set; }
    public decimal Average { get; set; }
    public decimal? ExpectedMin { get; set; }
    public decimal? ExpectedMax { get; set; }
    public int OutOfRangeCount { get; set; }
}

public class HumidityStatsV2Response
{
    public decimal Min { get; set; }
    public decimal Max { get; set; }
    public decimal Average { get; set; }
    public decimal? ExpectedMin { get; set; }
    public decimal? ExpectedMax { get; set; }
    public int OutOfRangeCount { get; set; }
}

// Address DTOs
public class AddressV2Request
{
    [Required]
    public string Street { get; set; } = string.Empty;
    
    [Required]
    public string City { get; set; } = string.Empty;
    
    [Required]
    public string PostalCode { get; set; } = string.Empty;
    
    [Required]
    public string Country { get; set; } = string.Empty;
}

public class AddressV2Response
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

// Range DTOs
public class TemperatureRangeV2Request
{
    public decimal Min { get; set; }
    public decimal Max { get; set; }
}

public class TemperatureRangeV2Response
{
    public decimal Min { get; set; }
    public decimal Max { get; set; }
}

public class HumidityRangeV2Request
{
    public decimal Min { get; set; }
    public decimal Max { get; set; }
}

public class HumidityRangeV2Response
{
    public decimal Min { get; set; }
    public decimal Max { get; set; }
}

// Error Response DTO
public class ErrorV2Response
{
    public string Error { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
