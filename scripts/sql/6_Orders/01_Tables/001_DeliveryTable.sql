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

DECLARE @RecipientId INT, @SenderId INT, @CarrierId INT,  @Now DATETIME2 = SYSDATETIME();

SET @CarrierId = (SELECT Id FROM Customers.Company WHERE CompanyName = 'Carrier Logistics AB');
SET @SenderId = (SELECT Id FROM Customers.Company WHERE CompanyName = 'Sender Foods AB');
SET @RecipientId = (SELECT Id FROM Customers.Company WHERE CompanyName = 'Recipient Pharma AB');

IF NOT EXISTS (SELECT 1 FROM Logistics.Recipient WHERE CompanyId = @RecipientId)
BEGIN
    INSERT INTO Logistics.Recipient (CompanyId) VALUES (@RecipientId);
END
SET @RecipientId = (SELECT Id FROM Logistics.Recipient WHERE CompanyId = @RecipientId);

IF NOT EXISTS (SELECT 1 FROM Logistics.Sender WHERE CompanyId = @SenderId)
BEGIN
    INSERT INTO Logistics.Sender (CompanyId) VALUES (@SenderId);
END
SET @SenderId = (SELECT Id FROM Logistics.Sender WHERE CompanyId = @SenderId);

IF NOT EXISTS (SELECT 1 FROM Logistics.Carrier WHERE CompanyId = @CarrierId)
BEGIN
    -- NOTE: Carrier also needs VehicleId!
    INSERT INTO Logistics.Carrier (CompanyId) VALUES (@CarrierId); -- replace with actual VehicleId
END
SET @CarrierId = (SELECT Id FROM Logistics.Carrier WHERE CompanyId = @CarrierId);

INSERT INTO Orders.Delivery (RouteId, ExpectedTempId, ExpectedHumidId, RecipientId, SenderId, CarrierId, OrderPlaced)
VALUES (1, 1, 1, @RecipientId, @SenderId, @CarrierId, @Now);
