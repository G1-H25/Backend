IF OBJECT_ID('dbo.GpsData', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.gpsdata (
            deviceid NVARCHAR(100),
            latitude FLOAT,
            longitude FLOAT,
            createdat DATETIME
        );
    END
GO
