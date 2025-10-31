using Microsoft.AspNetCore.Mvc;
using GpsApp.DTO;

/// <summary>
/// Carrier and vehicle management controller using dependency inversion principle.
/// </summary>
/// <remarks>
/// Manages complete carrier registration with multi-step entity creation.
/// Demonstrates SOLID: SRP (carrier management), OCP (interface-based), LSP (substitutable),
/// ISP (focused interfaces), DIP (abstraction over concretions).
/// Creates Registration → Vehicle → Carrier relationship chain.
/// Constructor injection enables testing and loose coupling.
/// </remarks>
[ApiController]
[Route("[controller]")]
public class CurrentLocationHistoryController : ControllerBase
{
    private readonly ISqlInsert _insertService;
    private readonly ISqlGetAdvanced _sqlAdvanced;
    private readonly ISqlGet _sqlGet;
    private readonly IAuthorizationService _authService;
    private readonly ISqlUpdate _sqlUpdate;

    /// <summary>
    /// Controller for managing GPS current location history for gateways.
    /// </summary>
    /// <remarks>
    /// Demonstrates SOLID principles through dependency injection and abstraction of data access.
    /// Handles creation, retrieval, and linking of gateway current location history records.
    /// </remarks>
    public CurrentLocationHistoryController(ISqlInsert insertService, IAuthorizationService authService, ISqlGetAdvanced sqlGetAdvanced, ISqlGet sqlGet, ISqlUpdate sqlUpdate)
    {
        _insertService = insertService;
        _authService = authService;
        _sqlAdvanced = sqlGetAdvanced;
        _sqlGet = sqlGet;
        _sqlUpdate = sqlUpdate;
    }


    /// <summary>
    /// Creates a new current location history record and links it to a gateway.
    /// </summary>
    /// <param name="request">
    /// A <see cref="CurrentLocationHistoryCreate"/> object containing gateway ID, latitude, longitude, and optional timestamp.
    /// </param>
    /// <returns>
    /// Returns an <see cref="OkObjectResult"/> containing the newly created location history ID and associated gateway ID,
    /// or a <see cref="BadRequestResult"/> if validation fails,
    /// or a <see cref="StatusCodeResult"/> if database insertion fails.
    /// </returns>
    /// <remarks>
    /// This endpoint performs a two-step operation:
    /// 1. Inserts a new record into <c>Secrets.LocationHistory</c>.
    /// 2. Updates the corresponding <c>Secrets.Gateway</c> entry to reference the new current location.
    /// 
    /// If <c>PolledAt</c> is not provided, it defaults to UTC time.
    /// </remarks>
    [HttpPost("create")]
    public async Task<IActionResult> CreateCurrentLocationHistory([FromBody] CurrentLocationHistoryCreate request)
    {
        //  Validate inputs
        if (request.GatewayId <= 0)
            return BadRequest("Invalid GatewayId.");

        if (request.PolledAt == default)
            request.PolledAt = DateTime.UtcNow;

        if (request.Latitude == default)
            return BadRequest("Latitude info is incomplete.");

        if (request.Longitude == default)
            return BadRequest("Longitude info is incomplete.");

        //  Step 1: Insert Location History record
        var locationValues = new Dictionary<string, object>
        {
            { "PolledAt", request.PolledAt },
            { "Longitude", request.Longitude.ToString() },
            { "Latitude", request.Latitude.ToString() }
        };

        int locationHistoryId = await _insertService.InsertAndReturnIdAsync("Secrets.LocationHistory", locationValues);

        if (locationHistoryId <= 0)
            return StatusCode(500, "Failed to insert LocationHistory.");

        //  Step 2: Update Gateway to reference the new location
        var updateValues = new Dictionary<string, object>
        {
            { "CurrentLocationId", locationHistoryId }
        };

        var filters = new Dictionary<string, object>
        {
            { "Id", request.GatewayId }
        };

        await _sqlUpdate.UpdateAsync("Secrets.Gateway", updateValues, filters);

        return Ok(new
        {
            message = "CurrentLocationHistory created successfully.",
            locationHistoryId,
            gatewayId = request.GatewayId
        });
    }


