using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GpsApp.Domain.Aggregates;
using GpsApp.Domain.ValueObjects;
using GpsApp.Infrastructure.Data.Repositories;
using GpsApp.DTO.V2;
using Swashbuckle.AspNetCore.Annotations;

namespace GpsApp.Controllers.V2;

/// <summary>
/// V2 Gateway Controller - Handles gateway operations with Basic Authentication
/// Supports hardware-limited devices that cannot use JWT
/// </summary>
[ApiController]
[Route("api/v2/[controller]")]
[Authorize(AuthenticationSchemes = "Basic")] // Requires Basic Authentication for gateway operations
public class GatewayController : ControllerBase
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IPackageMeasurementRepository _packageMeasurementRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GatewayController(
        IShipmentRepository shipmentRepository,
        IPackageMeasurementRepository packageMeasurementRepository,
        IUnitOfWork unitOfWork)
    {
        _shipmentRepository = shipmentRepository ?? throw new ArgumentNullException(nameof(shipmentRepository));
        _packageMeasurementRepository = packageMeasurementRepository ?? throw new ArgumentNullException(nameof(packageMeasurementRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Gets expected sensor IDs for a delivery leg
    /// Step: Gateway retrieves list of SensorId
    /// </summary>
    /// <param name="deliveryLegId">Unique identifier of the delivery leg</param>
    /// <returns>List of expected sensor IDs for the delivery leg</returns>
    [HttpGet("delivery-legs/{deliveryLegId}/expected-sensors")]
    [SwaggerOperation(
        Summary = "Get Expected Sensors",
        Description = "Gateway retrieves list of expected sensor IDs for a delivery leg",
        OperationId = "GetExpectedSensors",
        Tags = new[] { "Gateway Operations" }
    )]
    [SwaggerResponse(200, "Expected sensors retrieved", typeof(GatewaySensorListV2Response))]
    [SwaggerResponse(401, "Basic authentication required")]
    [SwaggerResponse(403, "Gateway not authorized for this delivery leg")]
    [SwaggerResponse(404, "Delivery leg not found", typeof(ErrorV2Response))]
    public async Task<ActionResult<GatewaySensorListV2Response>> GetExpectedSensors(Guid deliveryLegId)
    {
        try
        {
            var gatewayIdClaim = User.FindFirst("GatewayId");
            if (gatewayIdClaim == null)
            {
                return Unauthorized(new ErrorV2Response { Error = "Gateway ID not found in authentication" });
            }

            // Find shipment containing this delivery leg
            var shipments = await _shipmentRepository.GetAllAsync();
            var shipment = shipments.FirstOrDefault(s => 
                s.DeliveryLegs.Any(dl => dl.DeliveryLegId.Value == deliveryLegId));

            if (shipment == null)
            {
                return NotFound(new ErrorV2Response { Error = "Delivery leg not found" });
            }

            var deliveryLeg = shipment.DeliveryLegs.FirstOrDefault(dl => dl.DeliveryLegId.Value == deliveryLegId);
            if (deliveryLeg == null)
            {
                return NotFound(new ErrorV2Response { Error = "Delivery leg not found" });
            }

            // Verify gateway is connected to this delivery leg
            if (deliveryLeg.GatewayId?.Value.ToString() != gatewayIdClaim.Value)
            {
                return Forbid("Gateway not authorized for this delivery leg");
            }

            var expectedSensorIds = shipment.GetExpectedSensorIds();

            return Ok(new GatewaySensorListV2Response
            {
                GatewayId = deliveryLeg.GatewayId.Value,
                DeliveryLegId = deliveryLegId,
                ExpectedSensorIds = expectedSensorIds.Select(s => s.Value).ToList()
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorV2Response { Error = ex.Message });
        }
    }

    /// <summary>
    /// Reports sensor presence verification
    /// Step: SensorIds respond, Missing SensorIds logged
    /// </summary>
    [HttpPost("delivery-legs/{deliveryLegId}/verify-sensors")]
    public async Task<IActionResult> VerifySensorPresence(Guid deliveryLegId, [FromBody] SensorPresenceV2Request request)
    {
        try
        {
            var gatewayIdClaim = User.FindFirst("GatewayId");
            if (gatewayIdClaim == null)
            {
                return Unauthorized(new ErrorV2Response { Error = "Gateway ID not found in authentication" });
            }

            // Find shipment containing this delivery leg
            var shipments = await _shipmentRepository.GetAllAsync();
            var shipment = shipments.FirstOrDefault(s => 
                s.DeliveryLegs.Any(dl => dl.DeliveryLegId.Value == deliveryLegId));

            if (shipment == null)
            {
                return NotFound(new ErrorV2Response { Error = "Delivery leg not found" });
            }

            var deliveryLeg = shipment.DeliveryLegs.FirstOrDefault(dl => dl.DeliveryLegId.Value == deliveryLegId);
            if (deliveryLeg == null)
            {
                return NotFound(new ErrorV2Response { Error = "Delivery leg not found" });
            }

            // Verify gateway is connected to this delivery leg
            if (deliveryLeg.GatewayId?.Value.ToString() != gatewayIdClaim.Value)
            {
                return Forbid("Gateway not authorized for this delivery leg");
            }

            // Verify sensor presence
            var presentSensorIds = request.PresentSensorIds.Select(id => new SensorId(id)).ToList();
            shipment.VerifySensorPresence(deliveryLeg, presentSensorIds);

            await _unitOfWork.SaveChangesAsync();

            var expectedSensorIds = shipment.GetExpectedSensorIds();
            var missingSensorIds = expectedSensorIds.Except(presentSensorIds).Select(s => s.Value).ToList();

            return Ok(new GatewaySensorListV2Response
            {
                GatewayId = deliveryLeg.GatewayId.Value,
                DeliveryLegId = deliveryLegId,
                ExpectedSensorIds = expectedSensorIds.Select(s => s.Value).ToList(),
                PresentSensorIds = request.PresentSensorIds,
                MissingSensorIds = missingSensorIds
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorV2Response { Error = ex.Message });
        }
    }

    /// <summary>
    /// Records a batch of measurements from sensors
    /// Step: Gateway starts transmitting measurements
    /// </summary>
    [HttpPost("delivery-legs/{deliveryLegId}/measurements")]
    public async Task<IActionResult> RecordMeasurements(Guid deliveryLegId, [FromBody] MeasurementBatchV2Request request)
    {
        try
        {
            var gatewayIdClaim = User.FindFirst("GatewayId");
            if (gatewayIdClaim == null)
            {
                return Unauthorized(new ErrorV2Response { Error = "Gateway ID not found in authentication" });
            }

            // Find shipment containing this delivery leg
            var shipments = await _shipmentRepository.GetAllAsync();
            var shipment = shipments.FirstOrDefault(s => 
                s.DeliveryLegs.Any(dl => dl.DeliveryLegId.Value == deliveryLegId));

            if (shipment == null)
            {
                return NotFound(new ErrorV2Response { Error = "Delivery leg not found" });
            }

            var deliveryLeg = shipment.DeliveryLegs.FirstOrDefault(dl => dl.DeliveryLegId.Value == deliveryLegId);
            if (deliveryLeg == null)
            {
                return NotFound(new ErrorV2Response { Error = "Delivery leg not found" });
            }

            // Verify gateway is connected to this delivery leg
            if (deliveryLeg.GatewayId?.Value.ToString() != gatewayIdClaim.Value)
            {
                return Forbid("Gateway not authorized for this delivery leg");
            }

            // Convert request readings to domain objects
            var readings = request.Readings.Select(r => new MeasurementReading(
                new SensorId(r.SensorId),
                r.Timestamp,
                new Temperature(r.Temperature),
                new Humidity(r.Humidity),
                r.Latitude.HasValue && r.Longitude.HasValue ? new GpsCoordinate(r.Latitude.Value, r.Longitude.Value) : null
            )).ToList();

            // Record measurements in shipment
            shipment.RecordMeasurementBatch(deliveryLeg, readings);

            // Record measurements in individual package measurements
            foreach (var package in shipment.Packages.Where(p => p.HasSensor))
            {
                var packageMeasurement = await _packageMeasurementRepository.GetActiveByPackageAndDeliveryLegAsync(
                    package.PackageId, deliveryLeg.DeliveryLegId);

                if (packageMeasurement != null)
                {
                    var packageReadings = readings.Where(r => r.SensorId.Value == package.SensorId!.Value).ToList();
                    if (packageReadings.Any())
                    {
                        packageMeasurement.RecordMeasurementBatch(packageReadings);
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();

            return Ok(new { 
                Message = "Measurements recorded successfully",
                ReadingCount = readings.Count,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorV2Response { Error = ex.Message });
        }
    }

    /// <summary>
    /// Gets measurement summary for a package during a delivery leg
    /// </summary>
    [HttpGet("delivery-legs/{deliveryLegId}/packages/{packageId}/measurements/summary")]
    public async Task<ActionResult<MeasurementSummaryV2Response>> GetMeasurementSummary(Guid deliveryLegId, Guid packageId)
    {
        try
        {
            var gatewayIdClaim = User.FindFirst("GatewayId");
            if (gatewayIdClaim == null)
            {
                return Unauthorized(new ErrorV2Response { Error = "Gateway ID not found in authentication" });
            }

            // Find shipment containing this delivery leg
            var shipments = await _shipmentRepository.GetAllAsync();
            var shipment = shipments.FirstOrDefault(s => 
                s.DeliveryLegs.Any(dl => dl.DeliveryLegId.Value == deliveryLegId));

            if (shipment == null)
            {
                return NotFound(new ErrorV2Response { Error = "Delivery leg not found" });
            }

            var deliveryLeg = shipment.DeliveryLegs.FirstOrDefault(dl => dl.DeliveryLegId.Value == deliveryLegId);
            if (deliveryLeg == null)
            {
                return NotFound(new ErrorV2Response { Error = "Delivery leg not found" });
            }

            // Verify gateway is connected to this delivery leg
            if (deliveryLeg.GatewayId?.Value.ToString() != gatewayIdClaim.Value)
            {
                return Forbid("Gateway not authorized for this delivery leg");
            }

            var packageMeasurement = await _packageMeasurementRepository.GetActiveByPackageAndDeliveryLegAsync(
                new PackageId(packageId), new DeliveryLegId(deliveryLegId));

            if (packageMeasurement == null)
            {
                return NotFound(new ErrorV2Response { Error = "No active measurement session found for this package" });
            }

            var summary = packageMeasurement.GetSummary();

            return Ok(new MeasurementSummaryV2Response
            {
                PackageId = packageId,
                DeliveryLegId = deliveryLegId,
                SessionStartTime = packageMeasurement.SessionStartTime,
                SessionEndTime = packageMeasurement.SessionEndTime,
                ReadingCount = packageMeasurement.ReadingCount,
                TemperatureStats = summary.TemperatureStats != null ? new TemperatureStatsV2Response
                {
                    Min = summary.TemperatureStats.Min.Value,
                    Max = summary.TemperatureStats.Max.Value,
                    Average = summary.TemperatureStats.Average.Value,
                    ExpectedMin = summary.TemperatureStats.ExpectedMin?.Value,
                    ExpectedMax = summary.TemperatureStats.ExpectedMax?.Value,
                    OutOfRangeCount = summary.TemperatureStats.OutOfRangeCount
                } : null,
                HumidityStats = summary.HumidityStats != null ? new HumidityStatsV2Response
                {
                    Min = summary.HumidityStats.Min.Value,
                    Max = summary.HumidityStats.Max.Value,
                    Average = summary.HumidityStats.Average.Value,
                    ExpectedMin = summary.HumidityStats.ExpectedMin?.Value,
                    ExpectedMax = summary.HumidityStats.ExpectedMax?.Value,
                    OutOfRangeCount = summary.HumidityStats.OutOfRangeCount
                } : null,
                Violations = summary.Violations.ToList()
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorV2Response { Error = ex.Message });
        }
    }

    /// <summary>
    /// Gets recent measurements for a package during a delivery leg
    /// </summary>
    [HttpGet("delivery-legs/{deliveryLegId}/packages/{packageId}/measurements")]
    public async Task<ActionResult<List<MeasurementReadingV2Response>>> GetRecentMeasurements(
        Guid deliveryLegId, 
        Guid packageId,
        [FromQuery] int? limit = 100)
    {
        try
        {
            var gatewayIdClaim = User.FindFirst("GatewayId");
            if (gatewayIdClaim == null)
            {
                return Unauthorized(new ErrorV2Response { Error = "Gateway ID not found in authentication" });
            }

            // Find shipment containing this delivery leg
            var shipments = await _shipmentRepository.GetAllAsync();
            var shipment = shipments.FirstOrDefault(s => 
                s.DeliveryLegs.Any(dl => dl.DeliveryLegId.Value == deliveryLegId));

            if (shipment == null)
            {
                return NotFound(new ErrorV2Response { Error = "Delivery leg not found" });
            }

            var deliveryLeg = shipment.DeliveryLegs.FirstOrDefault(dl => dl.DeliveryLegId.Value == deliveryLegId);
            if (deliveryLeg == null)
            {
                return NotFound(new ErrorV2Response { Error = "Delivery leg not found" });
            }

            // Verify gateway is connected to this delivery leg
            if (deliveryLeg.GatewayId?.Value.ToString() != gatewayIdClaim.Value)
            {
                return Forbid("Gateway not authorized for this delivery leg");
            }

            var packageMeasurement = await _packageMeasurementRepository.GetActiveByPackageAndDeliveryLegAsync(
                new PackageId(packageId), new DeliveryLegId(deliveryLegId));

            if (packageMeasurement == null)
            {
                return NotFound(new ErrorV2Response { Error = "No active measurement session found for this package" });
            }

            var readings = packageMeasurement.Readings
                .OrderByDescending(r => r.Timestamp)
                .Take(limit ?? 100)
                .Select(MapToMeasurementReadingResponse)
                .ToList();

            return Ok(readings);
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorV2Response { Error = ex.Message });
        }
    }

    private static MeasurementReadingV2Response MapToMeasurementReadingResponse(MeasurementReading reading)
    {
        return new MeasurementReadingV2Response
        {
            SensorId = reading.SensorId.Value,
            Timestamp = reading.Timestamp,
            Temperature = reading.Temperature.Value,
            Humidity = reading.Humidity.Value,
            Latitude = reading.GpsCoordinate?.Latitude,
            Longitude = reading.GpsCoordinate?.Longitude,
            IsTemperatureInRange = reading.IsTemperatureInRange(null),
            IsHumidityInRange = reading.IsHumidityInRange(null),
            ViolationType = reading.GetViolationType(null, null)
        };
    }
}
