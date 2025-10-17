namespace GpsApp.DTO
{


        /*
        CREATE VIEW Orders.DeliveryDetailsView
AS
    SELECT
        deliv.Id AS DeliveryId,
        troute.Code AS RouteCode,
        sens.TemperatureCel AS CurrentTemp,
        temp.Min AS ExpectedTempMin,
        temp.Max AS ExpectedTempMax,
        sens.TempMinMeasured AS TempMinMeasured,
        sens.TempMaxMeasured AS TempMaxMeasured,
        sens.TempTimeOutside AS TempOutOfRange,
        sens.HumdityPct AS CurrentHumid,
        humid.Min AS ExpectedHumidMin,
        humid.Max AS ExpectedHumidMax,
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
        JOIN Measurements.ExpectedTemp temp ON deliv.ExpectedTempId = temp.Id
        JOIN Measurements.ExpectedHumid humid ON deliv.ExpectedHumidId = humid.Id
        JOIN Logistics.Recipient rec ON deliv.RecipientId = rec.Id
        JOIN Customers.Company recCom ON rec.CompanyId = recCom.Id
        JOIN Logistics.Sender sen ON deliv.SenderId = sen.Id
        JOIN Customers.Company senCom ON sen.CompanyId = senCom.Id
        JOIN Logistics.Carrier carr ON deliv.CarrierId = carr.Id
        JOIN Customers.Company carrCom ON carr.CompanyId = carrCom.Id;
GO
        */
    public record DeliveryDto(
        int DeliveryId,
        string RouteCode,
        float CurrentTemp,
        float ExpectedTempMin,
        float ExpectedTempMax,
        float TempMinMeasured,
        float TempMaxMeasured,
        float TempOutOfRange,
        float CurrentHumid,
        float ExpectedHumidMin,
        float ExpectedHumidMax,
        float HumidMinMeasured,
        float HumidMaxMeasured,
        float HumidOutOfRange,
        string Carrier,
        string Sender,
        string Recipient,
        DateTime OrderPlaced
    );
}