public record SensorTemperatureRequest(
    int SensorId,
    int GatewayId,
    DateTime PolledAt,
    decimal? TemperatureCel
);
