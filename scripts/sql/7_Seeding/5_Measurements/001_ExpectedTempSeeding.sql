IF
    NOT EXISTS (
        SELECT 1
        FROM Measurements.ExpectedTemp
        WHERE Note = 'Medicine'
    )
    BEGIN
        INSERT INTO Measurements.ExpectedTemp (Note, Min, Max)
        VALUES ('Medicine', 2.5, 8.5)
    END
GO