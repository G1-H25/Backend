using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using GpsApp.Controllers.V2;
using GpsApp.Domain.Aggregates;
using GpsApp.Domain.ValueObjects;
using GpsApp.Infrastructure.Data.Repositories;
using GpsApp.DTO.V2;

namespace GpsApp.Tests.Unit.Controllers.V2;

/// <summary>
/// Unit tests for V2 GatewayController
/// Tests follow Arrange-Act-Assert pattern
/// </summary>
public class GatewayControllerTests
{
    private readonly Mock<IShipmentRepository> _mockShipmentRepository;
    private readonly Mock<IPackageMeasurementRepository> _mockPackageMeasurementRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly GatewayController _controller;

    public GatewayControllerTests()
    {
        _mockShipmentRepository = new Mock<IShipmentRepository>();
        _mockPackageMeasurementRepository = new Mock<IPackageMeasurementRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        
        _controller = new GatewayController(
            _mockShipmentRepository.Object,
            _mockPackageMeasurementRepository.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task GetExpectedSensors_WithValidGateway_ReturnsSensorList()
    {
        // Arrange
        var gatewayId = Guid.NewGuid();
    var deliveryLegId = Guid.NewGuid();
    var sensorId = Guid.NewGuid();
    var shipment = CreateTestShipmentWithSensors(gatewayId, deliveryLegId, sensorId);
        
        SetupControllerUser(gatewayId.ToString());
        _mockShipmentRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Shipment> { shipment });

        // Act
        var result = await _controller.GetExpectedSensors(deliveryLegId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GatewaySensorListV2Response>(okResult.Value);
        Assert.Equal(gatewayId, response.GatewayId);
        Assert.Equal(deliveryLegId, response.DeliveryLegId);
        Assert.Single(response.ExpectedSensorIds);
    }

    [Fact]
    public async Task GetExpectedSensors_WithUnauthorizedGateway_ReturnsForbid()
    {
        // Arrange
        var gatewayId = Guid.NewGuid();
    var deliveryLegId = Guid.NewGuid();
    var sensorId = Guid.NewGuid();
    var shipment = CreateTestShipmentWithSensors(gatewayId, deliveryLegId, sensorId);
        
        SetupControllerUser("different-gateway-id");
        _mockShipmentRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Shipment> { shipment });


        // Act
        var result = await _controller.GetExpectedSensors(deliveryLegId);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task VerifySensorPresence_WithValidRequest_ReturnsSensorStatus()
    {
        // Arrange
        var gatewayId = Guid.NewGuid();
        var deliveryLegId = Guid.NewGuid();
    var sensorId = Guid.NewGuid();
    var shipment = CreateTestShipmentWithSensors(gatewayId, deliveryLegId, sensorId);
        
        SetupControllerUser(gatewayId.ToString());
        _mockShipmentRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Shipment> { shipment });
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var request = new SensorPresenceV2Request
        {
            DeliveryLegId = deliveryLegId,
            PresentSensorIds = new List<Guid> { sensorId }
        };

    // Act
    var result = await _controller.VerifySensorPresence(deliveryLegId, request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<GatewaySensorListV2Response>(okResult.Value);
        Assert.Equal(gatewayId, response.GatewayId);
        Assert.Equal(deliveryLegId, response.DeliveryLegId);
        Assert.Single(response.PresentSensorIds);
        
    _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordMeasurements_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var gatewayId = Guid.NewGuid();
        var deliveryLegId = Guid.NewGuid();
        var sensorId = Guid.NewGuid();
    var shipment = CreateTestShipmentWithSensors(gatewayId, deliveryLegId, sensorId);
        var packageMeasurement = CreateTestPackageMeasurement();
        
        SetupControllerUser(gatewayId.ToString());
        _mockShipmentRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Shipment> { shipment });
        _mockPackageMeasurementRepository.Setup(r => r.GetActiveByPackageAndDeliveryLegAsync(
            It.IsAny<PackageId>(), It.IsAny<DeliveryLegId>()))
            .ReturnsAsync(packageMeasurement);
    _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var request = new MeasurementBatchV2Request
        {
            DeliveryLegId = deliveryLegId,
            Readings = new List<MeasurementReadingV2Request>
            {
                new()
                {
                    SensorId = sensorId,
                    Timestamp = DateTime.UtcNow,
                    Temperature = 5.5m,
                    Humidity = 45.0m,
                    Latitude = 40.7128m,
                    Longitude = -74.0060m
                }
            }
        };

    // Act
    var result = await _controller.RecordMeasurements(deliveryLegId, request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value as dynamic;
        Assert.Equal("Measurements recorded successfully", response.Message);
        Assert.Equal(1, response.ReadingCount);
        
    _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordMeasurements_WithUnauthorizedGateway_ReturnsForbid()
    {
        // Arrange
        var gatewayId = Guid.NewGuid();
        var deliveryLegId = Guid.NewGuid();
    var sensorId = Guid.NewGuid();
    var shipment = CreateTestShipmentWithSensors(gatewayId, deliveryLegId, sensorId);
        
        SetupControllerUser("different-gateway-id");
        _mockShipmentRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Shipment> { shipment });

        var request = new MeasurementBatchV2Request
        {
            DeliveryLegId = deliveryLegId,
            Readings = new List<MeasurementReadingV2Request>()
        };

        // Act
        var result = await _controller.RecordMeasurements(deliveryLegId, request);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetMeasurementSummary_WithValidRequest_ReturnsSummary()
    {
        // Arrange
        var gatewayId = Guid.NewGuid();
        var deliveryLegId = Guid.NewGuid();
        var packageId = Guid.NewGuid();
    var sensorId = Guid.NewGuid();
    var shipment = CreateTestShipmentWithSensors(gatewayId, deliveryLegId, sensorId);
        var packageMeasurement = CreateTestPackageMeasurement();
        
        SetupControllerUser(gatewayId.ToString());
        _mockShipmentRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Shipment> { shipment });
        _mockPackageMeasurementRepository.Setup(r => r.GetActiveByPackageAndDeliveryLegAsync(
            It.IsAny<PackageId>(), It.IsAny<DeliveryLegId>()))
            .ReturnsAsync(packageMeasurement);

        // Act
        var result = await _controller.GetMeasurementSummary(deliveryLegId, packageId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<MeasurementSummaryV2Response>(okResult.Value);
        Assert.Equal(packageId, response.PackageId);
        Assert.Equal(deliveryLegId, response.DeliveryLegId);
        Assert.Equal(0, response.ReadingCount);
    }

    [Fact]
    public async Task GetMeasurementSummary_WithNoActiveSession_ReturnsNotFound()
    {
        // Arrange
        var gatewayId = Guid.NewGuid();
        var deliveryLegId = Guid.NewGuid();
        var packageId = Guid.NewGuid();
    var sensorId = Guid.NewGuid();
    var shipment = CreateTestShipmentWithSensors(gatewayId, deliveryLegId, sensorId);
        
        SetupControllerUser(gatewayId.ToString());
        _mockShipmentRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Shipment> { shipment });
        _mockPackageMeasurementRepository.Setup(r => r.GetActiveByPackageAndDeliveryLegAsync(
            It.IsAny<PackageId>(), It.IsAny<DeliveryLegId>()))
            .ReturnsAsync((PackageMeasurement?)null);

        // Act
        var result = await _controller.GetMeasurementSummary(deliveryLegId, packageId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var errorResponse = Assert.IsType<ErrorV2Response>(notFoundResult.Value);
        Assert.Equal("No active measurement session found for this package", errorResponse.Error);
    }

    [Fact]
    public async Task GetRecentMeasurements_WithValidRequest_ReturnsMeasurements()
    {
        // Arrange
        var gatewayId = Guid.NewGuid();
        var deliveryLegId = Guid.NewGuid();
        var packageId = Guid.NewGuid();
        var shipment = CreateTestShipmentWithSensors();
        var packageMeasurement = CreateTestPackageMeasurement();
        
        SetupControllerUser(gatewayId.ToString());
        _mockShipmentRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Shipment> { shipment });
        _mockPackageMeasurementRepository.Setup(r => r.GetActiveByPackageAndDeliveryLegAsync(
            It.IsAny<PackageId>(), It.IsAny<DeliveryLegId>()))
            .ReturnsAsync(packageMeasurement);

        // Act
        var result = await _controller.GetRecentMeasurements(deliveryLegId, packageId, 50);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<List<MeasurementReadingV2Response>>(okResult.Value);
        Assert.Empty(response); // No readings in test measurement
    }

    private void SetupControllerUser(string gatewayId)
    {
        var claims = new List<Claim>
        {
            new("GatewayId", gatewayId),
            new("UserId", "1")
        };
        
        var identity = new ClaimsIdentity(claims, "Basic");
        var principal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }

    private static Shipment CreateTestShipmentWithSensors()
    {
        var shipmentId = Guid.NewGuid();
        var packageId = Guid.NewGuid();
        var sensorId = Guid.NewGuid();
        var deliveryLegId = Guid.NewGuid();
        var gatewayId = Guid.NewGuid();

        var sender = new Address("123 Main St", "New York", "10001", "USA");
        var recipient = new Address("456 Oak Ave", "Los Angeles", "90210", "USA");
        var package = new Package(PackageId.NewId(), sender, recipient);
        package.AttachSensor(new SensorId(sensorId));

        var startAddress = new Address("123 Main St", "New York", "10001", "USA");
        var endAddress = new Address("456 Oak Ave", "Los Angeles", "90210", "USA");
        var deliveryLeg = new DeliveryLeg(startAddress, endAddress);
        deliveryLeg = deliveryLeg.AssignGateway(new GatewayId(gatewayId));

        var shipment = new Shipment(
            new ShipmentId(shipmentId),
            DateTime.UtcNow,
            new List<Package> { package },
            new List<DeliveryLeg> { deliveryLeg });

        return shipment;
    }

    private static Shipment CreateTestShipmentWithSensors(Guid gatewayId, Guid deliveryLegId, Guid sensorId)
    {
        var shipmentId = Guid.NewGuid();
        var packageId = Guid.NewGuid();

        var sender = new Address("123 Main St", "New York", "10001", "USA");
        var recipient = new Address("456 Oak Ave", "Los Angeles", "90210", "USA");
        var package = new Package(PackageId.NewId(), sender, recipient);
        package.AttachSensor(new SensorId(sensorId));

        var startAddress = new Address("123 Main St", "New York", "10001", "USA");
        var endAddress = new Address("456 Oak Ave", "Los Angeles", "90210", "USA");

        var deliveryLeg = new DeliveryLeg(new DeliveryLegId(deliveryLegId), startAddress, endAddress, new GatewayId(gatewayId), DeliveryLegStatus.Ready);

        var shipment = new Shipment(
            new ShipmentId(shipmentId),
            DateTime.UtcNow,
            new List<Package> { package },
            new List<DeliveryLeg> { deliveryLeg });

        return shipment;
    }

    private static PackageMeasurement CreateTestPackageMeasurement()
    {
        var packageId = Guid.NewGuid();
        var deliveryLegId = Guid.NewGuid();
        var shipmentId = Guid.NewGuid();

        var startAddress = new Address("123 Main St", "New York", "10001", "USA");
        var endAddress = new Address("456 Oak Ave", "Los Angeles", "90210", "USA");
        var deliveryLeg = new DeliveryLeg(startAddress, endAddress);

        // Ensure the delivery leg has a gateway assigned since PackageMeasurement ctor
        // raises MeasurementSessionStartedEvent which expects a non-null GatewayId
        var gatewayId = GatewayId.NewId();
        deliveryLeg = deliveryLeg.AssignGateway(gatewayId);

        var expectedTempRange = new ExpectedRange<Temperature>(new Temperature(2.0m), new Temperature(8.0m));
        var expectedHumRange = new ExpectedRange<Humidity>(new Humidity(30.0m), new Humidity(70.0m));

        return new PackageMeasurement(
            PackageMeasurementId.NewId(),
            new PackageId(packageId),
            deliveryLeg,
            new ShipmentId(shipmentId),
            expectedTempRange,
            expectedHumRange);
    }
}
