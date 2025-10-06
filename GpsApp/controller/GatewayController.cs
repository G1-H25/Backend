using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GpsApp.DTO;
using Microsoft.Data.SqlClient;

[ApiController]
[Route("[controller]")]
public class GatewayController : ControllerBase
{
    private readonly SqlInsert _insertService;
    private readonly SqlGet _getService; // Added for checking ownership

    public GatewayController(SqlInsert insertService, SqlGet getService)
    {
        _insertService = insertService;
        _getService = getService;
    }

    [HttpPost]
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
            await _insertService.InsertAsync("Secrets.Gateway", new Dictionary<string, object>
            {
                { "UserId", userId }
            });

            return Ok(new { message = "Device linked to user successfully." });
        }
    }
}
