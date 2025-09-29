IF OBJECT_ID('dbo.Gateway', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Gateway (
        Id INT PRIMARY KEY,
        UserID INT NOT NULL,
        CONSTRAINT FK_Gateway_User FOREIGN KEY (UserID) REFERENCES dbo.Users(Id)
    );
END
GO

