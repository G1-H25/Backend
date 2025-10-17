IF OBJECT_ID('Measurements.Sensor', 'U') IS NULL
BEGIN
    CREATE TABLE Measurements.Sensor (
        Id INT IDENTITY (1, 1) PRIMARY KEY,
        GatewayId INT NOT NULL CONSTRAINT FK_Sensor_Gateway
            FOREIGN KEY (GatewayId) REFERENCES Secrets.Gateway(Id),
        PolledAt DATETIME NOT NULL,
        TemperatureCel DECIMAL(5,2) NULL,
        HumdityPct DECIMAL(5,2) NULL,
        TempTimeOutside INT DEFAULT 0,
        HumidTimeOutside INT DEFAULT 0,
        TempTimerStart DATETIME NULL,
        HumidTimerStart DATETIME NULL,
        Note VARCHAR(20) NULL
    );
END
GO
