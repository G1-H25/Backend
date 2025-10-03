IF OBJECT_ID('Sender', 'U') IS NULL
    BEGIN
        CREATE TABLE Sender (
            Id INT IDENTITY (1, 1),
            PRIMARY KEY (Id),
            FOREIGN KEY (CompanyId) REFERENCES Company (Id),
            FOREIGN KEY (PosadressId) REFERENCES Postadress (Id),
            FOREIGN KEY (ShipmentId) REFERENCES Shipment (Id)
        )
    END
GO
