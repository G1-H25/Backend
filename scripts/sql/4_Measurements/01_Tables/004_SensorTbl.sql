IF OBJECT_ID(Measurements.Sensor) IS NULL
BEGIN
    CREATE TABLE Measurements.Sensor(
        Id INT IDENTITY (1,1) PRIMARY KEY,
        HistoryId INT  NOT NULL CONSTRAINT FK_HistoryId
            FOREIGN KEY (HistoryId) REFERENCES Measurements.History(Id),
        Note VARCHAR(20)
    );
END
GO

DECLARE @HistoryId INT NOT NULL;
SET @HistoryId = (SELECT Id FROM Measurements.History WHERE Id = 1);

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