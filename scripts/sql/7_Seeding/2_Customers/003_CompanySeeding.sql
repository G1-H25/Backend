-- Declare variable for PostAddressId and set it based on ZipCode
DECLARE @PostAddressId INT;
SET @PostAddressId = (SELECT Id FROM Customers.PostAddress WHERE ZipCode = '12177');

-- Insert Chas Academy AB if not exists
IF NOT EXISTS (
    SELECT 1 FROM Customers.Company WHERE CompanyName = 'Chas Academy AB'
)
BEGIN 
    INSERT INTO Customers.Company (CompanyName, Email, PostAddressId)
    VALUES ('Chas Academy AB', 'info@chasacademy.se', @PostAddressId);
END

-- Insert Carrier Logistics AB if not exists
IF NOT EXISTS (
    SELECT 1 FROM Customers.Company WHERE CompanyName = 'Carrier Logistics AB'
)
BEGIN
    INSERT INTO Customers.Company (CompanyName, Email, PostAddressId)
    VALUES ('Carrier Logistics AB', 'carrier@logistics.se', @PostAddressId);
END

-- Insert Recipient Pharma AB if not exists
IF NOT EXISTS (
    SELECT 1 FROM Customers.Company WHERE CompanyName = 'Recipient Pharma AB'
)
BEGIN
    INSERT INTO Customers.Company (CompanyName, Email, PostAddressId)
    VALUES ('Recipient Pharma AB', 'recipient@pharma.se', @PostAddressId);
END

-- Insert Sender Foods AB if not exists
IF NOT EXISTS (
    SELECT 1 FROM Customers.Company WHERE CompanyName = 'Sender Foods AB'
)
BEGIN
    INSERT INTO Customers.Company (CompanyName, Email, PostAddressId)
    VALUES ('Sender Foods AB', 'sender@foods.se', @PostAddressId);
END
GO
