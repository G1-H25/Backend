using Microsoft.AspNetCore.Mvc;
using GpsApp.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System.ComponentModel.Design;

[ApiController]
[Route("[controller]")]
public class DeliveryController : ControllerBase
{
    private readonly SqlInsert _insertService;
    private readonly ISqlGetAdvanced _sqlAdvanced;
    private readonly ISqlGet _sqlGet;
    private readonly IAuthorizationService _authService;

    // get the connectionstring to azure database, authorization access
    public DeliveryController(SqlInsert insertService, IAuthorizationService authService, ISqlGetAdvanced sqlGetAdvanced, ISqlGet sqlGet)
    {
        _insertService = insertService;
        _authService = authService;
        _sqlAdvanced = sqlGetAdvanced;
        _sqlGet = sqlGet;
    }

    [HttpGet("retrieve")]
    public async Task<IActionResult> GetDelivery([FromQuery] int? id)
    {

        var filters = new Dictionary<string, object>();
        if (id.HasValue)
            filters.Add("deliv.Id", id.Value);

        var result = await _sqlAdvanced.FetchWithJoinsAsync(
            baseTable: "Orders.Delivery deliv",
            selectClause: @"
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
            ",
            joins: new List<string>
            {   
                    "JOIN Orders.DeliveryState delstate ON deliv.Id = delstate.Id",
                    "JOIN Logistics.TransportRoute troute ON deliv.RouteId = troute.Id",
                    "JOIN Measurements.Sensor sens ON deliv.SensorId = sens.Id",
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
                DeliveryId: Convert.ToInt32(r["DeliveryId"]),
                RouteCode: Convert.ToString(r["RouteCode"]),
                CurrentTemp: Convert.ToSingle(r["CurrentTemp"]),
                ExpectedTempMin: Convert.ToSingle(r["ExpectedTempMin"]),
                ExpectedTempMax: Convert.ToSingle(r["ExpectedTempMax"]),
                TempMinMeasured: Convert.ToSingle(r["TempMinMeasured"]),
                TempMaxMeasured: Convert.ToSingle(r["TempMaxMeasured"]),
                TempOutOfRange: Convert.ToSingle(r["TempOutOfRange"]),
                CurrentHumid: Convert.ToSingle(r["CurrentHumid"]),
                ExpectedHumidMin: Convert.ToSingle(r["ExpectedHumidMin"]),
                ExpectedHumidMax: Convert.ToSingle(r["ExpectedHumidMax"]),
                HumidMinMeasured: Convert.ToSingle(r["HumidMinMeasured"]),
                HumidMaxMeasured: Convert.ToSingle(r["HumidMaxMeasured"]),
                HumidOutOfRange: Convert.ToSingle(r["HumidOutOfRange"]),
                Carrier: Convert.ToString(r["Carrier"]),
                Sender: Convert.ToString(r["Sender"]),
                Recipient: Convert.ToString(r["Recipient"]),
                OrderPlaced: Convert.ToDateTime(r["OrderPlaced"])
            )
        );


/*
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

*/
        return result.Any() ? Ok(result) : NotFound("No delivery records found.");
    }
    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> CreateDelivery([FromBody] DeliveryCreateRequest request)
    {
        var sensor = await _sqlGet.FetchAsync("Measurements.Sensor", new Dictionary<string, object>
        {
            { "Id", request.SensorId }
        });

        if (sensor == null)
            return NotFound("Sensor not found.");

        if (!sensor.ContainsKey("GatewayId"))
            return BadRequest("Sensor has no associated gateway.");

        int gatewayId = Convert.ToInt32(sensor["GatewayId"]);

        bool canAccess = await _authService.UserCanAccessDevice(User, gatewayId);
        if (!canAccess)
            return Forbid("You do not have access to this sensor's gateway.");


        // Validate required fields (add more validations as needed)
        if (request.RouteId <= 0 || request.SensorId <= 0 || 
            request.RecipientId <= 0 || request.SenderId <= 0 || request.CarrierId <= 0)
            return BadRequest("Missing or invalid IDs.");

        // 1. Insert ExpectedTemp
        var expectedTempValues = new Dictionary<string, object>
        {
            { "Note", request.ExpectedTemp.Note },
            { "Min", request.ExpectedTemp.Min },
            { "Max", request.ExpectedTemp.Max }
        };
        int expectedTempId = await _insertService.InsertAndReturnIdAsync("Measurements.ExpectedTemp", expectedTempValues);

        // 2. Insert ExpectedHumid
        var expectedHumidValues = new Dictionary<string, object>
        {
            { "Note", request.ExpectedHumid.Note },
            { "Min", request.ExpectedHumid.Min },
            { "Max", request.ExpectedHumid.Max }
        };
        int expectedHumidId = await _insertService.InsertAndReturnIdAsync("Measurements.ExpectedHumid", expectedHumidValues);

        // 3. Insert Delivery
        var deliveryValues = new Dictionary<string, object>
        {
            { "RouteId", request.RouteId },
            { "SensorId", request.SensorId },
            { "ExpectedTempId", expectedTempId },
            { "ExpectedHumidId", expectedHumidId },
            { "RecipientId", request.RecipientId },
            { "SenderId", request.SenderId },
            { "CarrierId", request.CarrierId },
            { "OrderPlaced", request.OrderPlaced }
        };

        int deliveryId = await _insertService.InsertAndReturnIdAsync("Orders.Delivery", deliveryValues);

        return Ok(new { message = "Delivery created", deliveryId });
    }
}

