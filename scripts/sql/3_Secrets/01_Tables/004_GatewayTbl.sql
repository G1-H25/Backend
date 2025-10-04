IF OBJECT_ID('Secrets.Gateway', 'U') IS NULL
    BEGIN
    CREATE TABLE Secrets.Gateway
    (
        Id INT IDENTITY (1, 1) PRIMARY KEY,
        UserId INT NOT NULL CONSTRAINT FK_Gateway_UserId
            FOREIGN KEY (UserId) REFERENCES Secrets.Account(Id),
        CurrentLocationId INT NOT NULL CONSTRAINT FK_Gateway_CurrentLocationId
            FOREIGN KEY (CurrentLocationId) REFERENCES Secrets.LocationHistory(Id)
    )
END

DECLARE @UserId INT, @CurrentLocationId INT;
SET @UserId = (SELECT Id FROM Secrets.Account WHERE UserName = 'Admin');
SET @CurrentLocationId = (SELECT Id FROM Secrets.LocationHistory WHERE PolledAt = '2025-10-04T14:30:00');

BEGIN
INSERT INTO Secrets.Gateway (UserId, CurrentLocationId)
VALUES (@UserId, @CurrentLocationId)
END
GO