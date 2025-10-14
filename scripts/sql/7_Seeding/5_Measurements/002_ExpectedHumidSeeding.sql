IF
    NOT EXISTS (
        SELECT 1
        FROM Measurements.ExpectedHumid
        WHERE Note = 'Medicine'
    )
    BEGIN
        INSERT INTO Measurements.ExpectedHumid (Note, Min, Max)
        VALUES ('Medicine', 30, 70)
    END
GO