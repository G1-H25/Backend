using Microsoft.AspNetCore.Mvc;
using GpsApp.DTO;

[ApiController]
[Route("[controller]")]
public class ExpectedTempController : ControllerBase
{
    private readonly SqlInsert _insertService;
    private readonly SqlUpdate _sqlUpdate;

    public ExpectedTempController(SqlInsert insertService, SqlUpdate sqlUpdate)
    {
        _insertService = insertService;
        _sqlUpdate = sqlUpdate;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateExpectedTemp([FromBody] ExpectedTempCreateRequest request)
    {
        // Basic validation
        if (string.IsNullOrEmpty(request.Note) || request.Min >= request.Max)
        {
            return BadRequest("Invalid ExpectedTemp data.");
        }

        if (request.SensorId <= 0)
        {
            return BadRequest("Invalid SensorId.");
        }

        var values = new Dictionary<string, object>
        {
            { "Note", request.Note },
            { "Min", request.Min },
            { "Max", request.Max }
        };

        // 1. Insert new ExpectedTemp row and get its ID
        int expectedTempId = await _insertService.InsertAndReturnIdAsync("Measurements.ExpectedTemp", values);

        // 2. Update sensor to point to the new ExpectedTemp
        await _sqlUpdate.UpdateAsync("Measurements.Sensor",
            new Dictionary<string, object> { { "ExpectedTempId", expectedTempId } },
            new Dictionary<string, object> { { "Id", request.SensorId } });

        return Ok(new { message = "ExpectedTemp created and sensor updated", expectedTempId });
    }
}

