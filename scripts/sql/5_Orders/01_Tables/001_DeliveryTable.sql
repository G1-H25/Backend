-- First draft of how a Deliver order might look like.
IF OBJECT_ID('Orders.Delivery', 'U') IS NULL
BEGIN
    CREATE TABLE Orders.Delivery(
        Id INT IDENTITY (1,1) PRIMARY KEY,
        RouteId INT CONSTRAINT FK_RouteId 
            FOREIGN KEY (RouteId) REFERENCES Logistics.Route(Id),
        ExpectedTempId INT CONSTRAINT FK_ExpTempId
            FOREIGN KEY (ExpectedTempId) REFERENCES Measurements.ExpectedTemp(Id),
        ExpectedHumidId INT CONSTRAINT FK_ExpHumidId
            FOREIGN KEY (ExpectedHumidId) REFERENCES Measurements.ExpectedHumid(Id),
        CarrierId INT CONSTRAINT FK_CarrierId
            FOREIGN KEY (CarrierId) REFERENCES Logistics.Carrier(Id),
        SenderId INT CONSTRAINT FK_SenderId
            FOREIGN KEY (SenderId) REFERENCES Customers.Sender(Id),
        RecipientId INT CONSTRAINT FK_RecipientId
            FOREIGN KEY (RecipientId) REFERENCES Customers.Recipient(Id),
        StateId INT CONSTRAINT FK_StateId
            FOREIGN KEY (StateId) REFERENCES DeliveryState(Id),
        OrderPlaced DATETIME
    );
END
GO

IF OBJECT_ID('Orders.DeliveryState', 'U') IS NULL
BEGIN
    CREATE TABLE Orders.DeliveryState(
        Id INT IDENTITY (1,1) PRIMARY KEY,
        DeliveryId INT CONSTRAINT FK_DeliveryId
            FOREIGN KEY (DeliveryId) REFERENCES Delivery(Id),
        CurrentState VARCHAR(20),
        UpdatedAt DATETIME2
    );  
END
GO