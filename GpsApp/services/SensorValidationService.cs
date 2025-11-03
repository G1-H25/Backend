using GpsApp.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GpsApp.Services
{
    /// <summary>
    /// Domain service for validating sensor data according to business rules
    /// </summary>
    public class SensorValidationService : ISensorValidationService
    {
        /// <summary>
        /// Validates a batched sensor request
        /// </summary>
        /// <param name="request">The request to validate</param>
        /// <returns>Validation result with errors if any</returns>
        public ValidationResult ValidateBatchedSensorRequest(ConnectedToGateway request)
        {
            var errors = new List<string>();

            if (request == null)
            {
                errors.Add("Request body cannot be null.");
                return new ValidationResult { IsValid = false, Errors = errors };
            }

            if (request.GatewayUUID == Guid.Empty)
            {
                errors.Add("GatewayUUID must be a valid non-empty GUID.");
            }

            if (request.Readings?.sensors == null || !request.Readings.sensors.Any())
            {
                errors.Add("At least one sensor must be provided.");
            }
            else
            {
                // Validate that at least one sensor has measurements
                if (!request.Readings.sensors.Any(s => s.measurements != null && s.measurements.Any()))
                {
                    errors.Add("At least one sensor measurement must be provided.");
                }

                // Validate each sensor and its measurements
                foreach (var sensor in request.Readings.sensors)
                {
                    var sensorErrors = ValidateSensorData(sensor);
                    errors.AddRange(sensorErrors);
                }
            }

            return new ValidationResult
            {
                IsValid = !errors.Any(),
                Errors = errors
            };
        }

        /// <summary>
        /// Validates individual sensor data
        /// </summary>
        /// <param name="sensor">The sensor to validate</param>
        /// <returns>List of validation errors</returns>
        public List<string> ValidateSensorData(SensorData sensor)
        {
            var errors = new List<string>();

            if (sensor == null)
            {
                errors.Add("Sensor data cannot be null.");
                return errors;
            }

            if (sensor.sensor_UUID == default)
            {
                errors.Add($"Invalid sensor ID: {sensor.sensor_UUID}. Sensor ID must be provided");
            }

            if (sensor.measurements == null || !sensor.measurements.Any())
            {
                errors.Add($"Sensor {sensor.sensor_UUID} has no measurements.");
                return errors;
            }

            // Validate each measurement
            for (int i = 0; i < sensor.measurements.Count; i++)
            {
                var measurement = sensor.measurements[i];
                var measurementErrors = ValidateMeasurement(measurement, sensor.sensor_UUID, i);
                errors.AddRange(measurementErrors);
            }

            return errors;
        }

        /// <summary>
        /// Validates individual measurement data
        /// </summary>
        /// <param name="measurement">The measurement to validate</param>
        /// <param name="sensorId">The sensor ID for error context</param>
        /// <param name="measurementIndex">The measurement index for error context</param>
        /// <returns>List of validation errors</returns>
        public List<string> ValidateMeasurement(Measurement measurement, Guid sensorUUID, int measurementIndex)
        {
            var errors = new List<string>();

            if (measurement == null)
            {
                errors.Add($"Measurement {measurementIndex} for sensor {sensorUUID} cannot be null.");
                return errors;
            }

            if (measurement.timestamp <= 0)
            {
                errors.Add($"Invalid timestamp for sensor {sensorUUID}, measurement {measurementIndex}. Timestamp must be greater than 0.");
            }

            // Validate timestamp is not too far in the future (e.g., not more than 1 hour ahead)
            var maxFutureTime = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
            if (measurement.timestamp > maxFutureTime)
            {
                errors.Add($"Timestamp for sensor {sensorUUID}, measurement {measurementIndex} is too far in the future.");
            }

            // Validate timestamp is not too far in the past (e.g., not more than 1 year ago)
            var minPastTime = DateTimeOffset.UtcNow.AddYears(-1).ToUnixTimeSeconds();
            if (measurement.timestamp < minPastTime)
            {
                errors.Add($"Timestamp for sensor {sensorUUID}, measurement {measurementIndex} is too far in the past.");
            }

            // Validate temperature range (reasonable sensor range: -50°C to 100°C)
            if (measurement.temperature_c < -50 || measurement.temperature_c > 100)
            {
                errors.Add($"Temperature {measurement.temperature_c}°C for sensor {sensorUUID}, measurement {measurementIndex} is outside valid range (-50°C to 100°C).");
            }

            // Validate humidity range (0% to 100%)
            if (measurement.humidity_pct < 0 || measurement.humidity_pct > 100)
            {
                errors.Add($"Humidity {measurement.humidity_pct}% for sensor {sensorUUID}, measurement {measurementIndex} is outside valid range (0% to 100%).");
            }

            return errors;
        }

        /// <summary>
        /// Validates a single sensor DTO (for existing single sensor endpoint)
        /// </summary>
        /// <param name="sensorDto">The sensor DTO to validate</param>
        /// <returns>Validation result</returns>
        public ValidationResult ValidateSensorDto(SensorDto sensorDto)
        {
            var errors = new List<string>();

            if (sensorDto == null)
            {
                errors.Add("Sensor data cannot be null.");
                return new ValidationResult { IsValid = false, Errors = errors };
            }

            if (sensorDto.GatewayUUID == Guid.Empty)
            {
                errors.Add("GatewayUUID must be a valid non-empty GUID.");
            }

            if (sensorDto.UUID == Guid.Empty)
            {
                errors.Add("Sensor UUID must be a valid non-empty GUID.");
            }

            if (sensorDto.TemperatureCel == null)
            {
                errors.Add("Temperature is required.");
            }
            else if (sensorDto.TemperatureCel < -50 || sensorDto.TemperatureCel > 100)
            {
                errors.Add($"Temperature {sensorDto.TemperatureCel}°C is outside valid range (-50°C to 100°C).");
            }

            if (sensorDto.HumdityPct == null)
            {
                errors.Add("Humidity is required.");
            }
            else if (sensorDto.HumdityPct < 0 || sensorDto.HumdityPct > 100)
            {
                errors.Add($"Humidity {sensorDto.HumdityPct}% is outside valid range (0% to 100%).");
            }

            return new ValidationResult
            {
                IsValid = !errors.Any(),
                Errors = errors
            };
        }
    }

    /// <summary>
    /// Result of validation operation
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
