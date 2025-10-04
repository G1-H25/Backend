IF OBJECT_ID('Logistics.Recipient', 'U') IS NULL
    BEGIN
        CREATE TABLE Logistics.Recipient (
            Id INT IDENTITY (1, 1) PRIMARY KEY,
            CompanyId INT CONSTRAINT FK_Recipient_CompanyId
                FOREIGN KEY (CompanyId) REFERENCES Customers.Company(Id)
        )
    END

DECLARE @CompanyId INT;
SET @CompanyId = (SELECT Id FROM Customers.Company WHERE CompanyName = 'Recipient Pharma AB');
INSERT INTO Logistics.Recipient (CompanyId)
VALUES (@CompanyId)

-- Join data from Customer.Company references 
SELECT recip.Id, com.CompanyName, con.Email, pa.Zipcode
FROM Logistics.Recipient recip
    JOIN Customers.Company com ON recip.CompanyId = com.Id
    JOIN Customers.PostAddress pa ON com.PostAddressId = pa.Id
    JOIN Customers.Contact con ON com.ContactId = con.Id