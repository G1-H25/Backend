using GpsApp.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;



[ApiController]
[Route("[controller]")]
public class SensorController : ControllerBase
{
    private readonly SqlInsert _insertService;
    private readonly ISqlGetAdvanced _sqlGetAdvanced;
    private readonly ISqlGet _sqlGet;
    private readonly IAuthorizationService _authService;

    // get the connectionstring to azure database, authorization access
    public SensorController(SqlInsert insertService, IAuthorizationService authService, ISqlGetAdvanced sqlGetAdvanced, ISqlGet sqlGet)
    {
        _insertService = insertService;
        _authService = authService;
        _sqlGetAdvanced = sqlGetAdvanced;
        _sqlGet = sqlGet;
    }

    /// <summary>
    /// Adds new Sensor data.
    /// </summary>
    /// <param name="data">The Sensor data to insert.</param>
    /// <returns>Returns OK if inserted successfully.</returns>
    /// <remarks>
    /// 
    /// </remarks>
    [HttpPost]
    [Authorize] // Require JWT
    public async Task<IActionResult> PostSensorData([FromBody] SensorDto data)
    {
        // declare temporary variables
        int tempTimeOutside;
        DateTime? tempTimerStart;
        int humidTimeOutside;
        DateTime? humidTimerStart;
        //  1. Validate user access to the specified GatewayId via JWT-based authorization
        var canAccess = await _authService.UserCanAccessDevice(User, data.GatewayId);
        if (!canAccess)
            return Forbid("You do not have access to this gateway.");

        //  2. Validate input fields
        if (data.GatewayId <= 0)
            return BadRequest("GatewayId must be a positive integer.");

        if (data.PolledAt == default)
            data.PolledAt = DateTime.UtcNow; // Default to current UTC time if none provided

        if (data.TemperatureCel == null)
            return BadRequest("TemperatureCel must be provided.");

        if (data.HumdityPct == null)
            return BadRequest("HumdityPct must be provided.");

        //  3. Fetch the latest sensor reading for this gateway to compare previous state
        var readings = await _sqlGetAdvanced.FetchWithJoinsAsync(
            baseTable: "Measurements.Sensor sensor",
            selectClause: @"
            sensor.TempTimerStart,
            sensor.TempTimeOutside,
            sensor.HumidTimerStart,
            sensor.HumidTimeOutside,
            sensor.TemperatureCel,
            sensor.HumdityPct,
            sensor.PolledAt
        ",
            joins: new List<string>(), // No JOINs needed — just the same table
            filters: new Dictionary<string, object> {
            { "sensor.GatewayId", data.GatewayId }
            },
            map: r => new
            {
                TempTimerStart = r["TempTimerStart"] as DateTime?,
                TempTimeOutside = r["TempTimeOutside"] == DBNull.Value ? 0 : Convert.ToInt32(r["TempTimeOutside"]),
                HumidTimerStart = r["HumidTimerStart"] as DateTime?,
                HumidTimeOutside = r["HumidTimeOutside"] == DBNull.Value ? 0 : Convert.ToInt32(r["HumidTimeOutside"]),
                PolledAt = Convert.ToDateTime(r["PolledAt"])
            }
        );

        //  4. Get the most recent reading (i.e. latest by PolledAt)
        var lastReading = readings
            .OrderByDescending(r => r.PolledAt)
            .FirstOrDefault();

        // Use the timestamp of the current reading as reference
        var now = data.PolledAt;

        //  5. Handle temperature out-of-range logic
        //    If outside 10-30°C, track how long it has been out of range (LOGIC HERE WILL NEED TO CHANGE TO USE DATA MIN/MAX INSTEAD)
        bool tempOutOfRange = data.TemperatureCel < 10 || data.TemperatureCel > 30;

        if (tempOutOfRange)
        {
            if (lastReading?.TempTimerStart != null)
            {
                // Already out of range previously — continue counting
                tempTimerStart = lastReading.TempTimerStart;
                tempTimeOutside = (int)(now - lastReading.TempTimerStart.Value).TotalSeconds;
            }
            else
            {
                // Just went out of range now — start new timer
                tempTimerStart = now;
                tempTimeOutside = 0;
            }
        }
        else
        {
            // Back within range — reset timer and duration
            tempTimerStart = null;
            tempTimeOutside = 0;
        }

        //  6. Handle humidity out-of-range logic
        //    If outside 20–80%, track how long it has been out of range (LOGIC HERE WILL NEED TO CHANGE TO USE DATA MIN/MAX INSTEAD)
        bool humidOutOfRange = data.HumdityPct < 20 || data.HumdityPct > 80;

        if (humidOutOfRange)
        {
            if (lastReading?.HumidTimerStart != null)
            {
                // Already out of range — continue counting
                humidTimerStart = lastReading.HumidTimerStart;
                humidTimeOutside = (int)(now - lastReading.HumidTimerStart.Value).TotalSeconds;
            }
            else
            {
                // Just went out of range — start new timer
                humidTimerStart = now;
                humidTimeOutside = 0;
            }
        }
        else
        {
            // Back within range — reset humidity timer
            humidTimerStart = null;
            humidTimeOutside = 0;
        }

        //  7. Prepare the data dictionary for insertion
        var dataDict = new Dictionary<string, object>
    {
        { "GatewayId", data.GatewayId },
        { "PolledAt", data.PolledAt },
        { "TemperatureCel", data.TemperatureCel },
        { "HumdityPct", data.HumdityPct },
        { "TempTimeOutside", tempTimeOutside },
        { "HumidTimeOutside", humidTimeOutside },
        { "TempTimerStart", tempTimerStart },
        { "HumidTimerStart", humidTimerStart }
    };

        //  8. Insert new sensor record into the database
        await _insertService.InsertAsync("Measurements.Sensor", dataDict);

        //  9. Return success response
        return Ok("Inserted");
    }


