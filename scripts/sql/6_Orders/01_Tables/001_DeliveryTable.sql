-- First draft of how a Deliver order might look like.
IF OBJECT_ID('Orders.Delivery', 'U') IS NULL
BEGIN
    CREATE TABLE Orders.Delivery
    (
        Id INT IDENTITY (1,1) PRIMARY KEY,
        RouteId INT CONSTRAINT FK_Delivery_RouteId 
            FOREIGN KEY (RouteId) REFERENCES Logistics.TransportRoute(Id),
        ExpectedTempId INT CONSTRAINT FK_Delivery_ExpTempId
            FOREIGN KEY (ExpectedTempId) REFERENCES Measurements.ExpectedTemp(Id),
        ExpectedHumidId INT CONSTRAINT FK_Delivery_ExpHumidId
            FOREIGN KEY (ExpectedHumidId) REFERENCES Measurements.ExpectedHumid(Id),
        RecipientId INT CONSTRAINT FK_Delivery_RecipientId
            FOREIGN KEY (RecipientId) REFERENCES Logistics.Recipient(Id),
        SenderId INT CONSTRAINT FK_Delivery_SenderId
            FOREIGN KEY (SenderId) REFERENCES Logistics.Sender(Id),
        CarrierId INT CONSTRAINT FK_Delivery_CarrierId
            FOREIGN KEY (CarrierId) REFERENCES Logistics.Carrier(Id),
        OrderPlaced DATETIME
    );
    END
GO