IF OBJECT_ID('Measurements.SensorHistory', 'U') IS NULL
BEGIN
    CREATE TABLE Measurements.SensorHistory(
        Id INT IDENTITY (1,1) PRIMARY KEY,
        PolledAt DATETIME2 NOT NULL
            CONSTRAINT UQ_SensorHistory_PolledAt UNIQUE (PolledAt),
        TemperatureCel DECIMAL(3,1),
        HumidityPct DECIMAL(3,1)
    );
END

DECLARE @Now DATETIME2 = SYSDATETIME();

-- SEED DATA
IF NOT EXISTS (
    SELECT 1
    FROM Measurements.SensorHistory
    WHERE PolledAt = @Now
)
BEGIN
    INSERT INTO Measurements.SensorHistory(PolledAt, TemperatureCel, HumidityPct)
    VALUES (@Now, -10, 40);
END
GO