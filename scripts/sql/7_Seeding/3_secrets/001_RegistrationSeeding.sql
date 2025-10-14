IF
    NOT EXISTS (
        SELECT 1
        FROM Secrets.Registration
        WHERE Plate = 'AAA111'
    )
    BEGIN
        INSERT INTO Secrets.Registration (Plate, Brand, Model)
        VALUES ('AAA111', 'Mercedes', 'Vito')
    END
GO