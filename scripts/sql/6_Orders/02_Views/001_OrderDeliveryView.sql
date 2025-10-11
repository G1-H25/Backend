CREATE VIEW Orders.DeliveryDetailsView
AS
    SELECT
        deliv.Id AS DeliveryId,
        troute.Code AS RouteCode,
        sens.CurrentTemp AS CurrentTemp,
        temp.Min AS ExpTempMin,
        temp.Max AS ExpTempMax,
        sens.CurrentHumid AS CurrentHumid,
        humid.Min AS ExpHumidMin,
        humid.Max AS ExpHumidMax,
        sens.TempTimeOutside AS TempOutOfRange,
        sens.HumidTimeOutside AS HumidOutOfRange,
        carrCom.CompanyName AS CarrierName,
        senCom.CompanyName AS SenderName,
        recCom.CompanyName AS RecipientName,
        delstate.CurrentState AS CurrentState,
        deliv.OrderPlaced

    FROM Orders.Delivery deliv
        JOIN Orders.DeliveryState delstate ON deliv.Id = delstate.Id
        JOIN Logistics.TransportRoute troute ON deliv.RouteId = troute.Id
        JOIN Measurements.Sensor sens ON deliv.SensorId = sens.Id
        JOIN Measurements.ExpectedTemp temp ON deliv.ExpectedTempId = temp.Id
        JOIN Measurements.ExpectedHumid humid ON deliv.ExpectedHumidId = humid.Id
        JOIN Logistics.Recipient rec ON deliv.RecipientId = rec.Id
        JOIN Customers.Company recCom ON rec.CompanyId = recCom.Id
        JOIN Logistics.Sender sen ON deliv.SenderId = sen.Id
        JOIN Customers.Company senCom ON sen.CompanyId = senCom.Id
        JOIN Logistics.Carrier carr ON deliv.CarrierId = carr.Id
        JOIN Customers.Company carrCom ON carr.CompanyId = carrCom.Id;
GO