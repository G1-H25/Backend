using Microsoft.AspNetCore.Mvc;
using Moq;
using GpsApp.Controllers.V2;
using GpsApp.Domain.Aggregates;
using GpsApp.Domain.ValueObjects;
using GpsApp.Infrastructure.Data.Repositories;
using GpsApp.DTO.V2;

namespace GpsApp.Tests.Unit.Controllers.V2;

/// <summary>
/// Unit tests for V2 ShipmentController
/// Tests follow Arrange-Act-Assert pattern
/// </summary>
public class ShipmentControllerTests
{
    private readonly Mock<IShipmentRepository> _mockShipmentRepository;
    private readonly Mock<IPackageRepository> _mockPackageRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly ShipmentController _controller;

    public ShipmentControllerTests()
    {
        _mockShipmentRepository = new Mock<IShipmentRepository>();
        _mockPackageRepository = new Mock<IPackageRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        
        _controller = new ShipmentController(
            _mockShipmentRepository.Object,
            _mockPackageRepository.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task CreateShipment_WithValidRequest_ReturnsOkWithShipmentId()
    {
        // Arrange
        var request = new CreateShipmentV2Request
        {
            ShipmentDate = DateTime.UtcNow,
            Packages = new List<PackageV2Request>
            {
                new()
                {
                    Sender = new AddressV2Request { Street = "123 Main St", City = "New York", PostalCode = "10001", Country = "USA" },
                    Recipient = new AddressV2Request { Street = "456 Oak Ave", City = "Los Angeles", PostalCode = "90210", Country = "USA" },
                    ExpectedTemperatureRange = new TemperatureRangeV2Request { Min = 2, Max = 8 },
                    ExpectedHumidityRange = new HumidityRangeV2Request { Min = 30, Max = 70 }
                }
            },
            DeliveryLegs = new List<DeliveryLegV2Request>
            {
                new()
                {
                    StartAddress = new AddressV2Request { Street = "123 Main St", City = "New York", PostalCode = "10001", Country = "USA" },
                    EndAddress = new AddressV2Request { Street = "456 Oak Ave", City = "Los Angeles", PostalCode = "90210", Country = "USA" }
                }
            }
        };

        _mockUnitOfWork.Setup(u => u.Shipments.AddAsync(It.IsAny<Shipment>()));
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _controller.CreateShipment(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ShipmentV2Response>(okResult.Value);
        Assert.NotEqual(Guid.Empty, response.ShipmentId);
        Assert.Equal(request.ShipmentDate, response.ShipmentDate);
        Assert.Equal("Pending", response.Status);
        Assert.Single(response.Packages);
        Assert.Single(response.DeliveryLegs);
        
        _mockUnitOfWork.Verify(u => u.Shipments.AddAsync(It.IsAny<Shipment>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateShipment_WithInvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateShipmentV2Request
        {
            ShipmentDate = DateTime.UtcNow,
            Packages = new List<PackageV2Request>(), // Empty packages should cause validation error
            DeliveryLegs = new List<DeliveryLegV2Request>()
        };

        // Act
        var result = await _controller.CreateShipment(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var errorResponse = Assert.IsType<ErrorV2Response>(badRequestResult.Value);
        Assert.Contains("Shipment must have at least one package", errorResponse.Error);
    }

    [Fact]
    public async Task GetShipment_WithValidId_ReturnsShipment()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var shipment = CreateTestShipment(shipmentId);
        
        _mockShipmentRepository.Setup(r => r.GetByIdAsync(It.IsAny<ShipmentId>()))
            .ReturnsAsync(shipment);

        // Act
        var result = await _controller.GetShipment(shipmentId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ShipmentV2Response>(okResult.Value);
        Assert.Equal(shipmentId, response.ShipmentId);
        Assert.Equal("Pending", response.Status);
    }

    [Fact]
    public async Task GetShipment_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        
        _mockShipmentRepository.Setup(r => r.GetByIdAsync(It.IsAny<ShipmentId>()))
            .ReturnsAsync((Shipment?)null);

        // Act
        var result = await _controller.GetShipment(shipmentId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var errorResponse = Assert.IsType<ErrorV2Response>(notFoundResult.Value);
        Assert.Equal("Shipment not found", errorResponse.Error);
    }

    [Fact]
    public async Task AttachSensor_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var packageId = Guid.NewGuid();
        var sensorId = Guid.NewGuid();
        var shipment = CreateTestShipment(shipmentId);
        
        _mockShipmentRepository.Setup(r => r.GetByIdAsync(It.IsAny<ShipmentId>()))
            .ReturnsAsync(shipment);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new AttachSensorV2Request
        {
            PackageId = packageId,
            SensorId = sensorId
        };

        // Act
        var result = await _controller.AttachSensor(shipmentId, packageId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value as dynamic;
        Assert.Equal("Sensor attached successfully", response.Message);
        
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AttachSensor_WithNonExistentPackage_ReturnsNotFound()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var packageId = Guid.NewGuid();
        var shipment = CreateTestShipment(shipmentId);
        
        _mockShipmentRepository.Setup(r => r.GetByIdAsync(It.IsAny<ShipmentId>()))
            .ReturnsAsync(shipment);

        var request = new AttachSensorV2Request
        {
            PackageId = packageId,
            SensorId = Guid.NewGuid()
        };

        // Act
        var result = await _controller.AttachSensor(shipmentId, packageId, request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var errorResponse = Assert.IsType<ErrorV2Response>(notFoundResult.Value);
        Assert.Equal("Package not found in shipment", errorResponse.Error);
    }

    [Fact]
    public async Task ConnectGateway_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var deliveryLegId = Guid.NewGuid();
        var gatewayId = Guid.NewGuid();
        var shipment = CreateTestShipment(shipmentId);
        
        _mockShipmentRepository.Setup(r => r.GetByIdAsync(It.IsAny<ShipmentId>()))
            .ReturnsAsync(shipment);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new ConnectGatewayV2Request
        {
            DeliveryLegId = deliveryLegId,
            GatewayId = gatewayId
        };

        // Act
        var result = await _controller.ConnectGateway(shipmentId, deliveryLegId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value as dynamic;
        Assert.Equal("Gateway connected successfully", response.Message);
        
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task StartDeliveryLeg_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var deliveryLegId = Guid.NewGuid();
        var shipment = CreateTestShipment(shipmentId);
        
        _mockShipmentRepository.Setup(r => r.GetByIdAsync(It.IsAny<ShipmentId>()))
            .ReturnsAsync(shipment);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _controller.StartDeliveryLeg(shipmentId, deliveryLegId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value as dynamic;
        Assert.Equal("Delivery leg started successfully", response.Message);
        
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CompleteDeliveryLeg_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var deliveryLegId = Guid.NewGuid();
        var shipment = CreateTestShipment(shipmentId);
        
        _mockShipmentRepository.Setup(r => r.GetByIdAsync(It.IsAny<ShipmentId>()))
            .ReturnsAsync(shipment);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _controller.CompleteDeliveryLeg(shipmentId, deliveryLegId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value as dynamic;
        Assert.Equal("Delivery leg completed successfully", response.Message);
        
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetExpectedSensors_WithValidRequest_ReturnsSensorList()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var deliveryLegId = Guid.NewGuid();
        var shipment = CreateTestShipment(shipmentId);
        
        _mockShipmentRepository.Setup(r => r.GetByIdAsync(It.IsAny<ShipmentId>()))
            .ReturnsAsync(shipment);

        // Act
        var result = await _controller.GetExpectedSensors(shipmentId, deliveryLegId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GatewaySensorListV2Response>(okResult.Value);
        Assert.Equal(deliveryLegId, response.DeliveryLegId);
        Assert.Empty(response.ExpectedSensorIds); // No sensors attached in test shipment
    }

    private static Shipment CreateTestShipment(Guid shipmentId)
    {
        var sender = new Address("123 Main St", "New York", "10001", "USA");
        var recipient = new Address("456 Oak Ave", "Los Angeles", "90210", "USA");
        var package = new Package(PackageId.NewId(), sender, recipient);

        var startAddress = new Address("123 Main St", "New York", "10001", "USA");
        var endAddress = new Address("456 Oak Ave", "Los Angeles", "90210", "USA");
        var deliveryLeg = new DeliveryLeg(startAddress, endAddress);

        return new Shipment(
            new ShipmentId(shipmentId),
            DateTime.UtcNow,
            new List<Package> { package },
            new List<DeliveryLeg> { deliveryLeg });
    }
}
