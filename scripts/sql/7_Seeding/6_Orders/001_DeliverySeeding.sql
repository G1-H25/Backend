DECLARE @RecipientId INT, @SenderId INT, @CarrierId INT,  @Now DATETIME = SYSDATETIME();

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
    INSERT INTO Logistics.Carrier (CompanyId) VALUES (@CarrierId);
END
SET @CarrierId = (SELECT Id FROM Logistics.Carrier WHERE CompanyId = @CarrierId);

INSERT INTO Orders.Delivery (RouteId, SensorId, RecipientId, SenderId, CarrierId, OrderPlaced)
VALUES (1, 1, @RecipientId, @SenderId, @CarrierId, @Now);