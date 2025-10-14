IF OBJECT_ID('Measurements.ExpectedHumid', 'U') IS NULL
    BEGIN
        CREATE TABLE Measurements.ExpectedHumid (
            Id INT IDENTITY (1, 1) PRIMARY KEY,
            Note VARCHAR(50) NOT NULL,
            Min DECIMAL(3, 1) NOT NULL,
            Max DECIMAL(3, 1) NOT NULL
        );
    END
GO

