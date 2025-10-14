IF OBJECT_ID('Logistics.Recipient', 'U') IS NULL
    BEGIN
        CREATE TABLE Logistics.Recipient (
            Id INT IDENTITY (1, 1) PRIMARY KEY,
            CompanyId INT CONSTRAINT FK_Recipient_CompanyId
                FOREIGN KEY (CompanyId) REFERENCES Customers.Company(Id)
        )
    END
