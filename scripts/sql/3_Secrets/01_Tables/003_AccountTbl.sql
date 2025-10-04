IF OBJECT_ID('Secrets.Account', 'U') IS NULL
    BEGIN
        CREATE TABLE Secrets.Account
        (
            Id INT IDENTITY (1, 1) PRIMARY KEY,
            UserName VARCHAR(30) NOT NULL,
            UserPassword VARCHAR(30) NOT NULL,
            CreatedAt DATETIME2,
            Note VARCHAR(50)
        );
    END

-- SEED DATA 

IF
    NOT EXISTS (
        SELECT 1
        FROM Secrets.Account
        WHERE UserName = 'Admin'
    )
    BEGIN
        INSERT INTO Secrets.Account (UserName, UserPassword, CreatedAt, Note)
        VALUES ('Admin', 'hello!', '2025-10-04T14:30:00', 'Test admin')
    END
GO
