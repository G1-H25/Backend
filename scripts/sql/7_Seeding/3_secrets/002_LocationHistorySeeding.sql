IF
    NOT EXISTS (
        SELECT 1
        FROM Secrets.LocationHistory
        WHERE PolledAt = '2025-10-04T14:30:00'
    )
    BEGIN
        INSERT INTO Secrets.LocationHistory (Longitude, Latitude, PolledAt)
        VALUES ('18.07 E', '59.35 N', '2025-10-04T14:30:00')
    END
GO