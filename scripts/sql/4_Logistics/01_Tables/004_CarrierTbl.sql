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

DECLARE @CompanyId INT, @VehicleId INT;
SET @CompanyId = (SELECT Id FROM Customers.Company WHERE CompanyName = 'Carrier Logistics AB');
SET @VehicleId = (SELECT Id FROM Secrets.Registration WHERE Plate = 'AAA111')
INSERT INTO Logistics.Carrier (CompanyId, VehicleId)
VALUES (@CompanyId, @VehicleId)

SELECT carrier.Id, reg.Plate, com.CompanyName, con.Email, pa.Zipcode
FROM Logistics.Carrier carrier
    JOIN Customers.Company com ON carrier.CompanyId = com.Id
    JOIN Customers.PostAddress pa ON com.PostAddressId = pa.Id
    JOIN Customers.Contact con ON com.ContactId = con.Id
    JOIN Secrets.Registration reg ON carrier.VehicleId = reg.Id