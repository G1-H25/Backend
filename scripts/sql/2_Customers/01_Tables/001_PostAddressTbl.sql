IF OBJECT_ID ('Customers.PostAddress', 'U') IS NULL
BEGIN
    CREATE TABLE Customers.PostAddress(
        Id INT IDENTITY (1,1) PRIMARY KEY,
        ZipCode VARCHAR(8) NOT NULL,
        Locality VARCHAR(30) NOT NULL,
        Country VARCHAR(30)
    );
END

-- MOCK DATA
IF NOT EXISTS (
    SELECT 1 
    FROM Customers.PostAddress
    WHERE ZipCode = '12177' 
)
BEGIN
    INSERT INTO Customers.PostAddress (ZipCode, Locality, Country) 
    VALUES ('12177', 'Stockholm', 'Sweden');
END
GO