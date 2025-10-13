using GpsApp.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;



[ApiController]
[Route("[controller]")]
public class SensorController : ControllerBase
{
    private readonly SqlInsert _insertService;
    private readonly IAuthorizationService _authService;

    // get the connectionstring to azure database, authorization access
    public SensorController(SqlInsert insertService, IAuthorizationService authService)
    {
        _insertService = insertService;
        _authService = authService;
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
        // Extract user info from JWT
        var canAccess = await _authService.UserCanAccessDevice(User, data.GatewayId);
        if (!canAccess)
            return Forbid("You do not have access to this gateway.");

        if (data.GatewayId <= 0)
            return BadRequest("GatewayId must be a positive integer.");
        // if timestamp not provided, will set timestamp
        if (data.PolledAt == default)
        {
            data.PolledAt = DateTime.UtcNow;
        }
        if (data.TemperatureCel == null)
            return BadRequest("TemperatureCel must be provided");
        
        if (data.HumdityPct == null)
            return BadRequest("HumdityPct must be provided");

        var dataDict = new Dictionary<string, object>
        {
            { "GatewayId", data.GatewayId },
            { "PolledAt", data.PolledAt },
            { "TemperatureCel", data.TemperatureCel },
            { "HumdityPct", data.HumdityPct }
        };

        await _insertService.InsertAsync("Measurements.Sensor", dataDict);

        return Ok("Inserted");
    }
}

