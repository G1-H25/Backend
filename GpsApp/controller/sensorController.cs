using GpsApp.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GpsApp.Services;
using Microsoft.Data.SqlClient;



/// <summary>
/// Sensor data management controller using dependency inversion principle.
/// </summary>
/// <remarks>
/// Core GPS tracking system controller handling real-time sensor data (temperature, humidity).
/// Demonstrates SOLID principles: SRP (data management), OCP (interface-based), LSP (substitutable),
/// ISP (focused interfaces), DIP (abstraction over concretions).
/// Dependencies injected via constructor - testable and decoupled design.
/// </remarks>
[ApiController]
[Route("[controller]")]
public class SensorController : ControllerBase
{
    private readonly ISqlInsert _insertService;
    private readonly ISqlGetAdvanced _sqlGetAdvanced;
    private readonly ISqlGet _sqlGet;
    private readonly IAuthorizationService _authService;
    private readonly ISqlUpdate _sqlUpdate;
    private readonly ISensorValidationService _validationService;

    /// <summary>
    /// Initializes sensor controller with injected dependencies.
    /// </summary>
    /// <param name="insertService">Database insertion service (ISqlInsert).</param>
    /// <param name="authService">Authorization service for access validation.</param>
    /// <param name="sqlGetAdvanced">Advanced query service with JOINs (ISqlGetAdvanced).</param>
    /// <param name="sqlGet">Basic query service (ISqlGet).</param>
    /// <param name="sqlUpdate">Database update service (ISqlUpdate).</param>
    /// <param name="validationService">Sensor data validation service (ISensorValidationService).</param>
    /// <remarks>Constructor injection - dependencies provided by DI container for testability.</remarks>
    public SensorController(ISqlInsert insertService, IAuthorizationService authService, ISqlGetAdvanced sqlGetAdvanced, ISqlGet sqlGet, ISqlUpdate sqlupdate, ISensorValidationService validationService)
    {
        _insertService = insertService;
        _authService = authService;
        _sqlGetAdvanced = sqlGetAdvanced;
        _sqlGet = sqlGet;
        _sqlUpdate = sqlupdate;
        _validationService = validationService;
    }

