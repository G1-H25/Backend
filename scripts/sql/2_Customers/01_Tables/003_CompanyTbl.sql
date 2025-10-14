IF OBJECT_ID ('Customers.Company', 'U') IS NULL
BEGIN
    CREATE TABLE Customers.Company(
        Id INT IDENTITY (1,1) PRIMARY KEY,
        CompanyName VARCHAR(50) NOT NULL,
        Email VARCHAR(50) NOT NULL,
        PostAddressId INT NOT NULL CONSTRAINT FK_Company_PostAddressId
            FOREIGN KEY (PostAddressId) REFERENCES Customers.PostAddress(Id)
    );
END
GO