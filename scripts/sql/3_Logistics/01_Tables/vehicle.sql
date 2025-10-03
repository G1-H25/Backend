IF OBJECT_ID('Vehicle', 'U') IS NULL
    BEGIN
        CREATE TABLE Vehicle (
            Id INT IDENTITY (1, 1),
            Type VARCHAR(20),
            Registration VARCHAR(30),
            PRIMARY KEY (Id)
        )
    END
GO
