using System.Collections.Generic;

namespace GpsApp.DTO
{
    //! @brief The contents of the json batch request from gateway
    public class BatchedSensorRequest
    {
        public string batch_id { get; set; } = string.Empty;
        public long generated_at { get; set; }
        public List<SensorData> sensors { get; set; } = new();
    }

    //! @brief A sensor with its measurements
    public class SensorData
    {
        public Guid sensor_UUID { get; set; }
        public List<Measurement> measurements { get; set; } = new();
    }

    //! @brief A single measurement reading
    public class Measurement
    {
        public long timestamp { get; set; }
        public float temperature_c { get; set; }
        public float humidity_pct { get; set; }
    }

    //! @brief A batched sensor request connected to a gateway
    public class ConnectedToGateway
    {
        public Guid GatewayUUID { get; set; }
        public BatchedSensorRequest Readings { get; set; } = new();
    }
}
