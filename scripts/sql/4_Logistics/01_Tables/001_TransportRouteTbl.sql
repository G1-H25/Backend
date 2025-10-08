IF OBJECT_ID('Logistics.TransportRoute', 'U') IS NULL
    BEGIN
        CREATE TABLE Logistics.TransportRoute (
            Id INT IDENTITY (1, 1) PRIMARY KEY,
            Code VARCHAR(50),
            Area VARCHAR(10),
        )
    END
GO
