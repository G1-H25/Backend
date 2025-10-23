IF OBJECT_ID('Secrets.Gateway', 'U') IS NULL
    BEGIN
    CREATE TABLE Secrets.Gateway
    (
        Id INT IDENTITY (1, 1) PRIMARY KEY,
        UUID UNIQUEIDENTIFIER NOT NULL,
        GatewayURL VARCHAR(50),
        UserId INT NULL CONSTRAINT FK_Gateway_UserId
            FOREIGN KEY (UserId) REFERENCES Secrets.Account(Id),
        CurrentLocationId INT NULL CONSTRAINT FK_Gateway_CurrentLocationId
            FOREIGN KEY (CurrentLocationId) REFERENCES Secrets.LocationHistory(Id)
    )
END
GO