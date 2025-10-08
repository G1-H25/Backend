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
GO
