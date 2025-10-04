IF OBJECT_ID ('Customers.Company', 'U') IS NULL
BEGIN
    CREATE TABLE Customers.Company(
        Id INT IDENTITY (1,1) PRIMARY KEY,
        CompanyName VARCHAR(50) NOT NULL,
        Email VARCHAR(50) NOT NULL,
        Street NVARCHAR(40) NOT NULL,
        StreetNumber INT NOT NULL,
        PostAddressId INT NOT NULL CONSTRAINT FK_Company_PostAddressId
            FOREIGN KEY (PostAddressId) REFERENCES Customers.PostAddress(Id),
        ContactId INT NOT NULL CONSTRAINT FK_Company_ContactId
            FOREIGN KEY (ContactId) REFERENCES Customers.Contact(Id)
        
    );
END

-- DECLARE Variables for REFERENCES and populate then with data from other tables.
DECLARE @PostAddressId INT, @ContactId INT;
SET @PostAddressId = (SELECT Id FROM Customers.PostAddress WHERE ZipCode = '12177')
SET @ContactId = (SELECT Id FROM Customers.Contact WHERE Email = 'johan.andersson@gmail.com')
-- SELF EXPLANATORY

IF NOT EXISTS (
    SELECT 1
    FROM Customers.Company
    WHERE CompanyName = 'Chas Academy AB'
)
-- Chas Academy test
BEGIN 
    INSERT INTO Customers.Company (CompanyName, Email, Street, StreetNumber, PostAddressId, ContactId)
    VALUES ('Chas Academy AB', 'info@chasacademy.se', 'Arenavägen', 61, @PostAddressId, @ContactId);
END

-- Carrier company
IF NOT EXISTS (
    SELECT 1 FROM Customers.Company WHERE CompanyName = 'Carrier Logistics AB'
)
BEGIN
    INSERT INTO Customers.Company (CompanyName, Email, Street, StreetNumber, PostAddressId, ContactId)
    VALUES ('Carrier Logistics AB', 'carrier@logistics.se', 'Transportvägen', 12, @PostAddressId, @ContactId);
END

-- Recipient company
IF NOT EXISTS (
    SELECT 1 FROM Customers.Company WHERE CompanyName = 'Recipient Pharma AB'
)
BEGIN
    INSERT INTO Customers.Company (CompanyName, Email, Street, StreetNumber, PostAddressId, ContactId)
    VALUES ('Recipient Pharma AB', 'recipient@pharma.se', 'Mottagarvägen', 22, @PostAddressId, @ContactId);
END

-- Sender company
IF NOT EXISTS (
    SELECT 1 FROM Customers.Company WHERE CompanyName = 'Sender Foods AB'
)
BEGIN
    INSERT INTO Customers.Company (CompanyName, Email, Street, StreetNumber, PostAddressId, ContactId)
    VALUES ('Sender Foods AB', 'sender@foods.se', 'Avsändargatan', 33, @PostAddressId, @ContactId);
END
GO