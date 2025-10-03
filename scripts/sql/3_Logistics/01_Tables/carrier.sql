IF OBJECT_ID('Carrier', 'U') IS NULL
    BEGIN
        CREATE TABLE Carrier (
            Id INT IDENTITY (1, 1),
            Type VARCHAR(20),
            PRIMARY KEY (Id),
            FOREIGN KEY (VehicleId) REFERENCES Vehicle (Id),
            FOREIGN KEY (Contact) REFERENCES Company (Contact_Id)
        )
    END
GO
