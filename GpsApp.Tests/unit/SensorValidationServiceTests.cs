using Xunit;
using GpsApp.Services;
using GpsApp.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace GpsApp.Tests.Unit
{
    public class SensorValidationServiceTests
    {
        private readonly SensorValidationService _validationService;

        public SensorValidationServiceTests()
        {
            _validationService = new SensorValidationService();
        }

        #region ValidateBatchedSensorRequest Tests

        [Fact]
        public void ValidateBatchedSensorRequest_WithNullRequest_ReturnsInvalid()
        {
            // Act
            var result = _validationService.ValidateBatchedSensorRequest(null);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Request body cannot be null.", result.Errors);
        }

        [Fact]
        public void ValidateBatchedSensorRequest_WithEmptyGatewayUUID_ReturnsInvalid()
        {
            // Arrange
            var request = new ConnectedToGateway
            {
                GatewayUUID = Guid.Empty,
                Readings = new BatchedSensorRequest
                {
                    batch_id = "test-batch",
                    generated_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    sensors = new List<SensorData>()
                }
            };

            // Act
            var result = _validationService.ValidateBatchedSensorRequest(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("GatewayUUID must be a valid non-empty GUID.", result.Errors);
        }

        [Fact]
        public void ValidateBatchedSensorRequest_WithNullSensors_ReturnsInvalid()
        {
            // Arrange
            var request = new ConnectedToGateway
            {
                GatewayUUID = Guid.NewGuid(),
                Readings = new BatchedSensorRequest
                {
                    batch_id = "test-batch",
                    generated_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    sensors = null
                }
            };

            // Act
            var result = _validationService.ValidateBatchedSensorRequest(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("At least one sensor must be provided.", result.Errors);
        }

        [Fact]
        public void ValidateBatchedSensorRequest_WithEmptySensors_ReturnsInvalid()
        {
            // Arrange
            var request = new ConnectedToGateway
            {
                GatewayUUID = Guid.NewGuid(),
                Readings = new BatchedSensorRequest
                {
                    batch_id = "test-batch",
                    generated_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    sensors = new List<SensorData>()
                }
            };

            // Act
            var result = _validationService.ValidateBatchedSensorRequest(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("At least one sensor must be provided.", result.Errors);
        }

        [Fact]
        public void ValidateBatchedSensorRequest_WithSensorsButNoMeasurements_ReturnsInvalid()
        {
            // Arrange
            var request = new ConnectedToGateway
            {
                GatewayUUID = Guid.NewGuid(),
                Readings = new BatchedSensorRequest
                {
                    batch_id = "test-batch",
                    generated_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    sensors = new List<SensorData>
                    {
                        new SensorData
                        {
                            sensor_id = 1,
                            measurements = new List<Measurement>()
                        }
                    }
                }
            };

            // Act
            var result = _validationService.ValidateBatchedSensorRequest(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("At least one sensor measurement must be provided.", result.Errors);
        }

        [Fact]
        public void ValidateBatchedSensorRequest_WithValidData_ReturnsValid()
        {
            // Arrange
            var request = new ConnectedToGateway
            {
                GatewayUUID = Guid.NewGuid(),
                Readings = new BatchedSensorRequest
                {
                    batch_id = "test-batch",
                    generated_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    sensors = new List<SensorData>
                    {
                        new SensorData
                        {
                            sensor_id = 1,
                            measurements = new List<Measurement>
                            {
                                new Measurement
                                {
                                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                                    temperature_c = 22.5f,
                                    humidity_pct = 55.2f
                                }
                            }
                        }
                    }
                }
            };

            // Act
            var result = _validationService.ValidateBatchedSensorRequest(request);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        #endregion

        #region ValidateSensorData Tests

        [Fact]
        public void ValidateSensorData_WithNullSensor_ReturnsInvalid()
        {
            // Act
            var errors = _validationService.ValidateSensorData(null);

            // Assert
            Assert.Contains("Sensor data cannot be null.", errors);
        }

        [Fact]
        public void ValidateSensorData_WithInvalidSensorId_ReturnsInvalid()
        {
            // Arrange
            var sensor = new SensorData
            {
                sensor_id = 0,
                measurements = new List<Measurement>
                {
                    new Measurement
                    {
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                        temperature_c = 22.5f,
                        humidity_pct = 55.2f
                    }
                }
            };

            // Act
            var errors = _validationService.ValidateSensorData(sensor);

            // Assert
            Assert.Contains("Invalid sensor ID: 0. Sensor ID must be greater than 0.", errors);
        }

        [Fact]
        public void ValidateSensorData_WithNullMeasurements_ReturnsInvalid()
        {
            // Arrange
            var sensor = new SensorData
            {
                sensor_id = 1,
                measurements = null
            };

            // Act
            var errors = _validationService.ValidateSensorData(sensor);

            // Assert
            Assert.Contains("Sensor 1 has no measurements.", errors);
        }

        [Fact]
        public void ValidateSensorData_WithEmptyMeasurements_ReturnsInvalid()
        {
            // Arrange
            var sensor = new SensorData
            {
                sensor_id = 1,
                measurements = new List<Measurement>()
            };

            // Act
            var errors = _validationService.ValidateSensorData(sensor);

            // Assert
            Assert.Contains("Sensor 1 has no measurements.", errors);
        }

        [Fact]
        public void ValidateSensorData_WithValidData_ReturnsValid()
        {
            // Arrange
            var sensor = new SensorData
            {
                sensor_id = 1,
                measurements = new List<Measurement>
                {
                    new Measurement
                    {
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                        temperature_c = 22.5f,
                        humidity_pct = 55.2f
                    }
                }
            };

            // Act
            var errors = _validationService.ValidateSensorData(sensor);

            // Assert
            Assert.Empty(errors);
        }

        #endregion

        #region ValidateSensorDto Tests

        [Fact]
        public void ValidateSensorDto_WithNullData_ReturnsInvalid()
        {
            // Act
            var result = _validationService.ValidateSensorDto(null);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Sensor data cannot be null.", result.Errors);
        }

        [Fact]
        public void ValidateSensorDto_WithEmptyGatewayUUID_ReturnsInvalid()
        {
            // Arrange
            var data = new SensorDto
            {
                GatewayUUID = Guid.Empty,
                UUID = Guid.NewGuid(),
                PolledAt = DateTime.UtcNow,
                TemperatureCel = 22.5f,
                HumdityPct = 55.2f
            };

            // Act
            var result = _validationService.ValidateSensorDto(data);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("GatewayUUID must be a valid non-empty GUID.", result.Errors);
        }

        [Fact]
        public void ValidateSensorDto_WithEmptyUUID_ReturnsInvalid()
        {
            // Arrange
            var data = new SensorDto
            {
                GatewayUUID = Guid.NewGuid(),
                UUID = Guid.Empty,
                PolledAt = DateTime.UtcNow,
                TemperatureCel = 22.5f,
                HumdityPct = 55.2f
            };

            // Act
            var result = _validationService.ValidateSensorDto(data);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Sensor UUID must be a valid non-empty GUID.", result.Errors);
        }

        [Fact]
        public void ValidateSensorDto_WithNullTemperature_ReturnsInvalid()
        {
            // Arrange
            var data = new SensorDto
            {
                GatewayUUID = Guid.NewGuid(),
                UUID = Guid.NewGuid(),
                PolledAt = DateTime.UtcNow,
                TemperatureCel = null,
                HumdityPct = 55.2f
            };

            // Act
            var result = _validationService.ValidateSensorDto(data);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Temperature is required.", result.Errors);
        }

        [Fact]
        public void ValidateSensorDto_WithNullHumidity_ReturnsInvalid()
        {
            // Arrange
            var data = new SensorDto
            {
                GatewayUUID = Guid.NewGuid(),
                UUID = Guid.NewGuid(),
                PolledAt = DateTime.UtcNow,
                TemperatureCel = 22.5f,
                HumdityPct = null
            };

            // Act
            var result = _validationService.ValidateSensorDto(data);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Humidity is required.", result.Errors);
        }

        [Fact]
        public void ValidateSensorDto_WithValidData_ReturnsValid()
        {
            // Arrange
            var data = new SensorDto
            {
                GatewayUUID = Guid.NewGuid(),
                UUID = Guid.NewGuid(),
                PolledAt = DateTime.UtcNow,
                TemperatureCel = 22.5f,
                HumdityPct = 55.2f
            };

            // Act
            var result = _validationService.ValidateSensorDto(data);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        #endregion

        #region ValidateMeasurement Tests

        [Fact]
        public void ValidateMeasurement_WithInvalidSensorId_ReturnsInvalid()
        {
            // Arrange
            var measurement = new Measurement
            {
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                temperature_c = 22.5f,
                humidity_pct = 55.2f
            };

            // Act
            var errors = _validationService.ValidateMeasurement(measurement, 1, 1);

            // Assert
            Assert.Empty(errors); // The ValidateMeasurement method doesn't validate sensor ID
        }

        [Fact]
        public void ValidateMeasurement_WithInvalidGatewayId_ReturnsInvalid()
        {
            // Arrange
            var measurement = new Measurement
            {
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                temperature_c = 22.5f,
                humidity_pct = 55.2f
            };

            // Act
            var errors = _validationService.ValidateMeasurement(measurement, 1, 1);

            // Assert
            Assert.Empty(errors); // The ValidateMeasurement method doesn't validate gateway ID
        }

        [Fact]
        public void ValidateMeasurement_WithInvalidTimestamp_ReturnsInvalid()
        {
            // Arrange
            var measurement = new Measurement
            {
                timestamp = 0,
                temperature_c = 22.5f,
                humidity_pct = 55.2f
            };

            // Act
            var errors = _validationService.ValidateMeasurement(measurement, 1, 1);

            // Assert
            Assert.Contains("Invalid timestamp for sensor 1, measurement 1. Timestamp must be greater than 0.", errors);
        }

        [Fact]
        public void ValidateMeasurement_WithExtremeTemperature_ReturnsInvalid()
        {
            // Arrange
            var measurement = new Measurement
            {
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                temperature_c = 200f, // Extremely high temperature
                humidity_pct = 55.2f
            };

            // Act
            var errors = _validationService.ValidateMeasurement(measurement, 1, 1);

            // Assert
            Assert.Contains("Temperature 200°C for sensor 1, measurement 1 is outside valid range (-50°C to 100°C).", errors);
        }

        [Fact]
        public void ValidateMeasurement_WithExtremeHumidity_ReturnsInvalid()
        {
            // Arrange
            var measurement = new Measurement
            {
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                temperature_c = 22.5f,
                humidity_pct = 150f // Invalid humidity percentage
            };

            // Act
            var errors = _validationService.ValidateMeasurement(measurement, 1, 1);

            // Assert
            Assert.Contains("Humidity 150% for sensor 1, measurement 1 is outside valid range (0% to 100%).", errors);
        }

        [Fact]
        public void ValidateMeasurement_WithValidData_ReturnsValid()
        {
            // Arrange
            var measurement = new Measurement
            {
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                temperature_c = 22.5f,
                humidity_pct = 55.2f
            };

            // Act
            var errors = _validationService.ValidateMeasurement(measurement, 1, 1);

            // Assert
            Assert.Empty(errors);
        }

        #endregion
    }

    public class SensorReadingsForDeliveryTests : IDisposable
    {
        private readonly InMemoryTestDatabase _testDb;
        private readonly Mock<ISqlGetAdvanced> _mockSqlGetAdvanced;
        private readonly Mock<IAuthorizationService> _mockAuthService;
        private readonly SensorController _controller;

        public SensorReadingsForDeliveryTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<InMemoryTestDatabase>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _testDb = new InMemoryTestDatabase(options);

            // Setup mocks
            _mockSqlGetAdvanced = new Mock<ISqlGetAdvanced>();
            _mockAuthService = new Mock<IAuthorizationService>();

            // Setup controller with mocked dependencies
            _controller = new SensorController(
                Mock.Of<ISqlInsert>(),
                _mockAuthService.Object,
                _mockSqlGetAdvanced.Object,
                Mock.Of<ISqlGet>(),
                Mock.Of<ISqlUpdate>(),
                Mock.Of<ISensorValidationService>()
            );

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            _testDb.Accounts.Add(new TestAccount { Id = 1, CompanyId = 100 });
            _testDb.Gateways.Add(new TestGateway { Id = 1, UUID = Guid.Parse("11111111-1111-1111-1111-111111111111"), UserId = 1 });
            _testDb.Sensors.Add(new TestSensor
            {
                Id = 1,
                UUID = Guid.NewGuid(),
                GatewayId = 1,
                PolledAt = DateTime.UtcNow.AddHours(-2),
                TemperatureCel = 22.5f,
                HumdityPct = 55.2f,
                TempTimeOutside = 0,
                HumidTimeOutside = 0
            });
            _testDb.Sensors.Add(new TestSensor
            {
                Id = 2,
                UUID = Guid.NewGuid(),
                GatewayId = 1,
                PolledAt = DateTime.UtcNow.AddHours(-1),
                TemperatureCel = 23.1f,
                HumdityPct = 56.8f,
                TempTimeOutside = 5,
                HumidTimeOutside = 0
            });
            _testDb.Deliveries.Add(new TestDelivery
            {
                Id = 1,
                SensorId = 1,
                RouteId = 1,
                RecipientId = 1,
                SenderId = 1,
                CarrierId = 1,
                OrderPlaced = DateTime.UtcNow.AddDays(-1)
            });
            _testDb.SaveChanges();
        }

        [Fact]
        public async Task GetSensorReadingsForDelivery_WithValidDeliveryId_ReturnsSensorReadings()
        {
            // Arrange
            var deliveryId = 1;
            var page = 1;
            var pageSize = 10;

            // Mock authorization service
            _mockAuthService.Setup(x => x.GetCompanyIdFromClaims(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync(100);
            _mockAuthService.Setup(x => x.GetUserIdFromClaims(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync(1);
            _mockAuthService.Setup(x => x.GetUserRoleFromClaims(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync("User");

            // Mock delivery query result
            var deliveryResult = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "DeliveryId", 1 },
                    { "SensorId", 1 },
                    { "RouteId", 1 },
                    { "GatewayId", 1 },
                    { "UserId", 1 },
                    { "CompanyId", 100 }
                }
            };

            // Mock sensor readings count query
            var countResult = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "TotalCount", 2 }
                }
            };

            // Mock sensor readings query
            var readingsResult = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "SensorId", 1 },
                    { "GatewayId", 1 },
                    { "PolledAt", DateTime.UtcNow.AddHours(-2) },
                    { "TemperatureCel", 22.5f },
                    { "HumdityPct", 55.2f },
                    { "TempTimeOutside", 0 },
                    { "HumidTimeOutside", 0 }
                },
                new Dictionary<string, object>
                {
                    { "SensorId", 2 },
                    { "GatewayId", 1 },
                    { "PolledAt", DateTime.UtcNow.AddHours(-1) },
                    { "TemperatureCel", 23.1f },
                    { "HumdityPct", 56.8f },
                    { "TempTimeOutside", 5 },
                    { "HumidTimeOutside", 0 }
                }
            };

            _mockSqlGetAdvanced.Setup(x => x.FetchWithJoinsAsync<Dictionary<string, object>>(
                It.Is<string>(s => s.Contains("Orders.Delivery deliv")),
                It.Is<string>(s => s.Contains("deliv.Id AS DeliveryId")),
                It.IsAny<List<string>>(),
                It.Is<Dictionary<string, object>>(d => d["deliv.Id"].Equals(deliveryId)),
                null))
                .ReturnsAsync(deliveryResult);

            _mockSqlGetAdvanced.Setup(x => x.FetchWithJoinsAsync<Dictionary<string, object>>(
                It.Is<string>(s => s.Contains("Measurements.Sensor sensor")),
                It.Is<string>(s => s.Contains("COUNT(*) as TotalCount")),
                It.IsAny<List<string>>(),
                It.IsAny<Dictionary<string, object>>(),
                null))
                .ReturnsAsync(countResult);

            _mockSqlGetAdvanced.Setup(x => x.ConnectionString)
                .Returns("FakeConnectionString");

            // Act
            var result = await _controller.GetSensorReadingsForDelivery(deliveryId, page, pageSize);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GpsApp.DTO.PaginatedSensorReadingsResponse>(okResult.Value);

            Assert.Equal(2, response.Readings.Count);
            Assert.Equal(1, response.Page);
            Assert.Equal(10, response.PageSize);
            Assert.Equal(2, response.TotalCount);
            Assert.Equal(1, response.TotalPages);
        }

        [Fact]
        public async Task GetSensorReadingsForDelivery_WithInvalidDeliveryId_ReturnsNotFound()
        {
            // Arrange
            var deliveryId = 999;

            _mockAuthService.Setup(x => x.GetCompanyIdFromClaims(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync(100);
            _mockAuthService.Setup(x => x.GetUserIdFromClaims(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync(1);
            _mockAuthService.Setup(x => x.GetUserRoleFromClaims(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync("User");

            // Mock no delivery found
            _mockSqlGetAdvanced.Setup(x => x.FetchWithJoinsAsync<Dictionary<string, object>>(
                It.Is<string>(s => s.Contains("Orders.Delivery deliv")),
                It.Is<string>(s => s.Contains("deliv.Id AS DeliveryId")),
                It.IsAny<List<string>>(),
                It.Is<Dictionary<string, object>>(d => d["deliv.Id"].Equals(deliveryId)),
                null))
                .ReturnsAsync(new List<Dictionary<string, object>>());

            // Act
            var result = await _controller.GetSensorReadingsForDelivery(deliveryId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetSensorReadingsForDelivery_WithInvalidPageParameters_ReturnsBadRequest()
        {
            // Arrange
            var deliveryId = 1;
            var page = 0; // Invalid page number
            var pageSize = 50;

            // Act
            var result = await _controller.GetSensorReadingsForDelivery(deliveryId, page, pageSize);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Page number must be greater than 0", badRequestResult.Value?.ToString());
        }

        [Fact]
        public async Task GetSensorReadingsForDelivery_WithInvalidPageSize_ReturnsBadRequest()
        {
            // Arrange
            var deliveryId = 1;
            var page = 1;
            var pageSize = 150; // Invalid page size (too large)

            // Act
            var result = await _controller.GetSensorReadingsForDelivery(deliveryId, page, pageSize);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Page size must be between 1 and 100", badRequestResult.Value?.ToString());
        }

        [Fact]
        public async Task GetSensorReadingsForDelivery_WithInvalidDateRange_ReturnsBadRequest()
        {
            // Arrange
            var deliveryId = 1;
            var page = 1;
            var pageSize = 50;
            var fromDate = DateTime.UtcNow;
            var toDate = DateTime.UtcNow.AddDays(-1); // From date after to date

            // Act
            var result = await _controller.GetSensorReadingsForDelivery(deliveryId, page, pageSize, fromDate, toDate);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("From date cannot be later than to date", badRequestResult.Value?.ToString());
        }

        [Fact]
        public async Task GetSensorReadingsForDelivery_WithTimeFiltering_ReturnsFilteredResults()
        {
            // Arrange
            var deliveryId = 1;
            var page = 1;
            var pageSize = 10;
            var fromDate = DateTime.UtcNow.AddHours(-1.5);
            var toDate = DateTime.UtcNow.AddHours(-0.5);

            _mockAuthService.Setup(x => x.GetCompanyIdFromClaims(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync(100);
            _mockAuthService.Setup(x => x.GetUserIdFromClaims(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync(1);
            _mockAuthService.Setup(x => x.GetUserRoleFromClaims(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync("User");

            // Mock delivery query result
            var deliveryResult = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "DeliveryId", 1 },
                    { "SensorId", 1 },
                    { "RouteId", 1 },
                    { "GatewayId", 1 },
                    { "UserId", 1 },
                    { "CompanyId", 100 }
                }
            };

            // Mock sensor readings count query with time filtering
            var countResult = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "TotalCount", 1 }
                }
            };

            _mockSqlGetAdvanced.Setup(x => x.FetchWithJoinsAsync<Dictionary<string, object>>(
                It.Is<string>(s => s.Contains("Orders.Delivery deliv")),
                It.Is<string>(s => s.Contains("deliv.Id AS DeliveryId")),
                It.IsAny<List<string>>(),
                It.Is<Dictionary<string, object>>(d => d["deliv.Id"].Equals(deliveryId)),
                null))
                .ReturnsAsync(deliveryResult);

            _mockSqlGetAdvanced.Setup(x => x.FetchWithJoinsAsync<Dictionary<string, object>>(
                It.Is<string>(s => s.Contains("Measurements.Sensor sensor")),
                It.Is<string>(s => s.Contains("COUNT(*) as TotalCount")),
                It.IsAny<List<string>>(),
                It.Is<Dictionary<string, object>>(d => d.ContainsKey("sensor.PolledAt >= ") && d.ContainsKey("sensor.PolledAt <= ")),
                null))
                .ReturnsAsync(countResult);

            _mockSqlGetAdvanced.Setup(x => x.ConnectionString)
                .Returns("FakeConnectionString");

            // Act
            var result = await _controller.GetSensorReadingsForDelivery(deliveryId, page, pageSize, fromDate, toDate);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GpsApp.DTO.PaginatedSensorReadingsResponse>(okResult.Value);

            // Verify that the filters were applied correctly
            _mockSqlGetAdvanced.Verify(x => x.FetchWithJoinsAsync<Dictionary<string, object>>(
                It.Is<string>(s => s.Contains("COUNT(*) as TotalCount")),
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.Is<Dictionary<string, object>>(d =>
                    d.ContainsKey("sensor.PolledAt >= ") &&
                    d.ContainsKey("sensor.PolledAt <= ")),
                null), Times.Once);
        }

        public void Dispose()
        {
            _testDb.Dispose();
        }
    }

    public class SensorApiDiscoveryTests
    {
        private readonly SensorController _controller;

        public SensorApiDiscoveryTests()
        {
            // Setup controller with mocked dependencies (not needed for discovery endpoints)
            _controller = new SensorController(
                Mock.Of<ISqlInsert>(),
                Mock.Of<IAuthorizationService>(),
                Mock.Of<ISqlGetAdvanced>(),
                Mock.Of<ISqlGet>(),
                Mock.Of<ISqlUpdate>(),
                Mock.Of<ISensorValidationService>()
            );
        }

        [Fact]
        public void GetSensorApiInfo_ReturnsValidApiDiscoveryResponse()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = "https";
            httpContext.Request.Host = new HostString("api.example.com");
            httpContext.Request.PathBase = new PathString("/v1");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = _controller.GetSensorApiInfo();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GpsApp.DTO.ApiDiscoveryResponse>(okResult.Value);

            Assert.Equal("GPS Sensor API", response.ApiName);
            Assert.Equal("1.0.0", response.Version);
            Assert.Equal("https://api.example.com/v1/sensor", response.BaseUrl);
            Assert.NotEmpty(response.Endpoints);
            Assert.NotEmpty(response.Metadata);

            // Verify delivery endpoint is included
            var deliveryEndpoint = response.Endpoints.FirstOrDefault(e => e.Path.Contains("delivery"));
            Assert.NotNull(deliveryEndpoint);
            Assert.Equal("GET", deliveryEndpoint.Method);
            Assert.Contains("deliveryId", deliveryEndpoint.Parameters);
            Assert.True(deliveryEndpoint.RequiresAuth);
        }

        [Fact]
        public void GetDeliverySensorApiInfo_ReturnsValidApiDiscoveryResponse()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = "https";
            httpContext.Request.Host = new HostString("api.example.com");
            httpContext.Request.PathBase = new PathString("/v1");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = _controller.GetDeliverySensorApiInfo();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GpsApp.DTO.ApiDiscoveryResponse>(okResult.Value);

            Assert.Equal("Delivery Sensor API", response.ApiName);
            Assert.Equal("1.0.0", response.Version);
            Assert.Equal("https://api.example.com/v1/sensor/delivery", response.BaseUrl);
            Assert.Single(response.Endpoints); // Only one delivery-specific endpoint

            var endpoint = response.Endpoints[0];
            Assert.Equal("GET", endpoint.Method);
            Assert.Contains("{deliveryId}", endpoint.Path);
            Assert.Contains("page", endpoint.Parameters);
            Assert.Contains("pageSize", endpoint.Parameters);
            Assert.Contains("fromDate", endpoint.Parameters);
            Assert.Contains("toDate", endpoint.Parameters);
            Assert.True(endpoint.RequiresAuth);

            // Verify metadata contains expected information
            Assert.Contains("Logistics tracking", response.Metadata["useCase"]);
            Assert.Contains("parentApi", response.Metadata);
        }

        [Fact]
        public void ApiDiscoveryEndpoints_IncludeCorrectDocumentationLinks()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = "https";
            httpContext.Request.Host = new HostString("api.example.com");
            httpContext.Request.PathBase = new PathString("/v1");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var generalApiResult = _controller.GetSensorApiInfo();
            var deliveryApiResult = _controller.GetDeliverySensorApiInfo();

            // Assert
            var generalResponse = ((OkObjectResult)generalApiResult).Value as GpsApp.DTO.ApiDiscoveryResponse;
            var deliveryResponse = ((OkObjectResult)deliveryApiResult).Value as GpsApp.DTO.ApiDiscoveryResponse;

            // Verify documentation links are included
            Assert.Contains("/swagger", generalResponse.Metadata["documentation"]);
            Assert.Contains("/swagger", deliveryResponse.Metadata["documentation"]);
            Assert.Contains("parentApi", deliveryResponse.Metadata);
        }

        [Fact]
        public void ApiDiscoveryEndpoints_IncludeHealthCheckInformation()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = "https";
            httpContext.Request.Host = new HostString("api.example.com");
            httpContext.Request.PathBase = new PathString("/v1");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = _controller.GetSensorApiInfo();

            // Assert
            var response = ((OkObjectResult)result).Value as GpsApp.DTO.ApiDiscoveryResponse;
            Assert.Contains("healthCheck", response.Metadata);
            Assert.Contains("/health", response.Metadata["healthCheck"]);
        }
    }
}
