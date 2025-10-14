IF OBJECT_ID('Orders.DeliveryState', 'U') IS NULL
BEGIN
    CREATE TABLE Orders.DeliveryState
    (
        Id INT IDENTITY (1,1) PRIMARY KEY,
        DeliveryId INT CONSTRAINT FK_DeliveryState_DeliveryId
            FOREIGN KEY (DeliveryId) REFERENCES Orders.Delivery(Id),
        CurrentState VARCHAR(20),
        UpdatedAt DATETIME2
    );
END
GO