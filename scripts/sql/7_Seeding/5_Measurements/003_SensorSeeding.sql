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
    INSERT INTO Measurements.Sensor(GatewayId, PolledAt, TemperatureCel, HumdityPct)
    VALUES (@GatewayId, @Now, 5.4, 60);
END
GO
