IF OBJECT_ID('DeliveryState', 'U') IS NULL
BEGIN
    CREATE TABLE DeliveryState(
        Id INT IDENTITY(1,1),
        Time NUMERIC NOT NULL DEFAULT CURRENT_TIMESTAMP, 
        PRIMARY KEY (Id), 
        FOREIGN KEY (ShippemntId) REFERENCES Shippment(Id), 
    )
END
GO
