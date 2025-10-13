IF OBJECT_ID('dbo.GpsData', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.GpsData (
       Id INT IDENTITY(1,1) PRIMARY KEY,           
       DeviceId INT NOT NULL,                       -- FK to Secrets.Gateway.Id 
       Latitude FLOAT NOT NULL,
       Longitude FLOAT NOT NULL,
       Timestamp DATETIME NOT NULL,
       CONSTRAINT FK_GpsData_Device FOREIGN KEY (DeviceId) REFERENCES Secrets.Gateway(Id)
    );
END
GO
