public record SensorHumidityDto(
    int SensorId,
    int GatewayId,
    DateTime PolledAt,
    decimal? HumidityPct
);
