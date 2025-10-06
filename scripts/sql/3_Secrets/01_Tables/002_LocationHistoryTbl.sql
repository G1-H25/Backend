IF OBJECT_ID('Secrets.CurrentLocation', 'U') IS NULL
    BEGIN
        CREATE TABLE Secrets.LocationHistory
        (
            Id INT IDENTITY (1, 1) PRIMARY KEY,
            PolledAt DATETIME2 NOT NULL,
            Longitude VARCHAR(10),
            Latitude VARCHAR(10)
        );
    END

IF
    NOT EXISTS (
        SELECT 1
        FROM Secrets.LocationHistory
        WHERE PolledAt = '2025-10-04T14:30:00'
    )
    BEGIN
        INSERT INTO Secrets.LocationHistory (Longitude, Latitude, PolledAt)
        VALUES ('18.07 E', '59.35 N', '2025-10-04T14:30:00')
    END
GO
