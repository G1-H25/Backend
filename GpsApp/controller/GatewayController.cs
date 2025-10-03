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
        if (request.DeviceId <= 0)
            return BadRequest("Invalid device ID.");

        var userIdClaim = User.FindFirst("userId");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized("User ID not found in token.");

        // Step 1: Check existing ownership 
        var existing = await _getService.FetchAsync("Gateway", new Dictionary<string, object>
        {
            { "Id", request.DeviceId }
        });

        if (existing != null)
        {
            var existingUserId = Convert.ToInt32(existing["UserID"]);

            if (existingUserId == userId)
            {
                return Ok(new { message = "Device already registered to this user." });
            }
            else
            {
                // Reassign device to this user
                var updateQuery = "UPDATE Gateway SET UserID = @UserId WHERE Id = @DeviceId";

                await using var connection = new SqlConnection(_insertService.ConnectionString);
                await using var command = new SqlCommand(updateQuery, connection);
                command.Parameters.AddWithValue("@DeviceId", request.DeviceId);
                command.Parameters.AddWithValue("@UserId", userId);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                return Ok(new { message = "Device reassigned to this user." });
            }
        }

        // Step 2: Insert new device
        await _insertService.InsertAsync("Gateway", new Dictionary<string, object>
        {
            { "Id", request.DeviceId },
            { "UserID", userId }
        });

        return Ok(new { message = "Device linked to user successfully." });
    }
}
