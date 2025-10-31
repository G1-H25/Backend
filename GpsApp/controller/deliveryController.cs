using Microsoft.AspNetCore.Mvc;
using GpsApp.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System.ComponentModel.Design;

/// <summary>
/// Delivery management controller using dependency inversion principle.
/// </summary>
/// <remarks>
/// Manages delivery operations with comprehensive sensor monitoring.
/// Demonstrates SOLID: SRP (delivery management), OCP (interface-based), LSP (substitutable),
/// ISP (focused interfaces), DIP (abstraction over concretions).
/// Constructor injection enables testing and loose coupling.
/// </remarks>
[ApiController]
[Route("[controller]")]
public class DeliveryController : ControllerBase
{
    private readonly ISqlInsert _insertService;
    private readonly ISqlGetAdvanced _sqlAdvanced;
    private readonly ISqlGet _sqlGet;
    private readonly IAuthorizationService _authService;
    private readonly ISqlUpdate _sqlUpdate;

    /// <summary>
    /// Initializes delivery controller with injected dependencies.
    /// </summary>
    /// <param name="insertService">Database insertion service (ISqlInsert).</param>
    /// <param name="authService">Authorization service for access validation.</param>
    /// <param name="sqlGetAdvanced">Advanced query service with JOINs (ISqlGetAdvanced).</param>
    /// <param name="sqlGet">Basic query service (ISqlGet).</param>
    /// <param name="sqlUpdate">Database update service (ISqlUpdate).</param>
    /// <remarks>Constructor injection - dependencies provided by DI container for testability.</remarks>
    public DeliveryController(ISqlInsert insertService, IAuthorizationService authService, ISqlGetAdvanced sqlGetAdvanced, ISqlGet sqlGet, ISqlUpdate sqlUpdate)
    {
        _insertService = insertService;
        _authService = authService;
        _sqlAdvanced = sqlGetAdvanced;
        _sqlGet = sqlGet;
        _sqlUpdate = sqlUpdate;
    }

