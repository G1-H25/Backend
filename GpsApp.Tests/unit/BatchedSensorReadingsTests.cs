/*
using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using GpsApp.Controllers;
using GpsApp.DTO;
using GpsApp.Tests.Unit;
using GpsApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Import the SensorReading type from the main project
using SensorReading = GpsApp.DTO.SensorReading;


namespace GpsApp.Tests.Unit
{
    public class BatchedSensorReadingsTests : IDisposable
    {
        private readonly InMemoryTestDatabase _testDb;
        private readonly Mock<ISqlGet> _mockSqlGet;
        private readonly Mock<ISqlGetAdvanced> _mockSqlGetAdvanced;
        private readonly Mock<IAuthorizationService> _mockAuthService;
        private readonly Mock<ISqlInsert> _mockSqlInsert;
        private readonly Mock<ISqlUpdate> _mockSqlUpdate;
        private readonly Mock<ISensorValidationService> _mockValidationService;
        private readonly SensorController _controller;

        public BatchedSensorReadingsTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<InMemoryTestDatabase>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _testDb = new InMemoryTestDatabase(options);

            // Setup mocks
            _mockSqlGet = new Mock<ISqlGet>();
            _mockSqlGetAdvanced = new Mock<ISqlGetAdvanced>();
            _mockAuthService = new Mock<IAuthorizationService>();
            _mockSqlInsert = new Mock<ISqlInsert>();
            _mockSqlUpdate = new Mock<ISqlUpdate>();
            _mockValidationService = new Mock<ISensorValidationService>();

            // Setup controller
            _controller = new SensorController(
                _mockSqlInsert.Object,
                _mockAuthService.Object,
                _mockSqlGetAdvanced.Object,
                _mockSqlGet.Object,
                _mockSqlUpdate.Object,
                _mockValidationService.Object
            );

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            _testDb.Accounts.Add(new TestAccount { Id = 1, CompanyId = 100 });
            _testDb.Gateways.Add(new TestGateway { Id = 1, UUID = Guid.Parse("11111111-1111-1111-1111-111111111111"), UserId = 1 });
            _testDb.SaveChanges();
        }

        [Fact]
        public async Task PostBatchedSensorData_WithValidData_ProcessesAllReadingsSuccessfully()
        {
            // Arrange - Create test data that bypasses validation via mocks
            var gatewayUuid = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var batchedRequest = CreateValidBatchedRequest(gatewayUuid);

            // Mock validation service to always return valid (bypassing validation logic)
            SetupValidationServiceMocks(valid: true);

            // Mock gateway lookup
            SetupGatewayLookupMock(gatewayUuid, gatewayId: 1);

            // Mock no existing sensor readings (new sensor data)
            SetupNoExistingSensorReadingsMock();

            // Mock no existing sensor records (new records)
            SetupNoExistingSensorRecordsMock();

            // Setup database operation mocks
            SetupDatabaseMocks();

            // Act
            var result = await _controller.PostBatchedSensorData(batchedRequest);

            // Assert - Verify controller behavior
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value;

            // Verify response structure and content
            AssertResponseStructure(response);
            Assert.Equal(2, GetResponseProperty<int>(response, "ProcessedCount"));
            Assert.Equal(0, GetResponseProperty<int>(response, "SkippedCount"));
            Assert.Empty(GetResponseProperty<List<string>>(response, "Errors"));

            // Verify business logic - InsertAsync should be called for each measurement
            _mockSqlInsert.Verify(x => x.InsertAsync(
                "Measurements.Sensor",
                It.IsAny<Dictionary<string, object>>()), Times.Exactly(2));

            // Verify validation service was called
            _mockValidationService.Verify(x => x.ValidateBatchedSensorRequest(It.IsAny<ConnectedToGateway>()), Times.Once);
        }

        [Fact]
        public async Task PostBatchedSensorData_WithInvalidGatewayUUID_ReturnsBadRequest()
        {
            // Arrange
            var invalidGatewayUuid = Guid.Parse("99999999-9999-9999-9999-999999999999");
            var batchedRequest = new ConnectedToGateway
            {
                GatewayUUID = invalidGatewayUuid,
                Readings = new BatchedSensorRequest
                {
                    batch_id = "test-batch-2",
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

            // Mock gateway not found
            _mockSqlGet.Setup(x => x.FetchAsync(
                "Secrets.Gateway",
                It.IsAny<Dictionary<string, object>>(),
                null))
                .ReturnsAsync((Dictionary<string, object>?)null);

            // Mock validation service to return valid result
            _mockValidationService.Setup(x => x.ValidateBatchedSensorRequest(It.IsAny<ConnectedToGateway>()))
                .Returns(new ValidationResult { IsValid = true, Errors = new List<string>() });

            // Act
            var result = await _controller.PostBatchedSensorData(batchedRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("does not exist", badRequestResult.Value?.ToString());
        }

        [Fact]
        public async Task PostBatchedSensorData_WithEmptyReadings_ReturnsBadRequest()
        {
            // Arrange
            var batchedRequest = new ConnectedToGateway
            {
                GatewayUUID = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Readings = new BatchedSensorRequest
                {
                    batch_id = "test-batch-3",
                    generated_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    sensors = new List<SensorData>()
                }
            };

            // Mock validation service to return invalid result for empty sensors
            _mockValidationService.Setup(x => x.ValidateBatchedSensorRequest(It.IsAny<ConnectedToGateway>()))
                .Returns(new ValidationResult
                {
                    IsValid = false,
                    Errors = new List<string> { "At least one sensor must be provided." }
                });

            // Act
            var result = await _controller.PostBatchedSensorData(batchedRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("At least one sensor must be provided", badRequestResult.Value?.ToString());
        }

        [Fact]
        public async Task PostBatchedSensorData_WithInvalidSensorData_ReturnsBadRequest()
        {
            // Arrange
            var batchedRequest = new ConnectedToGateway
            {
                GatewayUUID = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Readings = new BatchedSensorRequest
                {
                    batch_id = "test-batch-4",
                    generated_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    sensors = new List<SensorData>
                    {
                        new SensorData
                        {
                            sensor_id = 0, // Invalid sensor ID
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

            // Mock validation service to return invalid result for sensor ID
            _mockValidationService.Setup(x => x.ValidateBatchedSensorRequest(It.IsAny<ConnectedToGateway>()))
                .Returns(new ValidationResult
                {
                    IsValid = false,
                    Errors = new List<string> { "Invalid sensor ID: 0. Sensor ID must be greater than 0." }
                });

            // Act
            var result = await _controller.PostBatchedSensorData(batchedRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Invalid sensor ID", badRequestResult.Value?.ToString());
        }

        [Fact]
        public async Task PostBatchedSensorData_WithMixedValidAndInvalidData_ProcessesOnlyValidReadings()
        {
            // Arrange - Create test data with bounded validation via mocks
            var gatewayUuid = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var batchedRequest = CreateMixedValidInvalidBatchedRequest(gatewayUuid);

            // Mock validation service - request level validation passes, but some sensors fail
            _mockValidationService.Setup(x => x.ValidateBatchedSensorRequest(It.IsAny<ConnectedToGateway>()))
                .Returns(new ValidationResult { IsValid = true, Errors = new List<string>() });

            // Mock sensor validation - return error for sensor ID 0, valid for sensor ID 1
            _mockValidationService.Setup(x => x.ValidateSensorData(It.Is<SensorData>(s => s.sensor_id == 0)))
                .Returns(new List<string> { "Invalid sensor ID: 0. Sensor ID must be greater than 0." });
            _mockValidationService.Setup(x => x.ValidateSensorData(It.Is<SensorData>(s => s.sensor_id == 1)))
                .Returns(new List<string>());

            // Mock measurement validation - all measurements are valid
            _mockValidationService.Setup(x => x.ValidateMeasurement(It.IsAny<Measurement>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new List<string>());

            // Mock gateway lookup
            SetupGatewayLookupMock(gatewayUuid, gatewayId: 1);

            // Mock no existing sensor readings
            SetupNoExistingSensorReadingsMock();

            // Mock no existing sensor records
            SetupNoExistingSensorRecordsMock();

            // Setup database operation mocks
            SetupDatabaseMocks();

            // Act
            var result = await _controller.PostBatchedSensorData(batchedRequest);

            // Assert - Verify controller behavior with mixed data
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value;

            AssertResponseStructure(response);
            Assert.Equal(1, GetResponseProperty<int>(response, "ProcessedCount")); // Only 1 valid reading
            Assert.Equal(1, GetResponseProperty<int>(response, "SkippedCount")); // 1 invalid reading
            Assert.Contains("Invalid sensor ID", GetResponseProperty<List<string>>(response, "Errors")[0]);

            // Verify business logic - InsertAsync should be called only once (for the valid reading)
            _mockSqlInsert.Verify(x => x.InsertAsync(
                "Measurements.Sensor",
                It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        #region Helper Methods

        private ConnectedToGateway CreateValidBatchedRequest(Guid gatewayUuid)
        {
            return new ConnectedToGateway
            {
                GatewayUUID = gatewayUuid,
                Readings = new BatchedSensorRequest
                {
                    batch_id = "test-batch-1",
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
                                    timestamp = 1698780000, 
                                    temperature_c = 22.5f, 
                                    humidity_pct = 40f 
                                }
                            }
                        },
                        new SensorData
                        {
                            sensor_id = 2,
                            measurements = new List<Measurement>
                            {
                                new Measurement
                                {
                                    timestamp = DateTimeOffset.UtcNow.AddMinutes(1).ToUnixTimeSeconds(),
                                    temperature_c = 23.1f,
                                    humidity_pct = 56.8f
                                }
                            }
                        }
                    }
                }
            };
        }

        private ConnectedToGateway CreateMixedValidInvalidBatchedRequest(Guid gatewayUuid)
        {
            return new ConnectedToGateway
            {
                GatewayUUID = gatewayUuid,
                Readings = new BatchedSensorRequest
                {
                    batch_id = "test-batch-mixed",
                    generated_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    sensors = new List<SensorData>
                    {
                        new SensorData
                        {
                            sensor_id = 1, // Valid sensor ID
                            measurements = new List<Measurement>
                            {
                                new Measurement
                                {
                                    timestamp = 1698780000,
                                    temperature_c = 22.5f,
                                    humidity_pct = 55.2f
                                }
                            }
                        },
                        new SensorData
                        {
                            sensor_id = 0, // Invalid sensor ID
                            measurements = new List<Measurement>
                            {
                                new Measurement
                                {
                                    timestamp = 1698780300,
                                    temperature_c = 23.1f,
                                    humidity_pct = 56.8f
                                }
                            }
                        }
                    }
                }
            };
        }

        private void SetupValidationServiceMocks(bool valid)
        {
            if (valid)
            {
                _mockValidationService.Setup(x => x.ValidateBatchedSensorRequest(It.IsAny<ConnectedToGateway>()))
                    .Returns(new ValidationResult { IsValid = true, Errors = new List<string>() });

                // Mock sensor validation - all sensors are valid
                _mockValidationService.Setup(x => x.ValidateSensorData(It.IsAny<SensorData>()))
                    .Returns(new List<string>());

                // Mock measurement validation - all measurements are valid
                _mockValidationService.Setup(x => x.ValidateMeasurement(It.IsAny<Measurement>(), It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(new List<string>());
            }
            else
            {
                _mockValidationService.Setup(x => x.ValidateBatchedSensorRequest(It.IsAny<ConnectedToGateway>()))
                    .Returns(new ValidationResult { IsValid = false, Errors = new List<string> { "Validation failed" } });
            }
        }

    private void SetupDatabaseMocks()
    {
        var nextSensorId = 1000;

        // Set up InsertAsync with callback for MockLiveData service chain
        _mockSqlInsert.Setup(x => x.InsertAsync(
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, object>>()))
            .Callback<string, Dictionary<string, object>>((table, dict) =>
            {
                if (dict != null && dict.ContainsKey("UUID") && dict.ContainsKey("GatewayId"))
                {
                    var insertedUuid = dict["UUID"];
                    var sensorId = nextSensorId++;

                    // Set up sensor fetch by UUID
                    _mockSqlGet.Setup(x => x.FetchAsync(
                        "Measurements.Sensor",
                        It.Is<Dictionary<string, object>>(d => 
                            d.ContainsKey("GatewayId") && 
                            d.ContainsKey("UUID") && 
                            d["UUID"].Equals(insertedUuid)),
                        null))
                        .ReturnsAsync(new Dictionary<string, object> 
                        { 
                            { "Id", sensorId }, 
                            { "GatewayId", 1 }, 
                            { "UUID", insertedUuid } 
                        });

                    // Set up sensor fetch by Id for MockLiveData
                    _mockSqlGet.Setup(x => x.FetchAsync(
                        "Measurements.Sensor",
                        It.Is<Dictionary<string, object>>(d => 
                            d.ContainsKey("Id") && 
                            Convert.ToInt32(d["Id"]) == sensorId),
                        null))
                        .ReturnsAsync(new Dictionary<string, object> 
                        { 
                            { "Id", sensorId }, 
                            { "GatewayId", 1 },
                            { "UUID", insertedUuid }
                        });
                }
            })
            .Returns(Task.CompletedTask);

        // Setup InsertAndReturnIdAsync for all the MockLiveData insert operations
        var nextId = 1;
        _mockSqlInsert.Setup(x => x.InsertAndReturnIdAsync(
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(() => nextId++);

        // Mock UpdateAsync for both sensor updates and MockLiveData updates
        _mockSqlUpdate.Setup(x => x.UpdateAsync(
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, object>>(),
            It.IsAny<Dictionary<string, object>>()))
            .Returns(Task.CompletedTask);

        // Mock gateway fetch
        _mockSqlGet
            .Setup(x => x.FetchAsync(
                "Secrets.Gateway",
                It.IsAny<Dictionary<string, object>>(),
                null))
            .ReturnsAsync(new Dictionary<string, object> { { "Id", 1 } });
    }        private void SetupGatewayLookupMock(Guid gatewayUuid, int gatewayId)
        {
            _mockSqlGet.Setup(x => x.FetchAsync(
                "Secrets.Gateway",
                It.Is<Dictionary<string, object>>(d => d["UUID"].Equals(gatewayUuid)),
                null))
                .ReturnsAsync(new Dictionary<string, object> { { "Id", gatewayId } });
        }

        private void SetupNoExistingSensorReadingsMock()
        {
            // Mock to return empty list since there are no existing readings
            // Use SensorReading type that matches the controller's map function return type
            _mockSqlGetAdvanced.Setup(x => x.FetchWithJoinsAsync<GpsApp.DTO.SensorReading>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<Func<System.Data.IDataRecord, GpsApp.DTO.SensorReading>>()))
                .ReturnsAsync(new List<GpsApp.DTO.SensorReading>());
        }

        private void SetupNoExistingSensorRecordsMock()
        {
            // No-op: Database fetch sequence for Measurements.Sensor is configured in SetupDatabaseMocks()
            // to simulate "no existing record" on first fetch and a returned Id after InsertAsync.
        }

        private void AssertResponseStructure(object response)
        {
            var responseType = response.GetType();
            Assert.NotNull(responseType.GetProperty("ProcessedCount"));
            Assert.NotNull(responseType.GetProperty("SkippedCount"));
            Assert.NotNull(responseType.GetProperty("Errors"));
            Assert.NotNull(responseType.GetProperty("Message"));
        }

        private T GetResponseProperty<T>(object response, string propertyName)
        {
            var property = response.GetType().GetProperty(propertyName);
            Assert.NotNull(property);
            var value = property.GetValue(response);
            Assert.NotNull(value);
            return (T)value;
        }

        #endregion

        public void Dispose()
        {
            _testDb.Dispose();
        }
    }
}

*/