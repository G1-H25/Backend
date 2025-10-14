IF NOT EXISTS (
    SELECT 1 FROM Secrets.Account WHERE AccountUsername = 'Admin'
)
BEGIN
    INSERT INTO Secrets.Account (AccountUsername, AccountPassword, AccountRole, DateCreated, Note)
    VALUES ('Admin', 'hello!', 'Admin', '2025-10-04T14:30:00', 'Test admin');
END
GO