    /// <summary>
    /// Inserts or updates sensor data for a specified gateway and sensor device.
    /// </summary>
    /// <param name="data">Sensor reading data (gateway ID, UUID, optional Timestamp, Temperature Value, Humidity Value.</param>
    /// <returns>
    /// Returns:
    /// <list type="bullet">
    /// <item><description>200 OK with confirmation message if the data was successfully inserted or updated.</description></item>
    /// <item><description>400 Bad Request if validation fails (invalid GatewayId, missing fields, or gateway does not exist).</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This endpoint performs the following steps:
    /// <para>1. Validates required fields in <see cref="SensorDto"/>:</para>
    /// <list type="bullet">
    ///   <item><description><c>GatewayId</c> must be a positive integer.</description></item>
    ///   <item><description><c>TemperatureCel</c> and <c>HumdityPct</c> must be non-null.</description></item>
    ///   <item><description><c>UUID</c> must be a valid, non-empty GUID.</description></item>
    /// </list>
    /// <para>3. Confirms that the specified gateway exists in the database; otherwise, returns a 400 error.</para>
    /// <para>4. Retrieves the most recent sensor reading  for the given gateway and sensor <c>UUID</c> to compare previous sensor state and summarization data.</para>
    /// <para>5. Calculates:</para>
    /// <list type="bullet">
    ///   <item><description>Total time the temperature and humidity have been outside predefined acceptable ranges (temperature: 10–30°C, humidity: 20–80% (Currently Mocked)).</description></item>
    ///   <item><description>Updated timer start timestamps for both temperature and humidity, tracking when values go out of range.</description></item>
    ///   <item><description>The lowest and highest recorded temperature and humidity values to date (min/max tracking).</description></item>
    /// </list>
    /// <para>6. If an existing sensor record is found (matched by <c>GatewayId</c> and <c>UUID</c>), the endpoint performs an <b>UPDATE</b> on the live sensor fields (<c>TemperatureCel</c>, <c>HumdityPct</c>, <c>PolledAt</c>) and can optionally update summarized data.</para>
    /// <para>7. If no previous sensor record exists, a new record is <b>INSERTED</b> into <c>Measurements.Sensor</c> with all calculated values.</para>
    /// <para>8. Returns 200 OK with confirmation on success.</para>
    /// <para>
    /// <b>Important:</b> The sensor <c>UUID</c> is used to identify individual sensor devices attached to gateways.
    /// Multiple sensors can belong to a single gateway. Updates and inserts are based on <c>GatewayId</c> + <c>UUID</c>.
    /// </para>
    /// <para>
    ///
    /// This route maintains live sensor values and aggregates summary data in one record per sensor while tracking historical min/max and out-of-range durations.
    /// </para>
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes("application/json")]
    [Produces("application/json")]
    // [Authorize] // Require JWT
    public async Task<IActionResult> PostSensorData([FromBody] SensorDto data)
    {
        // declare temporary variables
        int tempTimeOutside;
        DateTime? tempTimerStart;
        int humidTimeOutside;
        DateTime? humidTimerStart;

        //  1. Validate user access to the specified GatewayId via JWT-based authorization
        // var canAccess = await _authService.UserCanAccessDevice(User, data.GatewayId);
        // if (!canAccess)
        //    return Forbid("You do not have access to this gateway.");


        //  2. Validate input fields using domain validation service
        var validationResult = _validationService.ValidateSensorDto(data);
        if (!validationResult.IsValid)
        {
            return BadRequest(string.Join("; ", validationResult.Errors));
        }

        if (data.PolledAt == default)
            data.PolledAt = DateTime.UtcNow; // Default to current UTC time if none provided

        // Fetch GatewayId from DB using GatewayUUID in order to insert gatewayID as an integer later
        var gatewayRecord = await _sqlGet.FetchAsync("Secrets.Gateway", new Dictionary<string, object>
        {
            { "UUID", data.GatewayUUID }
        });

        if (gatewayRecord == null || !gatewayRecord.Any())
        {
            return BadRequest($"Gateway with UUID {data.GatewayUUID} does not exist.");
        }

        int gatewayId = Convert.ToInt32(gatewayRecord["Id"]);


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
            sensor.PolledAt,
            sensor.TempMinMeasured,
            sensor.TempMaxMeasured,
            sensor.HumidMinMeasured,
            sensor.HumidMaxMeasured
        ",
            joins: new List<string>(), // No JOINs needed — just the same table
            filters: new Dictionary<string, object> {
            { "sensor.GatewayId", gatewayId }
            },
            map: r => new
            {
                TempTimerStart = r["TempTimerStart"] as DateTime?,
                TempTimeOutside = r["TempTimeOutside"] == DBNull.Value ? 0 : Convert.ToInt32(r["TempTimeOutside"]),
                HumidTimerStart = r["HumidTimerStart"] as DateTime?,
                HumidTimeOutside = r["HumidTimeOutside"] == DBNull.Value ? 0 : Convert.ToInt32(r["HumidTimeOutside"]),
                PolledAt = Convert.ToDateTime(r["PolledAt"]),
                TempMinMeasured = r["TempMinMeasured"] == DBNull.Value ? data.TemperatureCel.Value : Convert.ToSingle(r["TempMinMeasured"]),
                TempMaxMeasured = r["TempMaxMeasured"] == DBNull.Value ? data.TemperatureCel.Value : Convert.ToSingle(r["TempMaxMeasured"]),
                HumidMinMeasured = r["HumidMinMeasured"] == DBNull.Value ? data.HumdityPct.Value : Convert.ToSingle(r["HumidMinMeasured"]),
                HumidMaxMeasured = r["HumidMaxMeasured"] == DBNull.Value ? data.HumdityPct.Value : Convert.ToSingle(r["HumidMaxMeasured"]),
            }
        );

        //  4. Get the most recent reading (i.e. latest by PolledAt)
        var lastReading = readings
            .OrderByDescending(r => r.PolledAt)
            .FirstOrDefault();

        // set the lowest and highest temperature value, that has been ever recorded on the sensor
        float tempMinMeasured = Math.Min(data.TemperatureCel.Value, lastReading?.TempMinMeasured ?? data.TemperatureCel.Value);
        float tempMaxMeasured = Math.Max(data.TemperatureCel.Value, lastReading?.TempMaxMeasured ?? data.TemperatureCel.Value);

        float humidMinMeasured = Math.Min(data.HumdityPct.Value, lastReading?.HumidMinMeasured ?? data.HumdityPct.Value);
        float humidMaxMeasured = Math.Max(data.HumdityPct.Value, lastReading?.HumidMaxMeasured ?? data.HumdityPct.Value);

        // Use the timestamp of the current reading as reference
        var now = data.PolledAt;

        //  5. Handle temperature out-of-range logic
        (tempTimeOutside, tempTimerStart) = TrackingTimeOutsideRange.TrackTimeOutsideRange(
            currentValue: data.TemperatureCel.Value,
            expectedMin: 10,
            expectedMax: 30,
            currentTimestamp: data.PolledAt,
            lastTimerStart: lastReading?.TempTimerStart
        );

        //  6. Handle Humid out-of-range logic
        (humidTimeOutside, humidTimerStart) = TrackingTimeOutsideRange.TrackTimeOutsideRange(
        currentValue: data.HumdityPct.Value,
        expectedMin: 20,
        expectedMax: 80,
        currentTimestamp: data.PolledAt,
        lastTimerStart: lastReading?.HumidTimerStart
        );

        //  7. Prepare the data dictionary for insertion

        var dataDict = new Dictionary<string, object>
    {
        { "GatewayId", gatewayId },
        { "UUID", data.UUID },
        { "PolledAt", data.PolledAt },
        { "TemperatureCel", data.TemperatureCel },
        { "HumdityPct", data.HumdityPct },
        { "TempTimeOutside", tempTimeOutside },
        { "HumidTimeOutside", humidTimeOutside },
        { "TempTimerStart", tempTimerStart },
        { "HumidTimerStart", humidTimerStart },
        { "TempMinMeasured", tempMinMeasured },
        { "TempMaxMeasured", tempMaxMeasured },
        { "HumidMinMeasured", humidMinMeasured },
        { "HumidMaxMeasured", humidMaxMeasured }
    };

        // Fetch existing sensor record by GatewayId + UUID
        var existingRecord = await _sqlGet.FetchAsync("Measurements.Sensor", new Dictionary<string, object>
    {
        { "GatewayId", gatewayId },
        { "UUID", data.UUID }
    });

        if (existingRecord != null)
        {
            // Update live data fields only
            var updateDict = new Dictionary<string, object>
        {
            { "TemperatureCel", data.TemperatureCel },
            { "HumdityPct", data.HumdityPct },
            { "PolledAt", data.PolledAt }
            // Add more fields here if you want to update summarized data on update
        };

            await _sqlUpdate.UpdateAsync("Measurements.Sensor", updateDict, new Dictionary<string, object>
        {
            { "Id", existingRecord["Id"] }
        });
        }
        else
        {
            //  8. Insert new sensor record into the database
            await _insertService.InsertAsync("Measurements.Sensor", dataDict);
        }

        //  9. Return success response
        return Ok($"Inserted, {data.UUID}");
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

    /// <summary>
    /// Fetches the ID of a sensor data record using optional filters.
    /// </summary>
    /// <param name="gatewayId">Required: The ID of the gateway associated with the sensor reading.</param>
    /// <param name="polledAt">Required: The timestamp when the sensor data was recorded.</param>
    /// <returns>
    /// Returns the ID of the matching sensor data record if found; 404 if not found or if parameters are invalid.
    /// </returns>
    [HttpGet("id")]
    public async Task<IActionResult> GetSensorIdByFilters(
        [FromQuery] int? gatewayId,
        [FromQuery] DateTime? polledAt)
    {
        if (!gatewayId.HasValue || gatewayId.Value <= 0)
            return BadRequest("Valid GatewayId is required.");

        if (!polledAt.HasValue)
            return BadRequest("PolledAt timestamp is required.");

        var filters = new Dictionary<string, object>
        {
            ["GatewayId"] = gatewayId.Value,
            ["PolledAt"] = polledAt.Value
        };

        var result = await _sqlGet.FetchAsync(
            tableName: "Measurements.SensorData",
            filters: filters,
            columns: new[] { "Id" }
        );

        if (result == null || !result.Any())
            return NotFound("Sensor data not found.");

        return Ok(new { Id = Convert.ToInt32(result["Id"]) });
    }

    /// <summary>
    /// Processes multiple sensor readings in a single batch operation.
    /// </summary>
    /// <param name="request">Batched sensor data containing gateway UUID and multiple sensor readings.</param>
    /// <returns>
    /// Returns:
    /// <list type="bullet">
    /// <item><description>200 OK with success message if all valid readings were processed successfully.</description></item>
    /// <item><description>400 Bad Request if validation fails (invalid GatewayUUID, empty readings, or invalid sensor data).</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This endpoint processes multiple sensor readings in a single request for improved efficiency.
    /// Each reading is validated individually, and only valid readings are processed.
    /// Invalid readings are skipped and reported in the response.
    ///
    /// The request should contain:
    /// - GatewayUUID: Valid GUID identifying the gateway
    /// - Readings: Collection of sensor data with measurements
    ///
    /// Response includes processing summary with counts of successful and failed operations.
    /// </remarks>
    [HttpPost("batch")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes("application/json")]
    [Produces("application/json")]
    // [Authorize] // Require JWT
    public async Task<IActionResult> PostBatchedSensorData([FromBody] ConnectedToGateway request)
    {
        // Validate request using domain validation service
        var validationResult = _validationService.ValidateBatchedSensorRequest(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(string.Join("; ", validationResult.Errors));
        }

        // Fetch GatewayId from DB using GatewayUUID
        var gatewayRecord = await _sqlGet.FetchAsync("Secrets.Gateway", new Dictionary<string, object>
        {
            { "UUID", request.GatewayUUID }
        });

        if (gatewayRecord == null || !gatewayRecord.Any())
        {
            return BadRequest($"Gateway with UUID {request.GatewayUUID} does not exist.");
        }

        int gatewayId = Convert.ToInt32(gatewayRecord["Id"]);

        var processedCount = 0;
        var skippedCount = 0;
        var errors = new List<string>();

        // Process each sensor and its measurements
        foreach (var sensorData in request.Readings.sensors)
        {
            var sensorId = sensorData.sensor_id;

            // Validate sensor data (including sensor ID)
            var sensorErrors = _validationService.ValidateSensorData(sensorData);
            if (sensorErrors.Any())
            {
                errors.AddRange(sensorErrors);
                skippedCount += sensorData.measurements.Count; // Skip all measurements for this invalid sensor
                continue;
            }

            for (int measurementIndex = 0; measurementIndex < sensorData.measurements.Count; measurementIndex++)
            {
                var measurement = sensorData.measurements[measurementIndex];
                try
                {
                    // Validate individual measurement using domain validation service
                    var measurementErrors = _validationService.ValidateMeasurement(measurement, sensorId, measurementIndex);
                    if (measurementErrors.Any())
                    {
                        errors.AddRange(measurementErrors);
                        skippedCount++;
                        continue;
                    }

                    // Convert timestamp to DateTime
                    var polledAt = DateTimeOffset.FromUnixTimeSeconds(measurement.timestamp).DateTime;

                    // Process the individual sensor reading (reuse existing logic)
                    var sensorDto = new SensorDto
                    {
                        GatewayUUID = request.GatewayUUID,
                        UUID = Guid.NewGuid(), // Generate a UUID for this reading
                        PolledAt = polledAt,
                        TemperatureCel = measurement.temperature_c,
                        HumdityPct = measurement.humidity_pct
                    };

                    // Call the existing single sensor processing logic
                    try
                    {
                        var result = await ProcessSingleSensorReading(sensorDto, gatewayId);
                        if (result.IsSuccess)
                        {
                            processedCount++;
                        }
                        else
                        {
                            errors.Add($"Failed to process sensor {sensorId}: {result.ErrorMessage}");
                            skippedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Error processing sensor {sensorId}: {ex.Message}");
                        skippedCount++;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Error processing sensor {sensorId}: {ex.Message}");
                    skippedCount++;
                }
            }
        }

        // Return response with processing summary
        var response = new
        {
            Message = $"Successfully processed {processedCount} sensor readings",
            ProcessedCount = processedCount,
            SkippedCount = skippedCount,
            Errors = errors
        };

        return Ok(response);
    }

    private async Task<(bool IsSuccess, string ErrorMessage)> ProcessSingleSensorReading(SensorDto data, int gatewayId)
    {
        try
        {
            // Fetch the latest sensor reading for this gateway to compare previous state
            var readings = await _sqlGetAdvanced.FetchWithJoinsAsync<GpsApp.DTO.SensorReading>(
                baseTable: "Measurements.Sensor sensor",
                selectClause: @"
                sensor.TempTimerStart,
                sensor.TempTimeOutside,
                sensor.HumidTimerStart,
                sensor.HumidTimeOutside,
                sensor.TemperatureCel,
                sensor.HumdityPct,
                sensor.PolledAt,
                sensor.TempMinMeasured,
                sensor.TempMaxMeasured,
                sensor.HumidMinMeasured,
                sensor.HumidMaxMeasured
            ",
                joins: new List<string>(),
                filters: new Dictionary<string, object> {
                { "sensor.GatewayId", gatewayId }
                },
                map: r => new GpsApp.DTO.SensorReading
                {
                    TempTimerStart = r["TempTimerStart"] as DateTime?,
                    TempTimeOutside = r["TempTimeOutside"] == DBNull.Value ? 0 : Convert.ToInt32(r["TempTimeOutside"]),
                    HumidTimerStart = r["HumidTimerStart"] as DateTime?,
                    HumidTimeOutside = r["HumidTimeOutside"] == DBNull.Value ? 0 : Convert.ToInt32(r["HumidTimeOutside"]),
                    PolledAt = Convert.ToDateTime(r["PolledAt"]),
                    TempMinMeasured = r["TempMinMeasured"] == DBNull.Value ? data.TemperatureCel.Value : Convert.ToSingle(r["TempMinMeasured"]),
                    TempMaxMeasured = r["TempMaxMeasured"] == DBNull.Value ? data.TemperatureCel.Value : Convert.ToSingle(r["TempMaxMeasured"]),
                    HumidMinMeasured = r["HumidMinMeasured"] == DBNull.Value ? data.HumdityPct.Value : Convert.ToSingle(r["HumidMinMeasured"]),
                    HumidMaxMeasured = r["HumidMaxMeasured"] == DBNull.Value ? data.HumdityPct.Value : Convert.ToSingle(r["HumidMaxMeasured"]),
                }
            );

            // Get the most recent reading
            var lastReading = readings
                .OrderByDescending(r => r.PolledAt)
                .FirstOrDefault();

            // Calculate min/max values
            float tempMinMeasured = Math.Min(data.TemperatureCel.Value, lastReading?.TempMinMeasured ?? data.TemperatureCel.Value);
            float tempMaxMeasured = Math.Max(data.TemperatureCel.Value, lastReading?.TempMaxMeasured ?? data.TemperatureCel.Value);
            float humidMinMeasured = Math.Min(data.HumdityPct.Value, lastReading?.HumidMinMeasured ?? data.HumdityPct.Value);
            float humidMaxMeasured = Math.Max(data.HumdityPct.Value, lastReading?.HumidMaxMeasured ?? data.HumdityPct.Value);

            // Calculate time outside range
            var (tempTimeOutside, tempTimerStart) = TrackingTimeOutsideRange.TrackTimeOutsideRange(
                currentValue: data.TemperatureCel.Value,
                expectedMin: 10,
                expectedMax: 30,
                currentTimestamp: data.PolledAt,
                lastTimerStart: lastReading?.TempTimerStart
            );

            var (humidTimeOutside, humidTimerStart) = TrackingTimeOutsideRange.TrackTimeOutsideRange(
                currentValue: data.HumdityPct.Value,
                expectedMin: 20,
                expectedMax: 80,
                currentTimestamp: data.PolledAt,
                lastTimerStart: lastReading?.HumidTimerStart
            );

            // Prepare data for insertion
            var dataDict = new Dictionary<string, object>
            {
                { "GatewayId", gatewayId },
                { "UUID", data.UUID },
                { "PolledAt", data.PolledAt },
                { "TemperatureCel", data.TemperatureCel },
                { "HumdityPct", data.HumdityPct },
                { "TempTimeOutside", tempTimeOutside },
                { "HumidTimeOutside", humidTimeOutside },
                { "TempTimerStart", tempTimerStart },
                { "HumidTimerStart", humidTimerStart },
                { "TempMinMeasured", tempMinMeasured },
                { "TempMaxMeasured", tempMaxMeasured },
                { "HumidMinMeasured", humidMinMeasured },
                { "HumidMaxMeasured", humidMaxMeasured }
            };

            // Check if record exists
            var existingRecord = await _sqlGet.FetchAsync("Measurements.Sensor", new Dictionary<string, object>
            {
                { "GatewayId", gatewayId },
                { "UUID", data.UUID }
            });

            if (existingRecord != null)
            {
                // Update existing record
                var updateDict = new Dictionary<string, object>
                {
                    { "TemperatureCel", data.TemperatureCel },
                    { "HumdityPct", data.HumdityPct },
                    { "PolledAt", data.PolledAt }
                };

                await _sqlUpdate.UpdateAsync("Measurements.Sensor", updateDict, new Dictionary<string, object>
                {
                    { "Id", existingRecord["Id"] }
                });
            }
            else
            {
                // Insert new record
                await _insertService.InsertAsync("Measurements.Sensor", dataDict);
            }

            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Retrieves sensor readings for a specific delivery with pagination and optional time filtering.
    /// </summary>
    /// <param name="deliveryId">The ID of the delivery to get sensor readings for.</param>
    /// <param name="page">Page number for pagination (default: 1).</param>
    /// <param name="pageSize">Number of records per page (default: 50, max: 100).</param>
    /// <param name="fromDate">Optional start date filter for readings.</param>
    /// <param name="toDate">Optional end date filter for readings.</param>
    /// <returns>
    /// Returns a paginated list of sensor readings for the delivery.
    /// </returns>
    /// <remarks>
    /// This endpoint retrieves all sensor readings associated with a delivery's sensor.
    /// The readings can be filtered by time range and paginated for better performance.
    /// </remarks>
    [HttpGet("delivery/{deliveryId}/readings")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedSensorReadingsResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSensorReadingsForDelivery(
        [FromRoute] int deliveryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        // Validate pagination parameters
        if (page < 1)
            return BadRequest("Page number must be greater than 0.");

        if (pageSize < 1 || pageSize > 100)
            return BadRequest("Page size must be between 1 and 100.");

        // Validate date range
        if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
            return BadRequest("From date cannot be later than to date.");

        // Get user claims for authorization
        var companyId = await _authService.GetCompanyIdFromClaims(User);
        var userId = await _authService.GetUserIdFromClaims(User);
        var role = await _authService.GetUserRoleFromClaims(User);

        if (companyId == null || userId == null || string.IsNullOrEmpty(role))
            return Forbid("Invalid user claims.");

        // First, get the delivery and its associated sensor
        var deliveryQuery = await _sqlGetAdvanced.FetchWithJoinsAsync<Dictionary<string, object>>(
            baseTable: "Orders.Delivery deliv",
            selectClause: @"
                deliv.Id AS DeliveryId,
                deliv.SensorId,
                deliv.RouteId,
                sensor.GatewayId,
                g.UserId,
                g.CompanyId
            ",
            joins: new List<string>
            {
                "JOIN Measurements.Sensor sensor ON deliv.SensorId = sensor.Id",
                "JOIN Secrets.Gateway g ON sensor.GatewayId = g.Id"
            },
            filters: new Dictionary<string, object>
            {
                { "deliv.Id", deliveryId }
            }
        );

        if (!deliveryQuery.Any())
            return NotFound($"Delivery with ID {deliveryId} not found.");

        var delivery = deliveryQuery.First();
        var sensorId = Convert.ToInt32(delivery["SensorId"]);
        var gatewayId = Convert.ToInt32(delivery["GatewayId"]);
        var gatewayUserId = Convert.ToInt32(delivery["UserId"]);
        var gatewayCompanyId = Convert.ToInt32(delivery["CompanyId"]);

        // Check authorization - user must have access to the gateway
        bool canAccess = false;
        if (role == "Admin")
        {
            canAccess = companyId.Value == gatewayCompanyId;
        }
        else
        {
            canAccess = userId.Value == gatewayUserId;
        }

        if (!canAccess)
            return Forbid("You do not have access to this delivery's sensor data.");

        // Build filters for sensor readings query - get all readings for the same gateway and sensor UUID
        var sensorFilters = new Dictionary<string, object>
        {
            { "sensor.GatewayId", gatewayId }
        };

        if (fromDate.HasValue)
        {
            sensorFilters.Add("sensor.PolledAt >= ", fromDate.Value);
        }

        if (toDate.HasValue)
        {
            sensorFilters.Add("sensor.PolledAt <= ", toDate.Value);
        }

        // Get total count for pagination
        var countQuery = await _sqlGetAdvanced.FetchWithJoinsAsync(
            baseTable: "Measurements.Sensor sensor",
            selectClause: "COUNT(*) as TotalCount",
            joins: new List<string>(),
            filters: sensorFilters
        );

        var totalCount = Convert.ToInt32(countQuery.First()["TotalCount"]);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        if (page > totalPages && totalCount > 0)
            return BadRequest($"Page {page} exceeds total pages ({totalPages}).");

        // Calculate OFFSET for pagination
        var offset = (page - 1) * pageSize;

        // Use repository abstraction for paginated sensor readings
        var readings = await _sqlGetAdvanced.GetSensorReadingsPaginatedAsync(
            gatewayId,
            fromDate,
            toDate,
            offset,
            pageSize
        );
        while (await reader.ReadAsync())
        {
            readings.Add(new SensorReadingDto(
                SensorId: reader.GetInt32(reader.GetOrdinal("SensorId")),
                GatewayId: reader.GetInt32(reader.GetOrdinal("GatewayId")),
                PolledAt: reader.GetDateTime(reader.GetOrdinal("PolledAt")),
                TemperatureCel: reader.IsDBNull(reader.GetOrdinal("TemperatureCel")) ? null : (float?)reader.GetDecimal(reader.GetOrdinal("TemperatureCel")),
                HumidityPct: reader.IsDBNull(reader.GetOrdinal("HumdityPct")) ? null : (float?)reader.GetDecimal(reader.GetOrdinal("HumdityPct")),
                TempTimeOutside: reader.GetInt32(reader.GetOrdinal("TempTimeOutside")),
                HumidTimeOutside: reader.GetInt32(reader.GetOrdinal("HumidTimeOutside"))
            ));
        }

        var response = new PaginatedSensorReadingsResponse(
            Readings: readings,
            Page: page,
            PageSize: pageSize,
            TotalCount: totalCount,
            TotalPages: totalPages,
            FromDate: fromDate,
            ToDate: toDate
        );

        return Ok(response);
    }

    /// <summary>
    /// Returns API discovery information for all sensor-related endpoints.
    /// </summary>
    /// <returns>
    /// Returns a comprehensive list of all sensor API endpoints with their descriptions,
    /// parameters, and usage information.
    /// </returns>
    /// <remarks>
    /// This endpoint provides API discoverability for the entire sensor subsystem,
    /// including data ingestion, querying, and delivery-related endpoints.
    /// Useful for API documentation and client integration.
    /// </remarks>
    [HttpGet("api")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiDiscoveryResponse))]
    public IActionResult GetSensorApiInfo()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/sensor";
        var endpoints = new List<ApiEndpointInfo>
        {
            // Data Ingestion Endpoints
            new ApiEndpointInfo(
                Method: "POST",
                Path: $"{baseUrl}",
                Description: "Inserts or updates sensor data for a specified gateway and sensor device. Validates input, checks authorization, and maintains historical min/max values and out-of-range duration tracking.",
                Parameters: new[] { "GatewayUUID (GUID)", "UUID (GUID)", "PolledAt (DateTime)", "TemperatureCel (float)", "HumdityPct (float)" },
                ResponseTypes: new[] { "200 OK", "400 Bad Request", "401 Unauthorized", "403 Forbidden" },
                RequiresAuth: false
            ),
            new ApiEndpointInfo(
                Method: "POST",
                Path: $"{baseUrl}/batch",
                Description: "Processes multiple sensor readings in a single batch operation for improved efficiency. Each reading is validated individually.",
                Parameters: new[] { "GatewayUUID (GUID)", "Readings (BatchedSensorRequest)" },
                ResponseTypes: new[] { "200 OK", "400 Bad Request", "401 Unauthorized", "403 Forbidden" },
                RequiresAuth: false
            ),

            // Query Endpoints
            new ApiEndpointInfo(
                Method: "GET",
                Path: $"{baseUrl}/available",
                Description: "Retrieves all available sensors accessible to the authenticated user. Returns sensor details with current readings.",
                Parameters: new string[0],
                ResponseTypes: new[] { "200 OK", "401 Unauthorized", "403 Forbidden" },
                RequiresAuth: true
            ),
            new ApiEndpointInfo(
                Method: "GET",
                Path: $"{baseUrl}/sensor-temperature",
                Description: "Retrieves sensor temperature data with optional filtering by sensor ID, exact temperature, or temperature range.",
                Parameters: new[] { "id (int, optional)", "temperature (decimal, optional)", "temperatureFrom (decimal, optional)", "temperatureTo (decimal, optional)" },
                ResponseTypes: new[] { "200 OK", "404 Not Found" },
                RequiresAuth: false
            ),
            new ApiEndpointInfo(
                Method: "GET",
                Path: $"{baseUrl}/sensor-humidity",
                Description: "Retrieves sensor humidity data with optional filtering by sensor ID, exact humidity, or humidity range.",
                Parameters: new[] { "id (int, optional)", "humidity (decimal, optional)", "humidityFrom (decimal, optional)", "humidityTo (decimal, optional)" },
                ResponseTypes: new[] { "200 OK", "404 Not Found" },
                RequiresAuth: false
            ),
            new ApiEndpointInfo(
                Method: "GET",
                Path: $"{baseUrl}/id",
                Description: "Fetches the ID of a sensor data record using gateway ID and timestamp filters.",
                Parameters: new[] { "gatewayId (int, required)", "polledAt (DateTime, required)" },
                ResponseTypes: new[] { "200 OK", "400 Bad Request", "404 Not Found" },
                RequiresAuth: false
            ),

            // Delivery-Related Endpoints
            new ApiEndpointInfo(
                Method: "GET",
                Path: $"{baseUrl}/delivery/{{deliveryId}}/readings",
                Description: "Retrieves sensor readings for a specific delivery with pagination and optional time filtering. Returns paginated sensor data associated with the delivery's sensor.",
                Parameters: new[] { "deliveryId (int, required)", "page (int, optional)", "pageSize (int, optional)", "fromDate (DateTime, optional)", "toDate (DateTime, optional)" },
                ResponseTypes: new[] { "200 OK", "400 Bad Request", "401 Unauthorized", "403 Forbidden", "404 Not Found" },
                RequiresAuth: true
            )
        };

        var metadata = new Dictionary<string, string>
        {
            { "description", "GPS Tracking System Sensor API - Manages environmental sensor data collection, monitoring, and historical analysis" },
            { "version", "1.0.0" },
            { "contact", "GPS Tracking System Team" },
            { "documentation", $"{Request.Scheme}://{Request.Host}{Request.PathBase}/swagger" },
            { "healthCheck", $"{Request.Scheme}://{Request.Host}{Request.PathBase}/health" }
        };

        var response = new ApiDiscoveryResponse(
            ApiName: "GPS Sensor API",
            Version: "1.0.0",
            BaseUrl: baseUrl,
            Endpoints: endpoints,
            Metadata: metadata
        );

        return Ok(response);
    }

    /// <summary>
    /// Returns API discovery information specifically for delivery-related sensor endpoints.
    /// </summary>
    /// <returns>
    /// Returns information about sensor endpoints that are related to delivery tracking,
    /// including sensor readings for specific deliveries.
    /// </returns>
    /// <remarks>
    /// This endpoint provides focused API discoverability for delivery-related sensor operations,
    /// useful for logistics and shipment tracking integrations.
    /// </remarks>
    [HttpGet("delivery/api")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiDiscoveryResponse))]
    public IActionResult GetDeliverySensorApiInfo()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/sensor/delivery";
        var endpoints = new List<ApiEndpointInfo>
        {
            new ApiEndpointInfo(
                Method: "GET",
                Path: $"{baseUrl}/{{deliveryId}}/readings",
                Description: "Retrieves paginated sensor readings for a specific delivery. Returns temperature, humidity, and environmental data collected by the sensor associated with the delivery, with optional time filtering and pagination support.",
                Parameters: new[] {
                    "deliveryId (int, required) - The delivery/shipment ID",
                    "page (int, optional, default: 1) - Page number for pagination",
                    "pageSize (int, optional, default: 50, max: 100) - Number of records per page",
                    "fromDate (DateTime, optional) - Start date filter (ISO 8601 format)",
                    "toDate (DateTime, optional) - End date filter (ISO 8601 format)"
                },
                ResponseTypes: new[] {
                    "200 OK - PaginatedSensorReadingsResponse",
                    "400 Bad Request - Invalid parameters",
                    "401 Unauthorized - Missing or invalid authentication",
                    "403 Forbidden - Access denied to delivery data",
                    "404 Not Found - Delivery not found"
                },
                RequiresAuth: true
            )
        };

        var metadata = new Dictionary<string, string>
        {
            { "description", "Delivery Sensor API - Provides sensor readings and environmental monitoring data for shipment tracking" },
            { "version", "1.0.0" },
            { "useCase", "Logistics tracking, environmental monitoring, shipment condition analysis" },
            { "parentApi", $"{Request.Scheme}://{Request.Host}{Request.PathBase}/sensor/api" },
            { "documentation", $"{Request.Scheme}://{Request.Host}{Request.PathBase}/swagger" }
        };

        var response = new ApiDiscoveryResponse(
            ApiName: "Delivery Sensor API",
            Version: "1.0.0",
            BaseUrl: baseUrl,
            Endpoints: endpoints,
            Metadata: metadata
        );

        return Ok(response);
    }

}

