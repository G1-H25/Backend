IF OBJECT_ID('Secrets.Account', 'U') IS NULL
    BEGIN
        CREATE TABLE Secrets.Account
        (
            Id INT IDENTITY(1,1) PRIMARY KEY,
            AccountUsername VARCHAR(100) NOT NULL UNIQUE,
            AccountPassword VARCHAR(100) NOT NULL,
            AccountRole VARCHAR(50) NOT NULL DEFAULT 'User',
            DateCreated DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
            Note VARCHAR(50)
        );
    END

-- SEED DATA 

IF NOT EXISTS (
    SELECT 1 FROM Secrets.Account WHERE Username = 'Admin'
)
BEGIN
    INSERT INTO Secrets.Account (Username, Password, Role, DateCreated, Note)
    VALUES ('Admin', 'hello!', 'Admin', '2025-10-04T14:30:00', 'Test admin');
END
GO
