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

-- SEED DATA 
IF
    NOT EXISTS (
        SELECT 1
        FROM Secrets.Registration
        WHERE Plate = 'AAA111'
    )
    BEGIN
        INSERT INTO Secrets.Registration (Plate, Brand, Model)
        VALUES ('AAA111', 'Mercedes', 'Vito')
    END
GO

