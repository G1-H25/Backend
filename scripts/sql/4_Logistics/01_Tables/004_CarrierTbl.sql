IF OBJECT_ID('Logistics.Carrier', 'U') IS NULL
    BEGIN
    CREATE TABLE Logistics.Carrier (
        Id INT IDENTITY (1, 1) PRIMARY KEY,
        CompanyId INT CONSTRAINT FK_Carrier_CompanyId
                FOREIGN KEY (CompanyId) REFERENCES Customers.Company(Id),
        VehicleId INT CONSTRAINT FK_Carrier_VehicleId
                FOREIGN KEY (VehicleId) REFERENCES Secrets.Vehicle(Id)
    )
END
GO
