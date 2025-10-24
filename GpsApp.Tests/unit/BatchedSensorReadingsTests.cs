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
        private readonly SensorValidationService _validationService;
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
            _validationService = new SensorValidationService();

            // Setup controller
            _controller = new SensorController(
                _mockSqlInsert.Object,
                _mockAuthService.Object,
                _mockSqlGetAdvanced.Object,
                _mockSqlGet.Object,
                _mockSqlUpdate.Object,
                _validationService
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
        public async Task PostBatchedSensorData_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var gatewayUuid = Guid.Parse("11111111-1111-1111-1111-111111111111");

            var batchedRequest = new ConnectedToGateway
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
                                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                                    temperature_c = 22.5f,
                                    humidity_pct = 55.2f
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

            // Mock gateway lookup
            _mockSqlGet.Setup(x => x.FetchAsync(
                "Secrets.Gateway",
                It.Is<Dictionary<string, object>>(d => d["UUID"].Equals(gatewayUuid)),
                null))
                .ReturnsAsync(new Dictionary<string, object> { { "Id", 1 } });

            // Mock no existing sensor readings
            _mockSqlGetAdvanced.Setup(x => x.FetchWithJoinsAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<Func<System.Data.IDataRecord, object>>()))
                .ReturnsAsync(new List<object>());

            // Mock no existing sensor records
            _mockSqlGet.Setup(x => x.FetchAsync(
                "Measurements.Sensor",
                It.IsAny<Dictionary<string, object>>(),
                null))
                .ReturnsAsync((Dictionary<string, object>?)null);

            // Act
            var result = await _controller.PostBatchedSensorData(batchedRequest);

            // Assert
            if (result is BadRequestObjectResult badRequest)
            {
                Assert.Fail($"Expected OkResult but got BadRequest: {badRequest.Value}");
            }

            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic responseValue = okResult.Value;
            Assert.Equal(2, responseValue.ProcessedCount);
            Assert.Equal(0, responseValue.SkippedCount);
            Assert.Empty(responseValue.Errors);

            // Verify that InsertAsync was called for each sensor reading
            _mockSqlInsert.Verify(x => x.InsertAsync(
                "Measurements.Sensor",
                It.IsAny<Dictionary<string, object>>()), Times.Exactly(2));
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

            // Act
            var result = await _controller.PostBatchedSensorData(batchedRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic responseValue = okResult.Value;
            Assert.Equal(0, responseValue.ProcessedCount);
            Assert.Equal(1, responseValue.SkippedCount);
            Assert.Contains("Invalid sensor ID", responseValue.Errors.ToString());
        }

        [Fact]
        public async Task PostBatchedSensorData_WithMixedValidAndInvalidData_ProcessesOnlyValidReadings()
        {
            // Arrange
            var gatewayUuid = Guid.Parse("11111111-1111-1111-1111-111111111111");

            var batchedRequest = new ConnectedToGateway
            {
                GatewayUUID = gatewayUuid,
                Readings = new BatchedSensorRequest
                {
                    batch_id = "test-batch-5",
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
                                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
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
                                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                                    temperature_c = 23.1f,
                                    humidity_pct = 56.8f
                                }
                            }
                        }
                    }
                }
            };

            // Mock gateway lookup
            _mockSqlGet.Setup(x => x.FetchAsync(
                "Secrets.Gateway",
                It.IsAny<Dictionary<string, object>>(),
                null))
                .ReturnsAsync(new Dictionary<string, object> { { "Id", 1 } });

            // Mock no existing sensor readings
            _mockSqlGetAdvanced.Setup(x => x.FetchWithJoinsAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<Func<System.Data.IDataRecord, object>>()))
                .ReturnsAsync(new List<object>());

            // Mock no existing sensor records
            _mockSqlGet.Setup(x => x.FetchAsync(
                "Measurements.Sensor",
                It.IsAny<Dictionary<string, object>>(),
                null))
                .ReturnsAsync((Dictionary<string, object>?)null);

            // Act
            var result = await _controller.PostBatchedSensorData(batchedRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Successfully processed", okResult.Value?.ToString());

            // Verify that InsertAsync was called only once (for the valid reading)
            _mockSqlInsert.Verify(x => x.InsertAsync(
                "Measurements.Sensor",
                It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        public void Dispose()
        {
            _testDb.Dispose();
        }
    }
}
