IF OBJECT_ID('TransportRoute', 'U') IS NULL
    BEGIN
        CREATE TABLE TransportRoute (
            Id INT IDENTITY (1, 1),
            Name VARCHAR(50),
            Code VARCHAR(10),
            CuttOfTime TIME NOT NULL,
            PRIMARY KEY (Id)
        )
    END
GO
