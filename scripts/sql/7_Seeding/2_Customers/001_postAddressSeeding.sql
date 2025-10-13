IF NOT EXISTS (
    SELECT 1 
    FROM Customers.PostAddress
    WHERE ZipCode = '12177' 
)
BEGIN
    INSERT INTO Customers.PostAddress (ZipCode, Locality, Country, Street, StreetNumber) 
    VALUES ('12177', 'Stockholm', 'Sweden', 'Some Street Name', 123);
END
GO