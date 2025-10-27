using Microsoft.AspNetCore.Mvc;
using GpsApp.Domain.Aggregates;
using GpsApp.Domain.ValueObjects;
using GpsApp.Infrastructure.Data.Repositories;

namespace GpsApp.Controllers;

/// <summary>
/// Example controller demonstrating EF Core usage with domain aggregates
/// </summary>
[ApiController]
[Route("api/[controller]")]
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
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateShipment([FromBody] CreateShipmentRequest request)
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

            return Ok(new { ShipmentId = shipment.ShipmentId.Value });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Gets a shipment by ID
    /// </summary>
    [HttpGet("{shipmentId}")]
    public async Task<IActionResult> GetShipment(Guid shipmentId)
    {
        try
        {
            var shipment = await _shipmentRepository.GetByIdAsync(new ShipmentId(shipmentId));
            
            if (shipment == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                ShipmentId = shipment.ShipmentId.Value,
                ShipmentDate = shipment.ShipmentDate,
                Status = shipment.Status.ToString(),
                PackageCount = shipment.Packages.Count,
                DeliveryLegCount = shipment.DeliveryLegs.Count
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all shipments
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllShipments()
    {
        try
        {
            var shipments = await _shipmentRepository.GetAllAsync();
            
            var result = shipments.Select(s => new
            {
                ShipmentId = s.ShipmentId.Value,
                ShipmentDate = s.ShipmentDate,
                Status = s.Status.ToString(),
                PackageCount = s.Packages.Count,
                DeliveryLegCount = s.DeliveryLegs.Count
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}

/// <summary>
/// Request DTO for creating a shipment
/// </summary>
public class CreateShipmentRequest
{
    public DateTime ShipmentDate { get; set; }
    public List<PackageRequest> Packages { get; set; } = new();
    public List<DeliveryLegRequest> DeliveryLegs { get; set; } = new();
}

public class PackageRequest
{
    public AddressRequest Sender { get; set; } = new();
    public AddressRequest Recipient { get; set; } = new();
}

public class DeliveryLegRequest
{
    public AddressRequest StartAddress { get; set; } = new();
    public AddressRequest EndAddress { get; set; } = new();
}

public class AddressRequest
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}
