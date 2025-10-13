IF NOT EXISTS (
    SELECT 1
    FROM Logistics.TransportRoute
    WHERE Code = 'STO'
)
BEGIN
    INSERT INTO Logistics.TransportRoute(Code, Area)
    VALUES ('STO', 'Stockholm');
END
GO