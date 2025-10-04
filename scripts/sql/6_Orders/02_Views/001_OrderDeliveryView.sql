CREATE VIEW Orders.DeliveryDetailsView
AS
    SELECT
        deliv.Id AS DeliveryId,
        troute.Code AS RouteCode,
        temp.Min AS TempMin,
        temp.Max AS TempMax,
        humid.Min AS HumidMin,
        humid.Max AS HumidMax,
        carrCom.CompanyName AS CarrierName,
        senCom.CompanyName AS SenderName,
        recCom.CompanyName AS RecipientName,
        deliv.OrderPlaced
    FROM Orders.Delivery deliv
        JOIN Logistics.TransportRoute troute ON deliv.RouteId = troute.Id
        JOIN Measurements.ExpectedTemp temp ON deliv.ExpectedTempId = temp.Id
        JOIN Measurements.ExpectedHumid humid ON deliv.ExpectedHumidId = humid.Id
        JOIN Logistics.Recipient rec ON deliv.RecipientId = rec.Id
        JOIN Customers.Company recCom ON rec.CompanyId = recCom.Id
        JOIN Logistics.Sender sen ON deliv.SenderId = sen.Id
        JOIN Customers.Company senCom ON sen.CompanyId = senCom.Id
        JOIN Logistics.Carrier carr ON deliv.CarrierId = carr.Id
        JOIN Customers.Company carrCom ON carr.CompanyId = carrCom.Id;