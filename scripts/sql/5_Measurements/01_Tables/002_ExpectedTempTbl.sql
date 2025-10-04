IF OBJECT_ID('Measurements.ExpectedTemp', 'U') IS NULL
    BEGIN
        CREATE TABLE Measurements.ExpectedTemp (
            Id INT IDENTITY (1, 1) PRIMARY KEY,
            Note VARCHAR(50) NOT NULL,
            Min DECIMAL(3, 1) NOT NULL,
            Max DECIMAL(3, 1) NOT NULL
        );
    END
GO

-- SEED DATA 
IF
    NOT EXISTS (
        SELECT 1
        FROM Measurements.ExpectedTemp
        WHERE Note = 'Medicine'
    )
    BEGIN
        INSERT INTO Measurements.ExpectedTemp (Note, Min, Max)
        VALUES ('Medicine', 2.5, 8.5)
    END
GO
