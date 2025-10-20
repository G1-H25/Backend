using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GpsApp.DTO;
using Microsoft.Data.SqlClient;

[ApiController]
[Route("[controller]")]
public class GatewayController : ControllerBase
{
    private readonly SqlInsert _insertService;
    private readonly ISqlGet _getService; // Added for checking ownership

    public GatewayController(SqlInsert insertService, ISqlGet getService)
    {
        _insertService = insertService;
        _getService = getService;
    }

    [HttpPost("register")]
    [Authorize]
    public async Task<IActionResult> RegisterDevice([FromBody] GatewayRequest request)
    {
        var userIdClaim = User.FindFirst("userId");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized("User ID not found in token.");

        if (request.DeviceId.HasValue && request.DeviceId.Value > 0)
        {
            // Step 1: Check existing ownership
            var existing = await _getService.FetchAsync("Secrets.Gateway", new Dictionary<string, object>
            {
                { "Id", request.DeviceId.Value }
            });

            if (existing == null)
                return NotFound("Device not found.");

            var existingUserId = Convert.ToInt32(existing["UserId"]);
            if (existingUserId == userId)
            {
                return Ok(new { message = "Device already registered to this user." });
            }
            else
            {
                // Reassign device to this user
                var updateQuery = "UPDATE Secrets.Gateway SET UserId = @UserId WHERE Id = @DeviceId";

                await using var connection = new SqlConnection(_insertService.ConnectionString);
                await using var command = new SqlCommand(updateQuery, connection);
                command.Parameters.AddWithValue("@DeviceId", request.DeviceId.Value);
                command.Parameters.AddWithValue("@UserId", userId);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                return Ok(new { message = "Device reassigned to this user." });
            }
        }
        else
        {
            // Step 2: Insert new device
            // Insert new device with UserId, but no DeviceId from client
            var newDeviceId = await _insertService.InsertAndReturnIdAsync("Secrets.Gateway", new Dictionary<string, object>
            {
                { "UserId", userId }
            });

            return Ok(new
            {
                message = "Device linked to user successfully.",
                deviceId = newDeviceId
            });
        }
    }


    /// <summary>
    /// Inserts a new gateway record into the database.
    /// </summary>
    /// <param name="request">The gateway insert request containing the required GatewayId, and optional GatewayURL and CurrentLocationId.</param>
    /// <returns>
    /// Returns an <see cref="OkObjectResult"/> if the gateway is inserted successfully,
    /// <see cref="BadRequestObjectResult"/> if the GatewayId is invalid or missing,
    /// or <see cref="StatusCodeResult"/> with status code 500 if a database error occurs.
    /// </returns>
    /// <remarks>
    /// The GatewayId must be provided by the IoT device and must be greater than 0.
    /// GatewayURL and CurrentLocationId can be omitted if not available.
    /// </remarks>
    [HttpPost("insert")]
    public async Task<IActionResult> InsertGateway([FromBody] GatewayInsertRequest request)
    {
        if (request.GatewayId <= 0)
            return BadRequest("DeviceId must be provided by IoT and must be greater than 0.");

        var data = new Dictionary<string, object>
        {
            ["Id"] = request.GatewayId, // From IoT
            ["GatewayURL"] = request.GatewayURL,
            ["CurrentLocationId"] = request.CurrentLocationId
        };

        try
        {
            await _insertService.InsertAsync("Secrets.Gateway", data);
            return Ok("Gateway inserted successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"DB error: {ex.Message}");
        }
    }


}

