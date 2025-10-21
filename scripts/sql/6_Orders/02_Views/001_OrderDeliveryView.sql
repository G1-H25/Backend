CREATE VIEW Orders.DeliveryDetailsView
AS
    SELECT
        deliv.Id AS DeliveryId,
        troute.Code AS RouteCode,
        sens.TemperatureCel AS CurrentTemp,
        sensTemp.Min AS ExpTempMin,
        sensTemp.Max AS ExpTempMax,
        sens.TempMinMeasured AS TempMinMeasured,
        sens.TempMaxMeasured AS TempMaxMeasured,
        sens.TempTimeOutside AS TempOutOfRange,
        sens.HumdityPct AS CurrentHumid,
        sensHumid.Min AS ExpHumidMin,
        sensHumid.Max AS ExpHumidMax,
        sens.HumidMinMeasured AS HumidMinMeasured,
        sens.HumidMaxMeasured AS HumidMaxMeasured,
        sens.HumidTimeOutside AS HumidOutOfRange,
        carrCom.CompanyName AS Carrier,
        senCom.CompanyName AS Sender,
        recCom.CompanyName AS Recipient,
        delstate.CurrentState AS CurrentState,
        deliv.OrderPlaced

    FROM Orders.Delivery deliv
        JOIN Orders.DeliveryState delstate ON deliv.Id = delstate.Id
        JOIN Logistics.TransportRoute troute ON deliv.RouteId = troute.Id
        JOIN Measurements.Sensor sens ON deliv.SensorId = sens.Id
        JOIN Measurements.ExpectedTemp sensTemp ON sens.Id = sensTemp.Id
        JOIN Measurements.ExpectedHumid sensHumid ON sens.Id = sensHumid.Id
        JOIN Logistics.Recipient rec ON deliv.RecipientId = rec.Id
        JOIN Customers.Company recCom ON rec.CompanyId = recCom.Id
        JOIN Logistics.Sender sen ON deliv.SenderId = sen.Id
        JOIN Customers.Company senCom ON sen.CompanyId = senCom.Id
        JOIN Logistics.Carrier carr ON deliv.CarrierId = carr.Id
        JOIN Customers.Company carrCom ON carr.CompanyId = carrCom.Id
GO