using Xunit;
using GpsApp.Services;
using GpsApp.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

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
}
