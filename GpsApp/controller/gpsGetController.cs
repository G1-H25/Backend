using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GpsApp.DTO;

[ApiController]
[Route("[controller]")]
public class GpsGetController : ControllerBase
{
    private readonly SqlGet _getService;
    private readonly IAuthorizationService _authService;

    public GpsGetController(SqlGet getService, IAuthorizationService authService)
    {
        _getService = getService;
        _authService = authService;
    }

    [HttpGet]
    [Authorize] // JWT is required
    public async Task<IActionResult> GetGpsData([FromQuery] GpsData data)
    {
        if (string.IsNullOrEmpty(data.DeviceId))
            return BadRequest("Device ID is required.");

        // 1. Get user ID from JWT claims
        var userId = await _authService.GetUserIdFromClaims(User);
        if (userId == null)
            return Unauthorized("User ID not found in token.");

        // 2. Verify device ownership
        var ownsDevice = await _authService.VerifyDeviceOwnership(userId.Value, data.DeviceId);
        if (!ownsDevice)
            return Forbid("You do not have access to this device's data.");

        // 3. Build filters
        var gpsFilters = new Dictionary<string, object> { { "DeviceId", data.DeviceId } };
        if (data.Timestamp.HasValue)
            gpsFilters.Add("Timestamp", data.Timestamp.Value);

        // 4. Fetch GPS data
        var gpsData = await _getService.FetchAsync("dbo.GpsData", gpsFilters);
        if (gpsData == null)
            return NotFound("No matching GPS data found.");

        return Ok(gpsData);
    }
}
