namespace GpsApp.DTO
{
    public class SensorDto
    {
        public Guid GatewayUUID { get; set; }
        public Guid UUID { get; set; }
        public DateTime PolledAt { get; set; }
        public float? TemperatureCel { get; set; }
        public float? HumdityPct { get; set; }
    }

    // Helper class to represent sensor reading data for database operations
    public class SensorReading
    {
        public DateTime? TempTimerStart { get; set; }
        public int TempTimeOutside { get; set; }
        public DateTime? HumidTimerStart { get; set; }
        public int HumidTimeOutside { get; set; }
        public DateTime PolledAt { get; set; }
        public float TempMinMeasured { get; set; }
        public float TempMaxMeasured { get; set; }
        public float HumidMinMeasured { get; set; }
        public float HumidMaxMeasured { get; set; }
    }

    // DTO for sensor reading response
    public record SensorReadingDto(
        int SensorId,
        int GatewayId,
        DateTime PolledAt,
        float? TemperatureCel,
        float? HumidityPct,
        int TempTimeOutside,
        int HumidTimeOutside
    );

    // DTO for paginated sensor readings response
    public record PaginatedSensorReadingsResponse(
        List<SensorReadingDto> Readings,
        int Page,
        int PageSize,
        int TotalCount,
        int TotalPages,
        DateTime? FromDate,
        DateTime? ToDate
    );

    // DTO for API endpoint information
    public record ApiEndpointInfo(
        string Method,
        string Path,
        string Description,
        string[] Parameters,
        string[] ResponseTypes,
        bool RequiresAuth
    );

    // DTO for API discovery response
    public record ApiDiscoveryResponse(
        string ApiName,
        string Version,
        string BaseUrl,
        List<ApiEndpointInfo> Endpoints,
        Dictionary<string, string> Metadata
    );
}