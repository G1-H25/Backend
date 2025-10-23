DECLARE @Now DATETIME = SYSDATETIME();

DECLARE @GatewayId INT = (
    SELECT TOP 1 Id FROM Secrets.Gateway
    ORDER BY Id 
);

IF NOT EXISTS (
    SELECT 1 
    FROM Measurements.Sensor
)
BEGIN
    INSERT INTO Measurements.Sensor(
        GatewayId, 
        UUID,
        PolledAt, 
        TemperatureCel, 
        HumdityPct, 
        TempMinMeasured,
        TempMaxMeasured,
        HumidMinMeasured,
        HumidMaxMeasured
        )
    VALUES (@GatewayId, NEWID(), @Now, 5.4, 60.0, 4.9, 6.7, 40, 78.0);
END
GO
