IF OBJECT_ID('Logistics.Sender', 'U') IS NULL
    BEGIN
        CREATE TABLE Logistics.Sender (
            Id INT IDENTITY (1, 1) PRIMARY KEY,
            CompanyId INT CONSTRAINT FK_Sender_CompanyId
                FOREIGN KEY (CompanyId) REFERENCES Customers.Company(Id)
        )
    END

DECLARE @CompanyId INT;
SET @CompanyId = (SELECT Id FROM Customers.Company WHERE CompanyName = 'Sender Foods AB');
INSERT INTO Logistics.Sender (CompanyId)
VALUES (@CompanyId)

-- Join data from Customer.Company references 
SELECT sen.Id, com.CompanyName, con.Email, pa.Zipcode
FROM Logistics.Sender sen
    JOIN Customers.Company com ON sen.CompanyId = com.Id
    JOIN Customers.PostAddress pa ON com.PostAddressId = pa.Id
    JOIN Customers.Contact con ON com.ContactId = con.Id