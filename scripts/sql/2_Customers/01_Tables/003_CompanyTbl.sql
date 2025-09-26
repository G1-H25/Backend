IF OBJECT_ID ('Customers.Company', 'U') IS NULL
BEGIN
    CREATE TABLE Customers.Company(
        Id INT IDENTITY (1,1) PRIMARY KEY,
        CompanyName VARCHAR(50) NOT NULL,
        Email VARCHAR(50) NOT NULL,
        Street NVARCHAR(40) NOT NULL,
        StreetNumber INT NOT NULL,
        PostNumberId INT NOT NULL CONSTRAINT FK_PostNumberId
            FOREIGN KEY (PostNumberId) REFERENCES Customers.PostAddress(Id),
        ContactId INT NOT NULL CONSTRAINT FK_ContactId
            FOREIGN KEY (ContactId) REFERENCES Customers.Contact(Id)
        
    );
END
GO

-- DECLARE Variables for REFERENCES and populate then with data from other tables.
DECLARE @PostAddressId INT, @ContactId INT;
SET @PostAddressId = (SELECT Id FROM Customers.PostAddress WHERE ZipCode = '12177')
SET @ContactId = (SELECT Id FROM Customers.Contact WHERE Email = 'johan.andersson@gmail.com')
-- SELF EXPLANATORY

IF NOT EXISTS (
    SELECT 1
    FROM Customers.CompanyName
    WHERE CompanyName = 'Chas Academy AB'
)
BEGIN 
    INSERT INTO Customers.CompanyName (CompanyName, Email, Street, StreetNumber, PostNumberId, ContactId)
    VALUES ('Chas Academy AB', 'info@chasacademy.se', 'Arenavägen', 61, @PostAddressId, @ContactId);
END
GO