using GpsApp.DTO;
using System.Collections.Generic;

namespace GpsApp.Services
{
    /// <summary>
    /// Interface for sensor validation service
    /// </summary>
    public interface ISensorValidationService
    {
        /// <summary>
        /// Validates a batched sensor request
        /// </summary>
        /// <param name="request">The request to validate</param>
        /// <returns>Validation result with errors if any</returns>
        ValidationResult ValidateBatchedSensorRequest(ConnectedToGateway request);

        /// <summary>
        /// Validates individual sensor data
        /// </summary>
        /// <param name="sensor">The sensor to validate</param>
        /// <returns>List of validation errors</returns>
        List<string> ValidateSensorData(SensorData sensor);

        /// <summary>
        /// Validates a single sensor reading
        /// </summary>
        /// <param name="data">The sensor reading to validate</param>
        /// <returns>Validation result with errors if any</returns>
        ValidationResult ValidateSensorDto(SensorDto data);

        /// <summary>
        /// Validates a measurement
        /// </summary>
        /// <param name="measurement">The measurement to validate</param>
        /// <param name="sensorId">The sensor ID</param>
        /// <param name="measurementIndex">The measurement index for error context</param>
        /// <returns>List of validation errors</returns>
        List<string> ValidateMeasurement(Measurement measurement, int sensorId, int measurementIndex);
    }
}
