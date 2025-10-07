IF OBJECT_ID('dbo.GpsData', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.GpsData (
       DeviceId NVARCHAR(100),
       Latitude FLOAT,
       Longitude FLOAT,
       Timestamp DATETIME
    );
END
GO