    [HttpGet("available")]
    [Authorize]
    public async Task<IActionResult> GetAvailableSensors()
    {
        var companyId = await _authService.GetCompanyIdFromClaims(User);
        var userId = await _authService.GetUserIdFromClaims(User);
        var role = await _authService.GetUserRoleFromClaims(User);

        if (companyId == null || userId == null || string.IsNullOrEmpty(role))
            return Forbid("Invalid user claims.");

        var baseTable = "Measurements.Sensor s";
        var selectClause = "s.Id, s.GatewayId, s.PolledAt, s.TemperatureCel, s.HumdityPct";
        var joins = new List<string>
        {
            "JOIN Secrets.Gateway g ON s.GatewayId = g.Id",
            "JOIN Secrets.Account a ON a.Id = g.UserId"
        };

        Dictionary<string, object> filters;

        if (role == "Admin")
        {
            // Admin: get all sensors in company
            filters = new Dictionary<string, object>
            {
                { "a.CompanyId", companyId.Value }
            };
        }
        else
        {
            // Regular user: get only sensors owned by user
            filters = new Dictionary<string, object>
            {
                { "g.UserId", userId.Value }
            };
        }

        var sensors = await _sqlGetAdvanced.FetchWithJoinsAsync<Dictionary<string, object>>(
            baseTable,
            selectClause,
            joins,
            filters
        );

        return Ok(sensors);
    }

