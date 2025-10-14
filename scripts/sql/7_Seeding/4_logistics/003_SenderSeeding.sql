DECLARE @CompanyId INT;
SET @CompanyId = (SELECT Id FROM Customers.Company WHERE CompanyName = 'Sender Foods AB');
INSERT INTO Logistics.Sender (CompanyId)
VALUES (@CompanyId)

-- Join data from Customer.Company references 
SELECT sen.Id, com.CompanyName, com.Email, pa.Zipcode
FROM Logistics.Sender sen
    JOIN Customers.Company com ON sen.CompanyId = com.Id
    JOIN Customers.PostAddress pa ON com.PostAddressId = pa.Id