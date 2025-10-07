using Microsoft.AspNetCore.Mvc;
using GpsApp.DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System.ComponentModel.Design;

[ApiController]
[Route("[controller]")]
public class DeliveryGetController : ControllerBase
{
    private readonly SqlGetAdvanced _sqlAdvanced;

    public DeliveryGetController(SqlGetAdvanced sqlAdvanced)
    {
        _sqlAdvanced = sqlAdvanced;
    }

    [HttpGet]
    public async Task<IActionResult> GetDelivery([FromQuery] int? id)
    {
        var filters = new Dictionary<string, object>();
        if (id.HasValue)
            filters.Add("d.Id", id.Value);

        /*
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
        */

        var result = await _sqlAdvanced.FetchWithJoinsAsync(
            baseTable: "Orders.Delivery deliv",
            selectClause: @"
                deliv.Id, troute.Code, temp.Min, temp.Max, humid.Min,
                humid.Max, carrCom.CompanyName, senCom.CompanyName, recCom.CompanyName, deliv.OrderPlaced",
            joins: new List<string>
            {
                "JOIN Logistics.TransportRoute troute ON deliv.RouteId = troute.Id",
                "JOIN Measurements.ExpectedTemp temp ON deliv.ExpectedTempId = temp.Id",
                "JOIN Measurements.ExpectedHumid humid ON deliv.ExpectedHumidId = humid.Id",
                "JOIN Logistics.Recipient rec ON deliv.RecipientId = rec.Id",
                "JOIN Customers.Company recCom ON rec.CompanyId = recCom.Id",
                "JOIN Logistics.Sender sen ON deliv.SenderId = sen.Id",
                "JOIN Customers.Company senCom ON sen.CompanyId = senCom.Id",
                "JOIN Logistics.Carrier carr ON deliv.CarrierId = carr.Id",
                "JOIN Customers.Company carrCom ON carr.CompanyId = carrCom.Id"
            },
            filters: filters,
            map: r => new DeliveryDto(
                Id: Convert.ToInt32(r["Id"]),
                RouteId: Convert.ToString(r["troute.Id"]),
                ExpectedTempId: Convert.ToSingle(r["ExpectedTempId"]),
                ExpectedHumidId: Convert.ToSingle(r["ExpectedHumidId"]),
                minMaxTemp: Convert.ToSingle(r["minMaxTemp"]),
                minMaxHumid: Convert.ToSingle(r["minMaxHumid"]),
                CarrierId: Convert.ToString(r["CarrierId"]),
                SenderId: Convert.ToString(r["SenderId"]),
                RecipientId: Convert.ToString(r["RecipientId"]),
                StateId: Convert.ToString(r["StateId"]),
                OrderPlaced: Convert.ToDateTime(r["OrderPlaced"])
            )

            /* DTO
                    int Id,
                    string RouteId,
                    float ExpectedTempId,
                    float ExpectedHumidId,
                    float minMaxTemp,
                    float minMaxHumid,
                    string CarrierId,
                    string SenderId,
                    string RecipientId,
                    string StateId,
                    DateTime OrderPlaced
        */
        );

        return result.Any() ? Ok(result) : NotFound("No delivery records found.");
    }
}

