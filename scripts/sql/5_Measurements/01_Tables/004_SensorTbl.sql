IF OBJECT_ID('Measurements.Sensor', 'U') IS NULL
BEGIN
    CREATE TABLE Measurements.Sensor(
        Id INT IDENTITY (1,1) PRIMARY KEY,
        HistoryId INT NOT NULL CONSTRAINT FK_Sensor_HistoryId
            FOREIGN KEY (HistoryId) REFERENCES Measurements.SensorHistory(Id),
        Note VARCHAR(20)
    );
END

DECLARE @HistoryId INT;
SET @HistoryId = (SELECT Id FROM Measurements.SensorHistory WHERE Id = 1);

IF NOT EXISTS (
    SELECT 1 
    FROM Measurements.Sensor
    WHERE Note = "TestSensor1"
)
BEGIN
    INSERT INTO Measurements.Sensor(HistoryId, Note)
    VALUES (@HistoryId, 'TestSensor1');
END
GO