using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using GpsApp.DTO;

[ApiController]
[Route("[controller]")]
public class GpsGetController : ControllerBase
{
    private readonly SqlGet _getService;

    public GpsGetController(SqlGet getService)
    {
        _getService = getService;
    }

    [HttpGet]
    [Authorize] // Require JWT
    public async Task<IActionResult> GetGpsData([FromQuery] GpsData data)
    {
        if (string.IsNullOrEmpty(data.DeviceId))
            return BadRequest("Device ID is required.");

        // 1. Extract user ID from JWT token
        var userIdClaim = User.FindFirst("userId");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized("User ID not found in token.");

        // 2. Confirm device ownership using Gateway table
        var ownership = await _getService.FetchAsync("Secrets.Gateway", new Dictionary<string, object>
        {
            { "Id", data.DeviceId }
        });

        if (ownership == null)
            return NotFound("Device is not registered.");

        var deviceOwnerId = Convert.ToInt32(ownership["UserId"]);
        if (deviceOwnerId != userId)
            return Forbid("You do not have access to this device's data.");

        // 3. Prepare filters for GPS data
        var gpsFilters = new Dictionary<string, object>
        {
            { "DeviceId", data.DeviceId }
        };

        if (data.Timestamp.HasValue)
            gpsFilters.Add("Timestamp", data.Timestamp.Value);

        // 4. Fetch and return GPS data
        var gpsData = await _getService.FetchAsync("dbo.GpsData", gpsFilters);

        if (gpsData == null)
            return NotFound("No matching GPS data found.");

        return Ok(gpsData);
    }
}
