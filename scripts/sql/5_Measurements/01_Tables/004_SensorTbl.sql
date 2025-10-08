IF OBJECT_ID('Measurements.Sensor', 'U') IS NULL
BEGIN
    CREATE TABLE Measurements.Sensor(
        Id INT IDENTITY (1,1) PRIMARY KEY,
        HistoryId INT NOT NULL CONSTRAINT FK_Sensor_HistoryId
            FOREIGN KEY (HistoryId) REFERENCES Measurements.SensorHistory(Id),
        Note VARCHAR(20)
    );
END
GO