    /// <summary>
    /// Retrieves a list of deliveries, optionally filtered by delivery ID.
    /// </summary>
    /// <param name="id">Optional delivery ID to filter by a specific delivery record.</param>
    /// <returns>
    /// Returns an <see cref="OkObjectResult"/> containing an array of deliveries if records are found,
    /// or <see cref="NotFoundResult"/> if no records match the filter.
    /// </returns>
    /// <remarks>
    /// This route retrieves one or more delivery records with associated route, sensor, carrier,
    /// and company information.  
    /// If <paramref name="id"/> is provided, the result will contain one record in an array.
    /// Otherwise, all delivery records will be returned.
    /// </remarks>
    [HttpGet("retrieveDeliveries")]
    [Authorize]
    public async Task<IActionResult> GetDeliveries([FromQuery] int? id)
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
                        sensTemp.Min AS ExpectedTempMin,
                        sensTemp.Max AS ExpectedTempMax,
                        sens.TempMinMeasured AS TempMinMeasured,
                        sens.TempMaxMeasured AS TempMaxMeasured,
                        sens.TempTimeOutside AS TempOutOfRange,
                        sens.HumdityPct AS CurrentHumid,
                        sensHumid.Min AS ExpectedHumidMin,
                        sensHumid.Max AS ExpectedHumidMax,
                        sens.HumidMinMeasured AS HumidMinMeasured,
                        sens.HumidMaxMeasured AS HumidMaxMeasured,
                        sens.HumidTimeOutside AS HumidOutOfRange,
                        carrCom.CompanyName AS Carrier,
                        senCom.CompanyName AS Sender,
                        recCom.CompanyName AS Recipient,
                        delstate.CurrentState AS CurrentState,
                        delstate.UpdatedAt AS StatusUpdated,
                        deliv.OrderPlaced
            ",
        /*
                JOIN Measurements.Sensor sens ON deliv.SensorId = sens.Id
        JOIN Measurements.ExpectedTemp sensTemp ON sens.Id = sensTemp.Id
        JOIN Measurements.ExpectedHumid sensHumid ON sens.Id = sensTemp.Id
        */
            joins: new List<string>
            {
                    "JOIN Orders.DeliveryState delstate ON deliv.Id = delstate.DeliveryId",
                    "JOIN Logistics.TransportRoute troute ON deliv.RouteId = troute.Id",
                    "JOIN Measurements.Sensor sens ON deliv.SensorId = sens.Id",
                    "JOIN Measurements.ExpectedTemp sensTemp ON sens.Id = sensTemp.Id",
                    "JOIN Measurements.ExpectedHumid sensHumid ON sens.Id = sensHumid.Id",
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
                OrderPlaced: Convert.ToDateTime(r["OrderPlaced"]),
                Status: new StatusDto(
                    Text: Convert.ToString(r["CurrentState"]),
                    Timestamp: Convert.ToDateTime(r["StatusUpdated"]).ToString("o")
                )
            )
        );

        return result.Any() ? Ok(result) : NotFound("No delivery records found.");
    }
    
    /// <summary>
    /// Retrieves a single delivery by its unique ID, with optional filters for route, sender, recipient, carrier, or sensor.
    /// </summary>
    /// <param name="id">The unique identifier of the delivery.</param>
    /// <param name="routeId">Optional filter by transport route ID.</param>
    /// <param name="senderId">Optional filter by sender ID.</param>
    /// <param name="recipientId">Optional filter by recipient ID.</param>
    /// <param name="carrierId">Optional filter by carrier ID.</param>
    /// <param name="sensorId">Optional filter by sensor ID.</param>
    /// <returns>
    /// Returns an <see cref="OkObjectResult"/> containing a single delivery record if found,  
    /// or a <see cref="NotFoundResult"/> if no delivery matches the given filters.
    /// </returns>
    /// <remarks>
    /// Use this route to fetch details for a specific delivery.  
    /// You may optionally restrict the search by route, sender, recipient, carrier, or sensor.
    /// This route returns **one object** (not an array).
    /// </remarks>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetDeliveryById(
        int id,
        [FromQuery] int? routeId,
        [FromQuery] int? senderId,
        [FromQuery] int? recipientId,
        [FromQuery] int? carrierId,
        [FromQuery] int? sensorId)
    {
        // --- Build dynamic filters ---
        var filters = new Dictionary<string, object>
        {
            { "deliv.Id", id }
        };

        if (routeId.HasValue)
            filters.Add("deliv.RouteId", routeId.Value);

        if (senderId.HasValue)
            filters.Add("deliv.SenderId", senderId.Value);

        if (recipientId.HasValue)
            filters.Add("deliv.RecipientId", recipientId.Value);

        if (carrierId.HasValue)
            filters.Add("deliv.CarrierId", carrierId.Value);

        if (sensorId.HasValue)
            filters.Add("deliv.SensorId", sensorId.Value);

        // --- Query ---
        var result = await _sqlAdvanced.FetchWithJoinsAsync(
            baseTable: "Orders.Delivery deliv",
            selectClause: @"
                deliv.Id AS DeliveryId,
                troute.Code AS RouteCode,
                sens.TemperatureCel AS CurrentTemp,
                sensTemp.Min AS ExpectedTempMin,
                sensTemp.Max AS ExpectedTempMax,
                sens.TempMinMeasured AS TempMinMeasured,
                sens.TempMaxMeasured AS TempMaxMeasured,
                sens.TempTimeOutside AS TempOutOfRange,
                sens.HumdityPct AS CurrentHumid,
                sensHumid.Min AS ExpectedHumidMin,
                sensHumid.Max AS ExpectedHumidMax,
                sens.HumidMinMeasured AS HumidMinMeasured,
                sens.HumidMaxMeasured AS HumidMaxMeasured,
                sens.HumidTimeOutside AS HumidOutOfRange,
                carrCom.CompanyName AS Carrier,
                senCom.CompanyName AS Sender,
                recCom.CompanyName AS Recipient,
                delstate.CurrentState AS CurrentState,
                delstate.UpdatedAt AS StatusUpdated,
                deliv.OrderPlaced
            ",
            joins: new List<string>
            {
                "JOIN Orders.DeliveryState delstate ON deliv.Id = delstate.DeliveryId",
                "JOIN Logistics.TransportRoute troute ON deliv.RouteId = troute.Id",
                "JOIN Measurements.Sensor sens ON deliv.SensorId = sens.Id",
                "JOIN Measurements.ExpectedTemp sensTemp ON sens.Id = sensTemp.Id",
                "JOIN Measurements.ExpectedHumid sensHumid ON sens.Id = sensHumid.Id",
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
                OrderPlaced: Convert.ToDateTime(r["OrderPlaced"]),
                Status: new StatusDto(
                    Text: Convert.ToString(r["CurrentState"]),
                    Timestamp: Convert.ToDateTime(r["StatusUpdated"]).ToString("o")
                )
            )
        );

        var delivery = result.FirstOrDefault();
        return delivery != null ? Ok(delivery) : NotFound($"Delivery with ID {id} not found.");
    }

    /*
    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> CreateDelivery([FromBody] DeliveryCreateRequest request)
    {
        // Validate IDs upfront
        if (request.RouteId <= 0 || request.SensorId <= 0 ||
            request.RecipientId <= 0 || request.SenderId <= 0 ||
            request.CarrierId <= 0)
        {
            return BadRequest("Missing or invalid IDs.");
        }

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

        // Insert delivery record
        var deliveryValues = new Dictionary<string, object>
    {
        { "RouteId", request.RouteId },
        { "SensorId", request.SensorId },
        { "RecipientId", request.RecipientId },
        { "SenderId", request.SenderId },
        { "CarrierId", request.CarrierId },
        { "OrderPlaced", request.OrderPlaced }
    };

        int deliveryId = await _insertService.InsertAndReturnIdAsync("Orders.Delivery", deliveryValues);

        return Ok(new { message = "Delivery created", deliveryId });
    }
    */


}

