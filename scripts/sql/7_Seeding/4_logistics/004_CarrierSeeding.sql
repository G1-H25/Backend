DECLARE @CompanyId INT, @VehicleId INT;
SET @CompanyId = (SELECT Id FROM Customers.Company WHERE CompanyName = 'Carrier Logistics AB');
SET @VehicleId = (SELECT Id FROM Secrets.Registration WHERE Plate = 'AAA111')
INSERT INTO Logistics.Carrier (CompanyId, VehicleId)
VALUES (@CompanyId, @VehicleId)

SELECT carrier.Id, reg.Plate, com.CompanyName, com.Email, pa.Zipcode
FROM Logistics.Carrier carrier
    JOIN Customers.Company com ON carrier.CompanyId = com.Id
    JOIN Customers.PostAddress pa ON com.PostAddressId = pa.Id
    JOIN Secrets.Registration reg ON carrier.VehicleId = reg.Id