    /// <summary>
    /// Retrieves the current location history ID associated with a gateway.
    /// </summary>
    /// <param name="gatewayUuid">Optional unique identifier (UUID) of the gateway.</param>
    /// <param name="gatewayId">Optional numeric ID of the gateway.</param>
    /// <returns>
    /// Returns an <see cref="OkObjectResult"/> containing the gateway reference and current location ID,
    /// or a <see cref="BadRequestResult"/> if no valid identifiers are provided,
    /// or a <see cref="NotFoundResult"/> if the gateway or location record cannot be found.
    /// </returns>
    /// <remarks>
    /// This route queries <c>Secrets.Gateway</c> using either the gateway UUID or ID.
    /// The response includes only the <c>CurrentLocationId</c> value.
    /// Use <c>/current-location</c> instead to fetch full latitude/longitude details.
    /// </remarks>
    [HttpGet("current-locationId")]
    public async Task<IActionResult> GetCurrentLocationHistoryId([FromQuery] Guid? gatewayUuid, [FromQuery] int? gatewayId)
    {
        //  Validate input
        if (!gatewayUuid.HasValue && (!gatewayId.HasValue || gatewayId <= 0))
            return BadRequest("Either Gateway UUID or Gateway ID must be provided.");

        //  Build filters
        var filters = new Dictionary<string, object>();

        if (gatewayUuid.HasValue)
            filters.Add("UUID", gatewayUuid.Value);

        if (gatewayId.HasValue && gatewayId.Value > 0)
            filters.Add("Id", gatewayId.Value);

        //  Fetch the gateway record
        var result = await _sqlGet.FetchAsync(
            tableName: "Secrets.Gateway",
            filters: filters,
            columns: new[] { "CurrentLocationId" }
        );

        if (result == null || result.Count == 0)
            return NotFound("Gateway not found or no current location associated.");

        //  Extract the CurrentLocationId
        int? currentLocationId = result["CurrentLocationId"] as int?;

        if (currentLocationId == null)
            return NotFound("No current location found for this Gateway.");

        //  Return
        return Ok(new
        {
            Gateway = gatewayUuid?.ToString() ?? gatewayId?.ToString(),
            CurrentLocationId = currentLocationId
        });
    }

    /// <summary>
    /// Retrieves the current location details (latitude, longitude, and timestamp) for a gateway.
    /// </summary>
    /// <param name="gatewayUuid">Optional unique identifier (UUID) of the gateway.</param>
    /// <param name="gatewayId">Optional numeric ID of the gateway.</param>
    /// <returns>
    /// Returns an <see cref="OkObjectResult"/> containing the gateway reference and its current location information,
    /// including latitude, longitude, and the <c>PolledAt</c> timestamp.
    /// Returns <see cref="BadRequestResult"/> if no identifiers are provided, or <see cref="NotFoundResult"/> if no record exists.
    /// </returns>
    /// <remarks>
    /// This route performs a two-step lookup:
    /// 1. Fetches <c>CurrentLocationId</c> from <c>Secrets.Gateway</c>.
    /// 2. Retrieves corresponding location data from <c>Secrets.LocationHistory</c>.
    ///
    /// Returns the latest GPS coordinates associated with the specified gateway.
    /// </remarks>
    [HttpGet("current-location")]
    public async Task<IActionResult> GetCurrentLocationHistory([FromQuery] Guid? gatewayUuid, [FromQuery] int? gatewayId)
    {
        //  Validate input
        if (!gatewayUuid.HasValue && (!gatewayId.HasValue || gatewayId <= 0))
            return BadRequest("Either Gateway UUID or Gateway ID must be provided.");

        //  Build filters for Gateway lookup
        var filters = new Dictionary<string, object>();

        if (gatewayUuid.HasValue)
            filters.Add("UUID", gatewayUuid.Value);

        if (gatewayId.HasValue && gatewayId.Value > 0)
            filters.Add("Id", gatewayId.Value);

        //  Fetch the Gateway record to get CurrentLocationId
        var gatewayResult = await _sqlGet.FetchAsync(
            tableName: "Secrets.Gateway",
            filters: filters,
            columns: new[] { "CurrentLocationId" }
        );

        if (gatewayResult == null || gatewayResult.Count == 0)
            return NotFound("Gateway not found or no current location associated.");

        //  Extract CurrentLocationId safely
        if (!gatewayResult.ContainsKey("CurrentLocationId") || gatewayResult["CurrentLocationId"] == DBNull.Value)
            return NotFound("No current location found for this Gateway.");

        int currentLocationId = Convert.ToInt32(gatewayResult["CurrentLocationId"]);

        //  Fetch location history details from Secrets.LocationHistory
        var locationFilters = new Dictionary<string, object>
        {
            { "Id", currentLocationId }
        };

        var locationResult = await _sqlGet.FetchAsync(
            tableName: "Secrets.LocationHistory",
            filters: locationFilters,
            columns: new[] { "Id", "Latitude", "Longitude", "PolledAt" }
        );

        if (locationResult == null || locationResult.Count == 0)
            return NotFound("Location history record not found.");

        //  Return structured response
        return Ok(new
        {
            Gateway = gatewayUuid?.ToString() ?? gatewayId?.ToString(),
            CurrentLocation = new
            {
                Id = Convert.ToInt32(locationResult["Id"]),
                Latitude = locationResult["Latitude"]?.ToString(),
                Longitude = locationResult["Longitude"]?.ToString(),
                PolledAt = locationResult["PolledAt"] is DateTime polledAt ? polledAt.ToString("O") : null
            }
        });
    }


}