    /// <summary>
    /// Retrieves sensor temperature data.
    /// </summary>
    /// <param name="id">Optional sensor Id to filter by a specific sensor.</param>
    /// <param name="temperature">Optional exact temperature value to filter sensor readings.</param>
    /// <param name="temperatureFrom">Optional minimum temperature value to filter sensor readings.</param>
    /// <param name="temperatureTo">Optional maximum temperature value to filter sensor readings.</param>
    /// <returns>
    /// Returns an <see cref="OkObjectResult"/> containing a list of sensor temperature data matching the filter criteria,
    /// or <see cref="NotFoundResult"/> if no records are found.
    /// </returns>
    /// <remarks>
    /// The route supports filtering sensor data by sensor Id, exact temperature, or a temperature range using temperatureFrom and temperatureTo.
    /// If no filters are provided, all sensor temperature data will be returned.
    /// </remarks>
    [HttpGet("sensor-temperature")]
    public async Task<IActionResult> GetSensorTemperature(
        [FromQuery] int? id,
        [FromQuery] decimal? temperature,
        [FromQuery] decimal? temperatureFrom,
        [FromQuery] decimal? temperatureTo)
    {
        var filters = new Dictionary<string, object>();

        if (id.HasValue)
            filters.Add("sensor.Id", id.Value);

        if (temperature.HasValue)
            filters.Add("sensor.TemperatureCel", temperature.Value);

        if (temperatureFrom.HasValue && temperatureTo.HasValue)
        {
            filters.Add("sensor.TemperatureCel >= ", temperatureFrom.Value);
            filters.Add("sensor.TemperatureCel <= ", temperatureTo.Value);
        }
        else if (temperatureFrom.HasValue)
        {
            filters.Add("sensor.TemperatureCel >= ", temperatureFrom.Value);
        }
        else if (temperatureTo.HasValue)
        {
            filters.Add("sensor.TemperatureCel <= ", temperatureTo.Value);
        }

        var result = await _sqlGetAdvanced.FetchWithJoinsAsync(
            baseTable: "Measurements.Sensor sensor",
            selectClause: @"
                sensor.Id AS SensorId,
                sensor.GatewayId,
                sensor.PolledAt,
                sensor.TemperatureCel
            ",
            joins: new List<string>(),
            filters: filters,
            map: r => new SensorTemperatureRequest(
                SensorId: Convert.ToInt32(r["SensorId"]),
                GatewayId: Convert.ToInt32(r["GatewayId"]),
                PolledAt: Convert.ToDateTime(r["PolledAt"]),
                TemperatureCel: r["TemperatureCel"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["TemperatureCel"])
            )
        );

        return result.Any() ? Ok(result) : NotFound("No sensor temperature records found.");
    }
    /// <summary>
    /// Retrieves sensor humidity data.
    /// </summary>
    /// <param name="id">Optional sensor Id to filter by a specific sensor.</param>
    /// <param name="humidity">Optional exact humidity value to filter sensor readings.</param>
    /// <param name="humidityFrom">Optional minimum humidity value to filter sensor readings.</param>
    /// <param name="humidityTo">Optional maximum humidity value to filter sensor readings.</param>
    /// <returns>
    /// Returns an <see cref="OkObjectResult"/> containing a list of sensor humidity data matching the filter criteria,
    /// or <see cref="NotFoundResult"/> if no records are found.
    /// </returns>
    /// <remarks>
    /// The route supports filtering sensor data by sensor Id, exact humidity, or a humidity range using humidityFrom and humidityTo.
    /// If no filters are provided, all sensor humidity data will be returned.
    /// </remarks>
    [HttpGet("sensor-humidity")]
    public async Task<IActionResult> GetSensorHumidity(
        [FromQuery] int? id,
        [FromQuery] decimal? humidity,
        [FromQuery] decimal? humidityFrom,
        [FromQuery] decimal? humidityTo)
    {
        var filters = new Dictionary<string, object>();

        if (id.HasValue)
            filters.Add("sensor.Id", id.Value);

        if (humidity.HasValue)
            filters.Add("sensor.HumdityPct", humidity.Value);

        if (humidityFrom.HasValue && humidityTo.HasValue)
        {
            filters.Add("sensor.HumdityPct >= ", humidityFrom.Value);
            filters.Add("sensor.HumdityPct <= ", humidityTo.Value);
        }
        else if (humidityFrom.HasValue)
        {
            filters.Add("sensor.HumdityPct >= ", humidityFrom.Value);
        }
        else if (humidityTo.HasValue)
        {
            filters.Add("sensor.HumdityPct <= ", humidityTo.Value);
        }

        var result = await _sqlGetAdvanced.FetchWithJoinsAsync(
            baseTable: "Measurements.Sensor sensor",
            selectClause: @"
                sensor.Id AS SensorId,
                sensor.GatewayId,
                sensor.PolledAt,
                sensor.HumdityPct
            ",
            joins: new List<string>(),
            filters: filters,
            map: r => new SensorHumidityDto(
                SensorId: Convert.ToInt32(r["SensorId"]),
                GatewayId: Convert.ToInt32(r["GatewayId"]),
                PolledAt: Convert.ToDateTime(r["PolledAt"]),
                HumidityPct: r["HumdityPct"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["HumdityPct"])
            )
        );

        return result.Any() ? Ok(result) : NotFound("No sensor humidity records found.");
    }




}

