IF
    NOT EXISTS (
        SELECT TOP 1
            Id,
            Longitude,
            Latitude,
            PolledAt
        FROM Secrets.LocationHistory
        ORDER BY PolledAt DESC
    )
    BEGIN
        INSERT INTO Secrets.LocationHistory (Longitude, Latitude, PolledAt)
        VALUES ('18.07 E', '59.35 N', '2025-10-04T14:30:00')
    END
GO