IF OBJECT_ID('Customers.Contact', 'U') IS NULL
BEGIN
    CREATE TABLE Customers.Contact(
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FirstName NVARCHAR(30) NOT NULL,
        LastName NVARCHAR(30) NOT NULL,
        PhoneNumber VARCHAR(12) NOT NULL,
        Email VARCHAR(40) NOT NULL,
        Note VARCHAR(100)
    );
END

-- MOCK DATA With idempotent insert
IF NOT EXISTS (
    SELECT 1
    FROM Customers.Contact
    WHERE Email = 'johan.andersson@gmail.com'
)
BEGIN
INSERT INTO Customers.Contact (FirstName,LastName,PhoneNumber,Email,Note)
    VALUES ('Johan', 'Andersson', '0454723847', 'johan.andersson@gmail.com', 'Manager');
END
GO