IF OBJECT_ID('Secrets.Registration', 'U') IS NULL
    BEGIN
        CREATE TABLE Secrets.Registration (
            Id INT IDENTITY (1, 1) PRIMARY KEY,
            Plate VARCHAR(10),
            Brand VARCHAR(20),
            Model VARCHAR(20)

        )
    END
GO