IF OBJECT_ID('Measurements.Sensor', 'U') IS NULL
BEGIN
    CREATE TABLE Measurements.Sensor (
        Id INT IDENTITY (1, 1) PRIMARY KEY,
        GatewayId INT NOT NULL CONSTRAINT FK_Sensor_Gateway
            FOREIGN KEY (GatewayId) REFERENCES Secrets.Gateway(Id),
        ExpectedTempId INT NULL CONSTRAINT FK_Delivery_ExpTempId
            FOREIGN KEY (ExpectedTempId) REFERENCES Measurements.ExpectedTemp(Id),
        ExpectedHumidId INT NULL CONSTRAINT FK_Delivery_ExpHumidId
            FOREIGN KEY (ExpectedHumidId) REFERENCES Measurements.ExpectedHumid(Id),
        PolledAt DATETIME NOT NULL,
        TemperatureCel DECIMAL(3,1) NULL,
        HumdityPct DECIMAL(3,1) NULL,
        TempTimeOutside INT DEFAULT 0,
        HumidTimeOutside INT DEFAULT 0,
        TempTimerStart DATETIME NULL,
        HumidTimerStart DATETIME NULL,
        TempMinMeasured DECIMAL(3,1),
        TempMaxMeasured DECIMAL(3,1),
        HumidMinMeasured DECIMAL(3,1),
        HumidMaxMeasured DECIMAL(3,1),
        Note VARCHAR(20) NULL
    );
END
GO
