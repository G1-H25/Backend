IF OBJECT_ID('Measurements.Sensor', 'U') IS NULL
BEGIN
    CREATE TABLE Measurements.Sensor
    (
        Id INT IDENTITY (1,1) PRIMARY KEY,
        PolledAt DATETIME NOT NULL,
        CurrentTemp DECIMAL(3,1),
        CurrentHumid INT,
        TempTimeOutside INT DEFAULT 0,
        HumidTimeOutside INT DEFAULT 0,
        TempTimerStart DATETIME NULL,
        HumidTimerStart DATETIME NULL,
        Note VARCHAR(20)
    );
END

DECLARE @Now DATETIME = SYSDATETIME();

IF  
    NOT EXISTS (
        SELECT 1 
        FROM Measurements.Sensor
        WHERE Note = "TestSensor1"
)
BEGIN
    INSERT INTO Measurements.Sensor(PolledAt, CurrentTemp, CurrentHumid, Note)
    VALUES (@Now, 5.4, 60, 'TestSensor1');
END
GO