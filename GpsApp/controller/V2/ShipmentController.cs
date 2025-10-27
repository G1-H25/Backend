using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GpsApp.Domain.Aggregates;
using GpsApp.Domain.ValueObjects;
using GpsApp.Infrastructure.Data.Repositories;
using GpsApp.DTO.V2;
using Swashbuckle.AspNetCore.Annotations;

namespace GpsApp.Controllers.V2;

/// <summary>
/// V2 Shipment Controller - Independent API supporting complete shipment workflow
/// Follows domain business logic workflow from DOMAIN_BUSINESS_LOGIC.md
/// </summary>
[ApiController]
[Route("api/v2/[controller]")]
[Authorize] // Requires JWT authentication for user operations
public class ShipmentController : ControllerBase
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IPackageRepository _packageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ShipmentController(
        IShipmentRepository shipmentRepository,
        IPackageRepository packageRepository,
        IUnitOfWork unitOfWork)
    {
        _shipmentRepository = shipmentRepository ?? throw new ArgumentNullException(nameof(shipmentRepository));
        _packageRepository = packageRepository ?? throw new ArgumentNullException(nameof(packageRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Creates a new shipment with packages and delivery legs
    /// Step: Intent for shipment established
    /// </summary>
    /// <param name="request">Shipment creation request with Swedish address format (e.g., Tegelgatan 12, Stockholm, 113 58, Sverige)</param>
    /// <returns>Created shipment with full details</returns>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create Shipment",
        Description = "Creates a new shipment with packages and delivery legs. Uses Swedish address format: Street, City, Postal Code, Country (e.g., Tegelgatan 12, Stockholm, 113 58, Sverige)",
        OperationId = "CreateShipment",
        Tags = new[] { "Shipment Management" }
    )]
    [SwaggerResponse(200, "Shipment created successfully", typeof(ShipmentV2Response))]
    [SwaggerResponse(400, "Invalid request data", typeof(ErrorV2Response))]
    [SwaggerResponse(401, "Authentication required")]
    public async Task<ActionResult<ShipmentV2Response>> CreateShipment([FromBody] CreateShipmentV2Request request)
    {
        try
        {
            // Create packages
            var packages = new List<Package>();
            foreach (var packageRequest in request.Packages)
            {
                var sender = new Address(
                    packageRequest.Sender.Street,
                    packageRequest.Sender.City,
                    packageRequest.Sender.PostalCode,
                    packageRequest.Sender.Country);

                var recipient = new Address(
                    packageRequest.Recipient.Street,
                    packageRequest.Recipient.City,
                    packageRequest.Recipient.PostalCode,
                    packageRequest.Recipient.Country);

                var package = new Package(PackageId.NewId(), sender, recipient);
                
                // Set expected ranges if provided
                if (packageRequest.ExpectedTemperatureRange != null)
                {
                    var tempRange = new ExpectedRange<Temperature>(
                        new Temperature(packageRequest.ExpectedTemperatureRange.Min),
                        new Temperature(packageRequest.ExpectedTemperatureRange.Max));
                    package.SetExpectedTemperatureRange(tempRange);
                }

                if (packageRequest.ExpectedHumidityRange != null)
                {
                    var humidityRange = new ExpectedRange<Humidity>(
                        new Humidity(packageRequest.ExpectedHumidityRange.Min),
                        new Humidity(packageRequest.ExpectedHumidityRange.Max));
                    package.SetExpectedHumidityRange(humidityRange);
                }

                packages.Add(package);
            }

            // Create delivery legs
            var deliveryLegs = new List<DeliveryLeg>();
            foreach (var legRequest in request.DeliveryLegs)
            {
                var startAddress = new Address(
                    legRequest.StartAddress.Street,
                    legRequest.StartAddress.City,
                    legRequest.StartAddress.PostalCode,
                    legRequest.StartAddress.Country);

                var endAddress = new Address(
                    legRequest.EndAddress.Street,
                    legRequest.EndAddress.City,
                    legRequest.EndAddress.PostalCode,
                    legRequest.EndAddress.Country);

                var deliveryLeg = new DeliveryLeg(startAddress, endAddress);
                deliveryLegs.Add(deliveryLeg);
            }

            // Create shipment
            var shipment = new Shipment(
                ShipmentId.NewId(),
                request.ShipmentDate,
                packages,
                deliveryLegs);

            // Save to database
            await _unitOfWork.Shipments.AddAsync(shipment);
            await _unitOfWork.SaveChangesAsync();

            return Ok(MapToShipmentResponse(shipment));
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorV2Response { Error = ex.Message });
        }
    }

    /// <summary>
    /// Gets a shipment by ID with full details
    /// </summary>
    /// <param name="shipmentId">Unique identifier of the shipment</param>
    /// <returns>Shipment details with Swedish address format</returns>
    [HttpGet("{shipmentId}")]
    [SwaggerOperation(
        Summary = "Get Shipment by ID",
        Description = "Retrieves a shipment by ID with full details including Swedish address format",
        OperationId = "GetShipment",
        Tags = new[] { "Shipment Management" }
    )]
    [SwaggerResponse(200, "Shipment found", typeof(ShipmentV2Response))]
    [SwaggerResponse(404, "Shipment not found", typeof(ErrorV2Response))]
    [SwaggerResponse(401, "Authentication required")]
    public async Task<ActionResult<ShipmentV2Response>> GetShipment(Guid shipmentId)
    {
        try
        {
            var shipment = await _shipmentRepository.GetByIdAsync(new ShipmentId(shipmentId));
            
            if (shipment == null)
            {
                return NotFound(new ErrorV2Response { Error = "Shipment not found" });
            }

            return Ok(MapToShipmentResponse(shipment));
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorV2Response { Error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all shipments for the authenticated user
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ShipmentV2Response>>> GetAllShipments()
    {
        try
        {
            var shipments = await _shipmentRepository.GetAllAsync();
            
            var result = shipments.Select(MapToShipmentResponse).ToList();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorV2Response { Error = ex.Message });
        }
    }

    /// <summary>
    /// Attaches a sensor to a package
    /// Step: Package sealed - Sensor registered on package
    /// </summary>
    [HttpPost("{shipmentId}/packages/{packageId}/attach-sensor")]
    public async Task<IActionResult> AttachSensor(Guid shipmentId, Guid packageId, [FromBody] AttachSensorV2Request request)
    {
        try
        {
            var shipment = await _shipmentRepository.GetByIdAsync(new ShipmentId(shipmentId));
            if (shipment == null)
            {
                return NotFound(new ErrorV2Response { Error = "Shipment not found" });
            }

            var package = shipment.Packages.FirstOrDefault(p => p.PackageId.Value == packageId);
            if (package == null)
            {
                return NotFound(new ErrorV2Response { Error = "Package not found in shipment" });
            }

            package.AttachSensor(new SensorId(request.SensorId));

            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "Sensor attached successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorV2Response { Error = ex.Message });
        }
    }

    /// <summary>
    /// Connects a gateway to a delivery leg
    /// Step: Packages loaded in vehicle - Packages connected to Gateway ECU Id
    /// </summary>
    [HttpPost("{shipmentId}/delivery-legs/{deliveryLegId}/connect-gateway")]
    public async Task<IActionResult> ConnectGateway(Guid shipmentId, Guid deliveryLegId, [FromBody] ConnectGatewayV2Request request)
    {
        try
        {
            var shipment = await _shipmentRepository.GetByIdAsync(new ShipmentId(shipmentId));
            if (shipment == null)
            {
                return NotFound(new ErrorV2Response { Error = "Shipment not found" });
            }

            var deliveryLeg = shipment.DeliveryLegs.FirstOrDefault(dl => dl.DeliveryLegId.Value == deliveryLegId);
            if (deliveryLeg == null)
            {
                return NotFound(new ErrorV2Response { Error = "Delivery leg not found in shipment" });
            }

            shipment.ConnectGatewayToDeliveryLeg(deliveryLeg, new GatewayId(request.GatewayId));

            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "Gateway connected successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorV2Response { Error = ex.Message });
        }
    }

    /// <summary>
    /// Starts a delivery leg
    /// Step: Shipment can be set to In Progress
    /// </summary>
    [HttpPost("{shipmentId}/delivery-legs/{deliveryLegId}/start")]
    public async Task<IActionResult> StartDeliveryLeg(Guid shipmentId, Guid deliveryLegId)
    {
        try
        {
            var shipment = await _shipmentRepository.GetByIdAsync(new ShipmentId(shipmentId));
            if (shipment == null)
            {
                return NotFound(new ErrorV2Response { Error = "Shipment not found" });
            }

            var deliveryLeg = shipment.DeliveryLegs.FirstOrDefault(dl => dl.DeliveryLegId.Value == deliveryLegId);
            if (deliveryLeg == null)
            {
                return NotFound(new ErrorV2Response { Error = "Delivery leg not found in shipment" });
            }

            shipment.StartDeliveryLeg(deliveryLeg);

            // Update shipment status to Shipped if this is the first leg
            if (shipment.Status == Shipment.ShipmentStatus.Pending)
            {
                shipment.ChangeStatus(Shipment.ShipmentStatus.Shipped);
            }

            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "Delivery leg started successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorV2Response { Error = ex.Message });
        }
    }

    /// <summary>
    /// Completes a delivery leg
    /// Step: Last leg completed - Shipment marked as completed
    /// </summary>
    [HttpPost("{shipmentId}/delivery-legs/{deliveryLegId}/complete")]
    public async Task<IActionResult> CompleteDeliveryLeg(Guid shipmentId, Guid deliveryLegId)
    {
        try
        {
            var shipment = await _shipmentRepository.GetByIdAsync(new ShipmentId(shipmentId));
            if (shipment == null)
            {
                return NotFound(new ErrorV2Response { Error = "Shipment not found" });
            }

            var deliveryLeg = shipment.DeliveryLegs.FirstOrDefault(dl => dl.DeliveryLegId.Value == deliveryLegId);
            if (deliveryLeg == null)
            {
                return NotFound(new ErrorV2Response { Error = "Delivery leg not found in shipment" });
            }

            shipment.CompleteDeliveryLeg(deliveryLeg);

            // Check if this was the last leg and complete the shipment
            var allLegsCompleted = shipment.DeliveryLegs.All(dl => dl.Status == DeliveryLegStatus.Completed);
            if (allLegsCompleted)
            {
                shipment.ChangeStatus(Shipment.ShipmentStatus.Delivered);
            }

            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "Delivery leg completed successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorV2Response { Error = ex.Message });
        }
    }

    /// <summary>
    /// Gets expected sensor IDs for a delivery leg
    /// Used by gateway to know which sensors to expect
    /// </summary>
    [HttpGet("{shipmentId}/delivery-legs/{deliveryLegId}/expected-sensors")]
    public async Task<ActionResult<GatewaySensorListV2Response>> GetExpectedSensors(Guid shipmentId, Guid deliveryLegId)
    {
        try
        {
            var shipment = await _shipmentRepository.GetByIdAsync(new ShipmentId(shipmentId));
            if (shipment == null)
            {
                return NotFound(new ErrorV2Response { Error = "Shipment not found" });
            }

            var deliveryLeg = shipment.DeliveryLegs.FirstOrDefault(dl => dl.DeliveryLegId.Value == deliveryLegId);
            if (deliveryLeg == null)
            {
                return NotFound(new ErrorV2Response { Error = "Delivery leg not found in shipment" });
            }

            var expectedSensorIds = shipment.GetExpectedSensorIds();

            return Ok(new GatewaySensorListV2Response
            {
                GatewayId = deliveryLeg.GatewayId?.Value ?? Guid.Empty,
                DeliveryLegId = deliveryLegId,
                ExpectedSensorIds = expectedSensorIds.Select(s => new Guid(s.ToString())).ToList()
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorV2Response { Error = ex.Message });
        }
    }

    private static ShipmentV2Response MapToShipmentResponse(Shipment shipment)
    {
        return new ShipmentV2Response
        {
            ShipmentId = shipment.ShipmentId.Value,
            ShipmentDate = shipment.ShipmentDate,
            Status = shipment.Status.ToString(),
            Packages = shipment.Packages.Select(MapToPackageResponse).ToList(),
            DeliveryLegs = shipment.DeliveryLegs.Select(MapToDeliveryLegResponse).ToList(),
            StartingAddress = shipment.StartingAddress != null ? MapToAddressResponse(shipment.StartingAddress) : null,
            EndingAddress = shipment.EndingAddress != null ? MapToAddressResponse(shipment.EndingAddress) : null
        };
    }

    private static PackageV2Response MapToPackageResponse(Package package)
    {
        return new PackageV2Response
        {
            PackageId = package.PackageId.Value,
            Sender = MapToAddressResponse(package.Sender),
            Recipient = MapToAddressResponse(package.Recipient),
            ExpectedTemperatureRange = package.ExpectedTemperatureRange != null ? new TemperatureRangeV2Response
            {
                Min = package.ExpectedTemperatureRange.Min.Value,
                Max = package.ExpectedTemperatureRange.Max.Value
            } : null,
            ExpectedHumidityRange = package.ExpectedHumidityRange != null ? new HumidityRangeV2Response
            {
                Min = package.ExpectedHumidityRange.Min.Value,
                Max = package.ExpectedHumidityRange.Max.Value
            } : null,
            SensorId = package.SensorId?.Value,
            CreatedAt = package.CreatedAt,
            SensorAttachedAt = package.SensorAttachedAt,
            HasSensor = package.HasSensor,
            IsReadyForShipping = package.IsReadyForShipping
        };
    }

    private static DeliveryLegV2Response MapToDeliveryLegResponse(DeliveryLeg deliveryLeg)
    {
        return new DeliveryLegV2Response
        {
            DeliveryLegId = deliveryLeg.DeliveryLegId.Value,
            StartAddress = MapToAddressResponse(deliveryLeg.StartAddress),
            EndAddress = MapToAddressResponse(deliveryLeg.EndAddress),
            Status = deliveryLeg.Status.ToString(),
            GatewayId = deliveryLeg.GatewayId?.Value,
            StartedAt = deliveryLeg.StartedAt,
            CompletedAt = deliveryLeg.CompletedAt
        };
    }

    private static AddressV2Response MapToAddressResponse(Address address)
    {
        return new AddressV2Response
        {
            Street = address.Street,
            City = address.City,
            PostalCode = address.PostalCode,
            Country = address.Country
        };
    }
}
