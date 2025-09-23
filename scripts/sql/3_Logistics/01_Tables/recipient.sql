IF OBJECT_ID('Recipient', 'U') IS NULL
BEGIN
    CREATE TABLE Recipient(
        Id INT IDENTITY(1,1),
        PRIMARY KEY (Id), 
        FOREIGN KEY (CompanyId) REFERENCES Company(Id), 
        FOREIGN KEY (PosadressId) REFERENCES Postadress (Id),
        FOREIGN KEY(ShipmentId) REFERENCES Shipment(Id)

    )
END
GO
