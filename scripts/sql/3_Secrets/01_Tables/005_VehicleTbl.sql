IF OBJECT_ID('Secrets.Vehicle', 'U') IS NULL
    BEGIN
    CREATE TABLE Secrets.Vehicle
    (
        Id INT IDENTITY (1, 1) PRIMARY KEY,
        GatewayId INT NOT NULL CONSTRAINT FK_Vehicle_GatewayId
                FOREIGN KEY (GatewayId) REFERENCES Secrets.Account(Id),
        RegistrationId INT NOT NULL CONSTRAINT FK_Vehicle_RegistrationId
                FOREIGN KEY (RegistrationId) REFERENCES Secrets.Registration(Id)
    )
END
GO