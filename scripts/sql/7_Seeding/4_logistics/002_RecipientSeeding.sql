DECLARE @CompanyId INT;
SET @CompanyId = (SELECT Id FROM Customers.Company WHERE CompanyName = 'Recipient Pharma AB');
INSERT INTO Logistics.Recipient (CompanyId)
VALUES (@CompanyId)

-- Join data from Customer.Company references 
SELECT recip.Id, com.CompanyName, com.Email, pa.Zipcode
FROM Logistics.Recipient recip
    JOIN Customers.Company com ON recip.CompanyId = com.Id
    JOIN Customers.PostAddress pa ON com.PostAddressId = pa.Id