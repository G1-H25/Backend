IF OBJECT_ID('Logistics.Sender', 'U') IS NULL
    BEGIN
        CREATE TABLE Logistics.Sender (
            Id INT IDENTITY (1, 1) PRIMARY KEY,
            CompanyId INT CONSTRAINT FK_Sender_CompanyId
                FOREIGN KEY (CompanyId) REFERENCES Customers.Company(Id)
        )
    END
GO