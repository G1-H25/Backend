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
GO